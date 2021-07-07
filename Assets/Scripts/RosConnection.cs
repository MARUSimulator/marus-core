using System;
using System.Collections.Generic;
using Grpc.Core;
using UnityEngine;
using System.Threading;
using System.Linq;
using Labust.Core;
using static Tf.Tf;
using static Sensorstreaming.SensorStreaming;
using static Remotecontrol.RemoteControl;
using static Ping.Ping;
using static Parameterserver.ParameterServer;
using Parameterserver;
using System.Collections;

namespace Labust.Networking
{

    /// <summary>
    /// Singleton class for configuring and connecting to 
    /// ROS server
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class RosConnection : MonoBehaviour
    {
        public string serverIP = "localhost";

        public int serverPort = 30052;
        public int connectionTimeout = 5;
        public string OriginFrameName = "map";
        public string OriginFrameLatitude = "/d2/LocalOriginLat";
        public string OriginFrameLongitude = "/d2/LocalOriginLon";

        public float DefaultLatitude = 45f;
        public float DefaultLongitude = 15f;
        static RosConnection _instance = null;

        Channel _streamingChannel;
        Dictionary<Type, ClientBase> _grpcClients;
        volatile bool _connected;

        public bool IsConnected => _connected;
        public CancellationToken cancellationToken => _streamingChannel.ShutdownToken;

        private ParameterServerClient _parameterServerClient;

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


        private Transform _mapFrame;
        private GeographicFrame _worldFrame;
        public GeographicFrame WorldFrame => _worldFrame;

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

            StartCoroutine(AfterConnection());
        }


        IEnumerator AfterConnection()
        {
            InitializeFrames();
            while (true)
            {
                if (_connected)
                {
                    OnRosConnected();
                    break;
                }
                yield return null;
            }
        }

        private void InitializeFrames()
        {
            var worldFrame = new GameObject("Earth frame");
            worldFrame.transform.position = Vector3.zero;
            worldFrame.transform.rotation = Quaternion.identity;
            worldFrame.SetActive(false);
            _worldFrame = new GeographicFrame(worldFrame.transform, DefaultLatitude, DefaultLongitude, 0f);

            var mapFrame = new GameObject("Map frame");
            mapFrame.transform.position = Vector3.zero;
            mapFrame.transform.rotation = Quaternion.identity;
            mapFrame.SetActive(false);
            mapFrame.transform.parent = worldFrame.transform;
            _mapFrame = mapFrame.transform;
        }

        void OnRosConnected()
        {
            GetParameterServer();
            GetRosFrames();
        }

        private void GetParameterServer()
        {
            _parameterServerClient = new ParameterServerClient(_streamingChannel);
        }

        public bool TryGetParameter<T>(string name, out T outValue)
        {
            object value = null;
            outValue = default(T);
            var paramValue = _parameterServerClient.GetParameter(new GetParamRequest() { Name = name });
            switch (paramValue.ParameterValueCase)
            {
                case ParamValue.ParameterValueOneofCase.None:
                    return false;
                case ParamValue.ParameterValueOneofCase.ValueBool:
                    value = paramValue.ValueBool;
                    break;
                case ParamValue.ParameterValueOneofCase.ValueInt:
                    value = paramValue.ValueInt;
                    break;
                case ParamValue.ParameterValueOneofCase.ValueDouble:
                    value = paramValue.ValueDouble;
                    break;
                case ParamValue.ParameterValueOneofCase.ValueStr:
                    value = paramValue.ValueStr;
                    break;
            }
            outValue = (T)value;
            return true;
        }

        public bool TrySetParameter<T>(string name, object value)
        {
            var request = new SetParamRequest();
            request.Name = name;
            switch (value)
            {
                case bool t1:
                    request.Value.ValueBool  = t1;
                    break;
                case int t2:
                    request.Value.ValueInt = t2;
                    break;
                case double t3:
                    request.Value.ValueDouble = t3;
                    break;
                case string t4:
                    request.Value.ValueStr = t4;
                    break;
                default:
                    return false;
            }
            _parameterServerClient.SetParameter(request);
            return true;
        }

        private void GetRosFrames()
        {
            double lat, lon;
            if (!TryGetParameter<double>(OriginFrameLatitude, out lat))
            {
                lat = DefaultLatitude;
                TrySetParameter<double>(OriginFrameLatitude, lat);
            }

            if(!TryGetParameter<double>(OriginFrameLongitude, out lon))
            {
                lon = DefaultLongitude;
                TrySetParameter<double>(OriginFrameLongitude, lon);
            }

            var frameClient = GetClient<TfClient>();
            var framesMsg = frameClient.GetAllFrames(new Common.Empty(), cancellationToken:cancellationToken);


            var worldFrameMsg = framesMsg.Frames.FirstOrDefault(tf => tf.FrameId == "earth" && tf.ChildFrameId == OriginFrameName);
            if (worldFrameMsg != null)
            {
                _worldFrame.transform.position = worldFrameMsg.Translation.AsUnity();
                _worldFrame.transform.rotation = worldFrameMsg.Rotation.AsUnity();
                _worldFrame = new GeographicFrame(_worldFrame.transform, (float)lat, (float)lon, 0f);

                _mapFrame.transform.position = Vector3.zero;
                _mapFrame.transform.rotation = Quaternion.identity;

            }
            // TBD : What to do with the rest of the frames ??? 
            // foreach(var tf in framesMsg.Frames)
            // {
            //     if ()
            //     {
            //         worldFrame.transform.position = tf.Translation.AsUnity();
            //         worldFrame.transform.rotation = tf.Rotation.AsUnity();
            //     }
            //     var frame = new GameObject($"{tf.ChildFrameId}_{tf.FrameId}");
            //     frame.transform.parent = worldFrame.transform;
            //     frame.transform.position = tf.Translation.AsUnity();
            //     frame.transform.rotation = tf.Rotation.AsUnity();
            // }
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