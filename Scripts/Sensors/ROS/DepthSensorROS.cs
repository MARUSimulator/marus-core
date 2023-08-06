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

using Geometry;
using Marus.Core;
using Marus.Networking;
using Marus.Sensors.Primitive;
using Sensorstreaming;
using Std;
using UnityEngine;
using static Sensorstreaming.SensorStreaming;
using Quaternion = Geometry.Quaternion;

namespace Marus.Sensors.ROS
{
    [RequireComponent(typeof(DepthSensor))]
    public class DepthSensorROS : SensorStreamer<SensorStreamingClient, DepthStreamingRequest>
    {
        double depth;
        public double covariance;
        DepthSensor sensor;


        new public void Start()
        {
            sensor = GetComponent<DepthSensor>();
            StreamSensor(sensor,
                streamingClient.StreamDepthSensor);
            base.Start();
        }


        protected override DepthStreamingRequest ComposeMessage()
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
                                Z = - (sensor.depth) //depth
                            },
                            //Orientation = new Quaternion() { }
                        }
                    }
                }
            };
            var covOut = new double[36];
            covOut[15] = covariance;
            depthOut.Data.Pose.Covariance.AddRange(covOut);
            return depthOut;
        }
    }
}
