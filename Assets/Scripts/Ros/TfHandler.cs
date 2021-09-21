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
using static Simulatoncontrol.SimulationControl;

using Parameterserver;
using System.Collections;
using Simulatoncontrol;
using Labust.Utils;
using Labust.Visualization;
using Tf;

namespace Labust.Networking
{

    /// <summary>
    /// Singleton class for configuring and connecting to 
    /// ROS server
    /// </summary>
    public class TfHandler : Singleton<TfHandler>
    {

        private Transform _mapFrame;
        private GameObject _tf;
        private GeographicFrame _originGeoFrame;
        public GeographicFrame OriginGeoFrame => _originGeoFrame;

        public TfFrameList FrameList;

        public bool StreamFrames = true;
        AsyncServerStreamingCall<TfFrameList> _frameStream;

        ServerStreamer<TfFrameList> _serverStreamer;

        protected override void Initialize()
        {
            _serverStreamer = new ServerStreamer<TfFrameList>();
            transform.parent = RosConnection.Instance.transform;
            InitializeTf();
            RosConnection.Instance.OnConnected += OnConnected;
        }


        public void OnConnected(Channel channel)
        {
            SetGeoOriginAndMap();
        }

        public void SetGeoOriginAndMap()
        {
            double lat, lon;
            var rosConn = RosConnection.Instance;
            var originFrameLatitude = rosConn.OriginFrameLatitude;
            var originFrameLongitude = rosConn.OriginFrameLongitude;
            var originFrameName = rosConn.OriginFrameName;

            var paramServer = ParamServerHandler.Instance;
            bool latLonRetreived = true;
            if (!paramServer.TryGetParameter<double>(originFrameLatitude, out lat))
            {
                latLonRetreived &= true;
                lat = _originGeoFrame.origin.latitude;
                paramServer.TrySetParameter<double>(originFrameLatitude, lat);
            }

            if(!paramServer.TryGetParameter<double>(originFrameLongitude, out lon))
            {
                latLonRetreived &= true;
                lon = _originGeoFrame.origin.longitude;
                paramServer.TrySetParameter<double>(originFrameLongitude, lon);
            }

            if (latLonRetreived)
            {
                _originGeoFrame = new GeographicFrame(_mapFrame.transform, lat, lon, 0);
            }

            // var frameClient = rosConn.GetClient<TfClient>();
            // var framesMsg = frameClient.GetAllFrames(new Std.Empty(), cancellationToken:rosConn.cancellationToken);

            // UpdateTf(framesMsg);
        }

        public void GetTfFrameStream()
        {
        }

        void Update()
        {
            var rosConn = RosConnection.Instance;
            if (StreamFrames)
            {
                if (!_serverStreamer.IsStreaming)
                {
                    var frameClient = rosConn.GetClient<TfClient>();
                    var frameStream = frameClient.StreamAllFrames(new Std.Empty(), 
                            cancellationToken:rosConn.cancellationToken);
                    _serverStreamer.StartStream(frameStream);
                }
                _serverStreamer.HandleNewMessages(UpdateTf);
            }
        }

        private void UpdateTf(TfFrameList frameList)
        {
            var rosConn = RosConnection.Instance;
            foreach (var frame in frameList.Frames)
            {
                if (frame.ChildFrameId == rosConn.OriginFrameName) 
                    continue;
                
                var frameObj = GetOrCreateGameObjectForFrame(frame);

                // set parent if it is not already set
                if (frameObj.transform.parent == null 
                    || frameObj.transform.parent.name != frame.FrameId)
                {
                    var parentFrame = frameList.Frames.FirstOrDefault(x => x.ChildFrameId == frame.FrameId);
                    if (parentFrame != null)
                    {
                        var parentFrameObj = GetOrCreateGameObjectForFrame(parentFrame);
                        frameObj.transform.parent = parentFrameObj.transform;
                    }
                }

                var position = frame.Translation.AsUnity().Map2Unity();
                var rotation = frame.Rotation.AsUnity().Map2Unity();
                frameObj.transform.localPosition = position;
                frameObj.transform.localRotation = rotation;
            }
        }

        private void InitializeTf()
        {
            _tf = new GameObject("tf");
            _tf.transform.parent = Visualizer.Instance.transform;
            var mapFrame = new GameObject("map");
            mapFrame.transform.position = Vector3.zero;
            mapFrame.transform.rotation = Quaternion.identity;
            // mapFrame.SetActive(false);
            mapFrame.transform.parent = _tf.transform;
            _mapFrame = mapFrame.transform;
            Visualizer.Instance.AddTransform(_mapFrame, "tf");
            var defaultLatitude = RosConnection.Instance.DefaultLatitude;
            var defaultLongitude = RosConnection.Instance.DefaultLongitude;
            _originGeoFrame = new GeographicFrame(mapFrame.transform, defaultLatitude, defaultLongitude, 0f);
        }

        private GameObject GetOrCreateGameObjectForFrame(TfFrame frame)
        {
            var obj = Helpers.FindGameObjectInChildren(frame.ChildFrameId, _tf);
            if (obj == null)
            {
                obj = new GameObject(frame.ChildFrameId);
                obj.transform.parent = _tf.transform;
                Visualizer.Instance.AddTransform(obj.transform, "tf");
            }
            return obj;
        }

    }

}