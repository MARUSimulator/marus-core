using System;
using Geometry;
using Labust.Core;
using Labust.Networking;
using Labust.Sensors;
using Sensorstreaming;
using Std;
using UnityEngine;
using Quaternion = Geometry.Quaternion;

namespace Labust.Sensors.Primitive
{
    /// <summary>
    /// Depth sensor implementation
    /// </summary>
    public class DepthSensor : SensorBase<DepthStreamingRequest>
    {
        double depth;
        public double covariance;

        public void Start()
        {
            streamHandle =  streamingClient?.StreamDepthSensor(cancellationToken:RosConnection.Instance.cancellationToken);
            if (string.IsNullOrEmpty(address))
                address = vehicle.name + "/depth";
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
                        FrameId = frameId,
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
            hasData = false;
        }

        protected override void SampleSensor()
        {
            depth = -transform.position.y;
            Log(new { depth });
            hasData = true;
        }
    }
}
