// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using Sensorstreaming;
using Google.Protobuf;
using Grpc.Core;
using System;
using Marus.Networking;
using Marus.Logger;
using Marus.Utils;

namespace Marus.Sensors
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

        protected Transform _vehicle;



        public Transform vehicle
        {
            get
            {
                _vehicle = Helpers.GetVehicle(transform);
                if (_vehicle == null)
                {
                    Debug.Log($@"Cannot get vehicle from sensor {transform.name}. 
                        Using sensor as the vehicle transform");
                    return transform;
                }
                return _vehicle;
            }
        }

        /// <summary>
        /// Set this when there is new data sampled
        /// </summary>
        [NonSerialized] public volatile bool hasData = false;
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
        [NonSerialized] public bool hasData;

        /// <summary>
        /// A client instance used for streaming sensor readings
        /// </summary>
        /// <value></value>
        protected SensorStreaming.SensorStreamingClient streamingClient
        {
            get
            {
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
        protected void Update()
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
    }
}
