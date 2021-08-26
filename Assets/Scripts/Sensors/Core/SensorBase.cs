using UnityEngine;
using UnityEngine.Rendering;
using Sensorstreaming;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Labust.Networking;
using Labust.Logger;

namespace Labust.Sensors
{
    /// <summary>
    /// Base class that every sensor has to implement
    /// Sensor streams readings to the server defined in RosConnection singleton instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SensorBase<T> : MonoBehaviour where T : IMessage
    {
        [Space]
        [Header("Streaming Parameters")]
        public float SensorUpdateHz = 1;
        public string address;
        //public bool RunRecording = false;

        protected Rigidbody body;
        /// <summary>
        /// Vehichle that sensor is attached to
        /// Gets rigid body component of a first ancestor
        /// </summary>
        /// <value></value>
        public Transform vehicle
        {
            get
            {
                if (body == null)
                {
                    var component = GetComponent<Rigidbody>();
                    if (component != null)
                        body = component;
                    else
                        body = Utils.Helpers.GetParentRigidBody(transform);
                }
                return body.transform;
            }
        }

        public enum SensorCallbackOrder
        {
            First,
            Last
        };

        /// <summary>
        /// Data class for sensor callback definition
        /// </summary>
        private class SensorCallback
        {
            // callback for sensor update
            public Action callback;
            // callback for sensor update wrapped inside function with different prototype
            public Action<ScriptableRenderContext, Camera[]> callbackWrapped;
            public SensorCallbackOrder executionOrder;
        };

        /// <summary>
        /// Set this when there is new data sampled
        /// </summary>
        protected volatile bool hasData = false;


        /// <summary>
        /// A client instance used for streaming sensor readings
        /// </summary>
        /// <value></value>
        protected SensorStreaming.SensorStreamingClient streamingClient
        {
            get
            {
                if (RosConnection.Instance == null)
                    throw new System.Exception("Ros connection not set!");
                return RosConnection.Instance.GetClient<SensorStreaming.SensorStreamingClient>();
            }
        }

        /// <summary>
        /// Set this in the Awake() method of the sensor script.
        /// Instantiate appropriate client service
        /// </summary>
        protected AsyncClientStreamingCall<T, StreamingResponse> streamHandle;

        // TODO: we have to see what to log
        private GameObjectLogger Logger;

        /// <summary>
        /// Used to write sensor reading messages
        /// </summary>
        /// <value></value>
        protected IClientStreamWriter<T> _streamWriter 
        {
            get
            {
                if (streamHandle != null)
                    return streamHandle.RequestStream;
                throw new System.Exception("Stream handle not set. Call streamingClient.Stream<rpc-name>() method first!");
            }
        }

        // public abstract T LastSampledValue { get; }


        protected void Log<W>(W data)
        {
            if (Logger == null)
            {
                Logger = DataLogger.Instance.GetLogger<W>(address);
            }
            (Logger as GameObjectLogger<W>).Log(data);
        }

        List<SensorCallback> _sensorCallbackList = new List<SensorCallback>();
        List<SensorCallback> _activeSensorCallbackList = new List<SensorCallback>();
        // only those are being invoked

        double cumulativeTime = 0;
        void Update()
        {
            cumulativeTime += Time.deltaTime;
            if (cumulativeTime > (1 / SensorUpdateHz))
            {
                cumulativeTime = 0;
                if (hasData && RosConnection.Instance.IsConnected)
                {
                    SendMessage();       
                }
            }
        }

        public void AddSensorCallback(SensorCallbackOrder order, Action callback)
        {
            // wrap callback to prototype wanted by the beginFrameRenering event
            Action<ScriptableRenderContext, Camera[]> wrapper = (p1, p2) => 
            {
                callback();
            };
            
            var sensorCallback = new SensorCallback 
            {
                callback = callback,
                callbackWrapped = wrapper, 
                executionOrder = order
            };
            AddSensorCallback(sensorCallback);
            _sensorCallbackList.Add(sensorCallback);
        }

        public void RemoveSensorCallbacks(Action callback)
        {
            var sensorCallback = _sensorCallbackList.FirstOrDefault(x => x.callback == callback);
            if (sensorCallback == null)
                return;
            RemoveSensorCallback(sensorCallback);
        }

        public void OnEnable()
        {
            foreach(var cb in _sensorCallbackList)
                AddSensorCallback(cb);
        }

        public void OnDisable()
        {
            foreach(var cb in _sensorCallbackList)
                RemoveSensorCallback(cb);
        }

        public abstract void SendMessage();

        private void AddSensorCallback(SensorCallback callback)
        {
            if (_activeSensorCallbackList.Contains(callback))
                return;

            _activeSensorCallbackList.Add(callback);
            if(callback.executionOrder == SensorCallbackOrder.First)
            {
                RenderPipelineManager.beginFrameRendering += callback.callbackWrapped;
            }
            else if(callback.executionOrder == SensorCallbackOrder.Last)
            {
                RenderPipelineManager.endFrameRendering += callback.callbackWrapped;
            }
        }
        private void RemoveSensorCallback(SensorCallback callback)
        {
            if (callback.executionOrder == SensorCallbackOrder.First)
            {
                RenderPipelineManager.beginFrameRendering -= callback.callbackWrapped;
            }
            else if (callback.executionOrder == SensorCallbackOrder.Last)
            {
                RenderPipelineManager.endFrameRendering -= callback.callbackWrapped;
            }
            _activeSensorCallbackList.Remove(callback);
        }

        // public static SensorBase[] GetActiveSensors()
        // {
        //     var _sensors = GameObject.FindObjectsOfType<SensorBase>();
        //     return _sensors;
        // }


        // External Helper functions


        // public static void ResetSensorTime(SensorBase[] sensors)
        // {
        //     foreach (Sensor sensor in sensors)
        //     {
        //         sensor.nextOSPtime = 0;
        //         sensor.OSPtime = sensor.nextOSPtime;
        //     }
        // }

        // public static bool SensorTimeUpdated(Sensor[] sensors)
        // {
        //     bool allSensorsUpdated = true;
        //     foreach (Sensor sensor in sensors)
        //     {
        //         allSensorsUpdated = allSensorsUpdated && (sensor.OSPtime == sensor.nextOSPtime);
        //     }
        //     return allSensorsUpdated;
        // }

        // public static void UpdateSensorTime(double newTime, Sensor[] sensors)
        // {
        //     foreach (Sensor sensor in sensors)
        //     {
        //         sensor.nextOSPtime = newTime;
        //     }
        // }

    }

}