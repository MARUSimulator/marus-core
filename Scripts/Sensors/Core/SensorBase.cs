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

    public abstract class SensorBase : MonoBehaviour
    {
        protected void Awake()
        {
            SensorSampler.Instance.AddSensorCallback(this, SampleSensor);
        }

        //public bool RunRecording = false;
        
        [Header("Sensor parameters")] 
        public String frameId;

        public float SampleFrequency = 20;

        protected Rigidbody body;

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
                if (body == null)
                {
                    return transform;
                }
                return body?.transform;
            }
        }

        /// <summary>
        /// Set this when there is new data sampled
        /// </summary>
        protected volatile bool hasData = false;
        protected abstract void SampleSensor();

        protected GameObjectLogger Logger;
        protected void Log<W>(W data)
        {
            if (Logger == null)
            {
                Logger = DataLogger.Instance.GetLogger<W>($"{vehicle.name}/{name}");
            }
            (Logger as GameObjectLogger<W>).Log(data);
        }

        void OnEnable()
        {
            SensorSampler.Instance.EnableCallback(this);
        }

        void OnDisable()
        {
            SensorSampler.Instance.DisableCallback(this);
        }

    }

    /// <summary>
    /// Base class that every sensor has to implement
    /// Sensor streams readings to the server defined in RosConnection singleton instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SensorStreamer<T> : MonoBehaviour where T : IMessage
    {

        [Space]
        [Header("Streaming Parameters")]
        public float UpdateFrequency = 1;
        public string address;
        public bool hasData;

        /// <summary>
        /// A client instance used for streaming sensor readings
        /// </summary>
        /// <value></value>
        protected SensorStreaming.SensorStreamingClient streamingClient
        {
            get
            {
                if (!RosConnection.Instance.IsConnected)
                    return null;
                return RosConnection.Instance.GetClient<SensorStreaming.SensorStreamingClient>();
            }
        }

        /// <summary>
        /// Set this in the Awake() method of the sensor script.
        /// Instantiate appropriate client service
        /// </summary>
        AsyncClientStreamingCall<T, StreamingResponse> streamHandle;


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
                return null;
            }
        }

        double cumulativeTime = 0;
        void Update()
        {
            cumulativeTime += Time.deltaTime;
            if (cumulativeTime > (1 / UpdateFrequency))
            {
                cumulativeTime = 0;
                if (hasData && RosConnection.Instance.IsConnected)
                {
                    SendMessage();       
                }
            }
        }

        protected void StreamSensor(AsyncClientStreamingCall<T, StreamingResponse> streamingCall)
        {
            streamHandle = streamingCall;
        }


        protected abstract void SendMessage();


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