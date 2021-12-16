
using Geometry;
using Labust.Core;
using Labust.Networking;
using Labust.Sensors.Primitive;
using Sensorstreaming;
using Std;
using UnityEngine;
using Quaternion = Geometry.Quaternion;

namespace Labust.Sensors.ROS
{
    [RequireComponent(typeof(DepthSensor))]
    public class DepthSensorROS : SensorStreamer<DepthStreamingRequest>
    {
        double depth;
        public double covariance;
        DepthSensor sensor;

        public void Start()
        {
            var sensor = GetComponent<DepthSensor>();
            if (string.IsNullOrEmpty(address))
                address = sensor.vehicle.name + "/depth";
            StreamSensor(streamingClient?.StreamDepthSensor(cancellationToken:RosConnection.Instance.cancellationToken));
        }

        void Update()
        {
            hasData = sensor.hasData;
            base.Update();
        }

        protected async override void SendMessage()
        {
            var depthOut = new DepthStreamingRequest
            {
                Address = address,
                Data = new PoseWithCovarianceStamped()
                {
                    Header = new Header
                    {
                        FrameId = sensor.frameId,
                        Timestamp = TimeHandler.Instance.TimeDouble
                    },
                    Pose = new PoseWithCovariance
                    {
                        Pose = new Geometry.Pose
                        {
                            Position = new Point()
                            {
                                X = 0,
                                Y = 0,
                                Z = depth
                            },
                            Orientation = new Quaternion() { }
                        }
                    }
                }
            };
            var covOut = new double[36];
            covOut[15] = covariance;
            depthOut.Data.Pose.Covariance.AddRange(covOut);
            await _streamWriter.WriteAsync(depthOut);
        }
    }
}