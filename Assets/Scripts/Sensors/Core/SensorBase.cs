using UnityEngine;
using UnityEngine.Rendering;
using Sensorstreaming;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Labust.Sensors
{
    public abstract class SensorBase<T> : MonoBehaviour where T : IMessage
    {
        [Space]
        [Header("Streaming Parameters")]
        public int SensorUpdateHz = 1;

        //public bool RunRecording = false;

        public enum SensorCallbackOrder
        {
            First,
            Last
        };

        private class SensorCallback
        {
            // callback for sensor update
            public Action callback;
            // callback for sensor update wrapped inside function with different prototype
            public Action<ScriptableRenderContext, Camera[]> callbackWrapped;
            public SensorCallbackOrder executionOrder;
        };

        protected volatile bool hasData = false;
        protected SensorStreaming.SensorStreamingClient streamingClient
        {
            get
            {
                if (RosConnection.Instance == null)
                    throw new System.Exception("Ros connection not set!");
                return RosConnection.Instance.GetClient<SensorStreaming.SensorStreamingClient>();
            }
        }
        protected AsyncClientStreamingCall<T, StreamingResponse> streamHandle;
        protected IClientStreamWriter<T> streamWriter 
        {
            get
            {
                if (streamHandle != null)
                    return streamHandle.RequestStream;
                throw new System.Exception("Stream handle not set. Call streamingClient.Stream<rpc-name>() method first!");
            }
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
            Action<ScriptableRenderContext, Camera[]> wrapper = (p1, p2) => callback();
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