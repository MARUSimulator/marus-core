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
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Marus.Sensors
{

    /// <summary>
    /// Base class that every sensor has to implement
    /// Sensor streams readings to the server defined in RosConnection singleton instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DefaultExecutionOrder(200)]
    public abstract class SensorStreamer<TClient, TMsg> : MonoBehaviour
        where TClient : ClientBase
        where TMsg : IMessage
    {

        [Space]
        [Header("Streaming Parameters")]
        public float UpdateFrequency = 1;
        public string address;

        public int MessageQueueSize = 10000;

        protected Transform _vehicle;

        SensorBase _sensor;

        Task _awaitable;
        double _prevMsgTime;

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

        volatile bool _killSendMsgsThread;
        Thread _sendMsgThread;
        ConcurrentQueue<TMsg> _msgQueue;


        /// <summary>
        /// Used to write sensor reading messages
        /// </summary>
        /// <value></value>
        private IClientStreamWriter<TMsg> _streamWriter
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

        public void Start()
        {
            if (_sendMsgThread == null)
            {
                _msgQueue = new ConcurrentQueue<TMsg>();
                _sendMsgThread = new Thread(SendMessagesThread) { IsBackground = false };
                _sendMsgThread.Priority = System.Threading.ThreadPriority.Highest;
                _killSendMsgsThread = false;
                _sendMsgThread.Start();
            }
        }

        protected void FixedUpdate()
        {
            SendMessage();
        }

        void SendMessage()
        {
            // if not connected, do not send
            if (!RosConnection.Instance.IsConnected)
                return;

            var dt = 1.0f / UpdateFrequency;
            var time = Time.fixedTimeAsDouble;
            // calculate when msg has to be sent,
            // and send when enough time passes
            var nextMsgTime = _prevMsgTime + dt;
            if (time >= nextMsgTime)
            {
                var msg = ComposeMessage();
                _prevMsgTime = nextMsgTime;
                if (_msgQueue.Count == MessageQueueSize)
                {
                    // remove first element
                    _msgQueue.TryDequeue(out _);
                    Debug.Log($"Grpc Message overflow in {_sensor.name}");
                }
                _msgQueue.Enqueue(msg);
            }
        }

        private async void SendMessagesThread()
        {
            int count = 0;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (true)
            {
                if (_killSendMsgsThread)
                {
                    return;
                }

                while (_msgQueue.TryDequeue(out var msg))
                {
                    await _streamWriter.WriteAsync(msg);
                    count++;
                }
                var dt = 1000.0 / UpdateFrequency; // in ms
                var sleepTime = dt - sw.ElapsedMilliseconds;
                if (sleepTime > 1) // minimum is 1 miliseconds
                {
                    Thread.Sleep((int)sleepTime);
                }

                if (sw.ElapsedMilliseconds > 5000)
                {
                    sw.Restart();
                    // check real frequency
                    // Debug.Log($"{Time.fixedTimeAsDouble}");
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

        private void OnDisable()
        {
            _killSendMsgsThread = true;
            _sendMsgThread.Join();
            _sendMsgThread = null;
        }

        protected abstract TMsg ComposeMessage();
    }
}
