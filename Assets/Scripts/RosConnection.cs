using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Grpc.Core;
using Sensorstreaming;
using Remotecontrol;
using UnityEngine;
using System.Threading;


namespace Labust.Networking
{

    /// <summary>
    /// Singleton class for configuring and connecting to 
    /// ROS server
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class RosConnection : MonoBehaviour
    {
        public string serverIP = "172.19.126.65";

        public int serverPort = 30052;
        public int connectionTimeout = 5;
        static RosConnection _instance = null;

        Channel _streamingChannel;
        Dictionary<Type, ClientBase> _grpcClients;
        volatile bool _connected;

        public bool IsConnected => _connected;
        public CancellationToken cancellationToken => _streamingChannel.ShutdownToken;

        /// <summary>
        /// RosConnection Instance Singleton
        /// </summary>
        /// <value></value>
        public static RosConnection Instance
        {
            get
            {
                return _instance;
            }
        }

        private void Awake()
        {
            // if the singleton hasn't been initialized yet
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }

            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            // init channel and streaming clients
            _streamingChannel = new Channel(serverIP, serverPort, ChannelCredentials.Insecure);
            InitializeClients();
            var t = new Thread(() =>
            {
                _connected = WaitForConnection();
            // maybe add what to do after failed connection
            // maybe ping server repeatedly 
        });
            t.Start();
        }

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


        /// <summary>
        /// Initialize client registry
        /// </summary>
        private void InitializeClients()
        {
            _grpcClients = new Dictionary<Type, ClientBase>()
        {
            {
                typeof(SensorStreaming.SensorStreamingClient),
                new SensorStreaming.SensorStreamingClient(_streamingChannel)
            },
            {
                typeof(RemoteControl.RemoteControlClient),
                new RemoteControl.RemoteControlClient(_streamingChannel)
            },
            {
                typeof(Ping.Ping.PingClient),
                new Ping.Ping.PingClient(_streamingChannel)
            }
        };
        }

        /// <summary>
        /// Ping gRPC service to see if connection if established.
        /// </summary>
        /// <returns></returns>
        bool WaitForConnection()
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
            Debug.Log("Shutting down grpc clients and channel. Await for sucessfull confirmation...");
            _grpcClients.Clear();
            var awaitable = _streamingChannel.ShutdownAsync();

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