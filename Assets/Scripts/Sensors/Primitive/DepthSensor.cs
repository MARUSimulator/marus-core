﻿using System;
using Geometry;
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

        public void Awake()
        {
            streamHandle =  streamingClient.StreamDepthSensor(cancellationToken:RosConnection.Instance.cancellationToken);
            AddSensorCallback(SensorCallbackOrder.Last, Refresh);
            if (string.IsNullOrEmpty(address))
                address = vehicle.name + "/depth";
        }

        public async override void SendMessage()
        {
            var depthOut = new DepthStreamingRequest
            {
                Address = address,
                Data = new PoseWithCovarianceStamped()
                {
                    Header = new Header
                    {
                        FrameId = frameId,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
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

        public void Refresh()
        {
            depth = -transform.position.y;
            Log(new { depth });
            hasData = true;
        }
    }
}
