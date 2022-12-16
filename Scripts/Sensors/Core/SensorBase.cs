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
using Marus.ROS;

namespace Marus.Sensors
{
    public abstract class SensorBase : MonoBehaviour
    {
        protected void Awake()
        {
            SensorSampler.Instance.AddSensorCallback(this, SampleSensor);
        }


        [Header("Sensor parameters")]
        [SerializeField]
        string frameId;

        public String FrameId
        {
            get
            {
                var veh = vehicle;
                if (string.IsNullOrEmpty(frameId))
                {
                    frameId = $"veh/{name}";
                }
                return TfHandler.ParseTfName(frameId, veh);
            }
        }

        public float SampleFrequency = 20;

        protected Transform _vehicle;
        [NonSerialized] public bool hasData;

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
    public abstract class SensorStreamer<TClient, TMsg> : MonoBehaviour
        where TClient : ClientBase
        where TMsg : IMessage
    {

        [Space]
        [Header("Streaming Parameters")]
        public float UpdateFrequency = 1;
        public string address;

        protected Transform _vehicle;
        private bool _isMessageSending;

        SensorBase _sensor;

        /// <summary>
        /// A client instance used for streaming sensor readings
        /// </summary>
        /// <value></value>
        protected TClient streamingClient
        {
            get
            {
                return RosConnection.Instance.GetClient<TClient>();
            }
        }

        AsyncClientStreamingCall<TMsg, Std.Empty> streamHandle;


        /// <summary>
        /// Used to write sensor reading messages
        /// </summary>
        /// <value></value>
        protected IClientStreamWriter<TMsg> _streamWriter
        {
            get
            {
                if (streamHandle != null)
                    return streamHandle.RequestStream;
                return null;
            }
        }

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

        #if UNITY_EDITOR
        protected void Reset()
        {
            if(gameObject.GetComponent<TfStreamerROS>() == null)
            {
                gameObject.AddComponent<TfStreamerROS>();
            }
            UpdateVehicle();
        }
        #endif

        public void UpdateVehicle()
        {
            var veh = vehicle;
            // reset address to empty if UpdateVehicle is not called from reset
            address = "";
            // if not same object, add vehicle name prefix to address
            if(veh != transform) address = $"{veh.name}/";

            address = address + gameObject.name;
        }

        double cumulativeTime = 0;
        protected void FixedUpdate()
        {
            TrySendMessage();
        }

        protected void TrySendMessage()
        {

            cumulativeTime += (Time.inFixedTimeStep ? Time.fixedDeltaTime : Time.deltaTime);
            if (cumulativeTime >= (1.0f / UpdateFrequency - 0.0001f))
            {
                cumulativeTime = 0;
                if (_sensor.hasData && RosConnection.Instance.IsConnected)
                {
                    if (!_isMessageSending)
                    {
                        _isMessageSending = true;
                        SendMessage();
                        _sensor.hasData = false;
                        _isMessageSending = false;
                    }
                }
            }
        }

        protected void StreamSensor(SensorBase sensor,
            // AsyncClientStreamingCall<TMsg, Std.Empty> streamingCall
            Func<Grpc.Core.Metadata, System.DateTime?, System.Threading.CancellationToken, AsyncClientStreamingCall<TMsg, Std.Empty>> streamingFn)
        {
            streamHandle = streamingFn(null, null, RosConnection.Instance.CancellationToken);
            _sensor = sensor;
        }

        protected abstract void SendMessage();
    }
}
