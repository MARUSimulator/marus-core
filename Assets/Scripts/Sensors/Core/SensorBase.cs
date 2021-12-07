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

        [Space]
        [Header("Streaming Parameters")]
        public float SensorUpdateHz = 1;
        public string address;
        //public bool RunRecording = false;
        
        [Header("Sensor parameters")] 
        public String frameId;

        protected Rigidbody body;

        protected abstract void SampleSensor();

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
                return body?.transform;
            }
        }
        /// <summary>
        /// Set this when there is new data sampled
        /// </summary>
        protected volatile bool hasData = false;

        protected GameObjectLogger Logger;
        protected void Log<W>(W data)
        {
            if (Logger == null)
            {
                Logger = DataLogger.Instance.GetLogger<W>(address);
            }
            (Logger as GameObjectLogger<W>).Log(data);
        }

        protected void AddSensorCallback(Action callback)
        {
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
    public abstract class SensorBase<T> : SensorBase where T : IMessage
    {

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
        protected AsyncClientStreamingCall<T, StreamingResponse> streamHandle;


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
            if (cumulativeTime > (1 / SensorUpdateHz))
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