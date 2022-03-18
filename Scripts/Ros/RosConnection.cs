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

using System;
using System.Collections.Generic;
using Grpc.Core;
using UnityEngine;
using System.Threading;
using Marus.Core;
using static Tf.Tf;
using static Sensorstreaming.SensorStreaming;
using static Remotecontrol.RemoteControl;
using static Ping.Ping;
using static Simulationcontrol.SimulationControl;
using static Visualization.Visualization;

using System.Collections;
using Simulationcontrol;
using Marus.Utils;
using static Acoustictransmission.AcousticTransmission;

namespace Marus.Networking
{

    /// <summary>
    /// Singleton class for configuring and connecting to 
    /// ROS server
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class RosConnection : Singleton<RosConnection>
    {
        [Header("Server info")]
        public string serverIP = "localhost";
        public int serverPort = 30052;
        [HideInRuntimeInspector]
        public int connectionTimeout = 5;


        [Header("Simulation")]
        public bool DisplayTf = false;

        [HideInRuntimeInspector]
        public bool RealtimeSimulation = true;

        [ConditionalHideInInspector("RealtimeSimulation", true)]
        [HideInRuntimeInspector]
        public float SimulationSpeed = 1;


        [Header("Earth origin frame")]
        public string OriginFrameName = "map";
        public string OriginFrameLatitude = "/d2/LocalOriginLat";
        public string OriginFrameLongitude = "/d2/LocalOriginLon";

        public double DefaultLatitude = 45;
        public double DefaultLongitude = 15;

        Channel _streamingChannel;
        Dictionary<Type, ClientBase> _grpcClients;
        volatile bool _connected;
        public bool IsConnected => _connected;
        public CancellationToken cancellationToken => _streamingChannel.ShutdownToken;

        private SimulationControlClient _simulationController;


        public event Action<Channel> OnConnected;

        /// <summary>
        /// Adds new client of type if it does not currently exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddNewClient<T>() where T : ClientBase
        {
            var t = typeof(T);
            if (_grpcClients.TryGetValue(t, out var clientBase))
                return clientBase as T;

            var client = Activator.CreateInstance(typeof(T), _streamingChannel) as T;
            _grpcClients.Add(t, client);
            return client as T;
        }

        /// <summary>
        /// Get gRPC client of given type.
        /// Clients are shared on the application instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetClient<T>() where T : ClientBase
        {
            if (_grpcClients.TryGetValue(typeof(T), out var client))
                return client as T;
            throw new Exception($"Client of type {typeof(T).Name} does not exist.");
        }

        protected override void Initialize()
        {
            CreateSingletons();
            // init channel and streaming clients
            var options = new List<ChannelOption>();
            options.Add(new ChannelOption(ChannelOptions.MaxSendMessageLength, 1024*1024*100));
            _streamingChannel = new Channel(serverIP, serverPort, ChannelCredentials.Insecure, options);
            InitializeClients();
            var t = new Thread(() =>
            {
                _connected = TryConnect();
                // maybe add what to do after failed connection
                // maybe ping server repeatedly 
            });
            t.Start();

            StartCoroutine(WhileConnectionAwait());
        }
        void CreateSingletons()
        {
            var paramServer = ParamServerHandler.Instance;
            var tfHandler = TfHandler.Instance;
            var timeHandler = TimeHandler.Instance;
            var visualizationRos = VisualizationROS.Instance;
        }

        IEnumerator WhileConnectionAwait()
        {
            while (true)
            {
                if (_connected)
                {
                    OnRosConnected();
                    OnConnected?.Invoke(_streamingChannel);
                    break;
                }
                yield return null;
            }
        }

        void OnRosConnected()
        {
            if (!RealtimeSimulation)
            {
                GetSimulationController();
            }
        }

        void Update()
        {
            if (!RealtimeSimulation && IsConnected)
            {
                StartCoroutine(RosStep());
            }
        }

        IEnumerator RosStep()
        {
            yield return new WaitForEndOfFrame();

            if (_simulationController != null)
            {
                _simulationController.Step(
                    new StepRequest
                    {
                        TotalTimeSecs = TimeHandler.Instance.TotalTimeSecs,
                        TotalTimeNsecs = TimeHandler.Instance.TotalTimeNsecs
                    }
                );
            }
        }



        private void GetSimulationController()
        {
            _simulationController = new SimulationControlClient(_streamingChannel);
            _simulationController.SetStartTime(
                new SetStartTimeRequest
                {
                    TimeSecs = TimeHandler.Instance.StartTimeSecs,
                    TimeNsecs = TimeHandler.Instance.StartTimeNsecs
                }
            );
        }

        /// <summary>
        /// Initialize client registry
        /// </summary>
        private void InitializeClients()
        {
            _grpcClients = new Dictionary<Type, ClientBase>()
            {
                {
                    typeof(SensorStreamingClient),
                    new SensorStreamingClient(_streamingChannel)
                },
                {
                    typeof(RemoteControlClient),
                    new RemoteControlClient(_streamingChannel)
                },
                {
                    typeof(PingClient),
                    new PingClient(_streamingChannel)
                },
                {
                    typeof(TfClient),
                    new TfClient(_streamingChannel)
                },
                {
                    typeof(AcousticTransmissionClient),
                    new AcousticTransmissionClient(_streamingChannel)
                },
                {
                    typeof(VisualizationClient),
                    new VisualizationClient(_streamingChannel)
                }
            };
        }

        /// <summary>
        /// Ping gRPC service to see if connection if established.
        /// </summary>
        /// <returns></returns>
        bool TryConnect()
        {
            // sleep until connected
            Debug.Log("Awaiting connection with ROS Server...");
            var pingClinent = GetClient<Ping.Ping.PingClient>();
            try
            {
                var response = pingClinent.Ping(new Ping.PingMsg(), deadline: DateTime.UtcNow.AddSeconds(connectionTimeout));
                if (response.Value == 1)
                {
                    Debug.Log("Connected to the ROS Server");
                    return true;
                }
            }
            catch (RpcException e)
            {
                Debug.Log($"Could not establish a connection to ROS Server. {e.Message}");
            }
            return false;
        }

        async void OnDisable()
        {
            if (_streamingChannel == null)
            {
                return;
            }

            Debug.Log("Shutting down grpc clients and channel. Await for sucessfull confirmation...");
            var awaitable = _streamingChannel?.ShutdownAsync();


            int time = 0;
            while (!awaitable.IsCompleted && time < 3)
            {
                Thread.Sleep(1000);
                time++;
            }
            if (awaitable.IsCompleted)
            {
                Debug.Log("Shut down of grpc clients and channel successfull.");
                await awaitable;
            }
            else
            {
                awaitable.Dispose();
                Debug.Log("Shut down of grpc clients and channel failed. Some client does not have cancelation token set.");
            }
        }
    }

}