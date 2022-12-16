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
using Auv;
using Marus.Networking;
using Sensorstreaming;
using UnityEngine;
using Marus.Core;
using Std;
using static Sensorstreaming.SensorStreaming;
using Geometry;

namespace Marus.Sensors.Primitive
{
    /// <summary>
    /// Pose sensor implementation
    /// </summary>
    [RequireComponent(typeof(PoseSensor))]
    public class PoseSensorROS : SensorStreamer<SensorStreamingClient, PoseStreamingRequest>
    {
        PoseSensor sensor;

        void Start()
        {
            sensor = GetComponent<PoseSensor>();
            if (string.IsNullOrEmpty(address))
                address = sensor.vehicle.name + "/pose";
            StreamSensor(sensor,
                streamingClient.StreamPoseSensor);
        }

        protected async override void SendMessage()
        {
            var orientation = sensor.orientation.Unity2Map();
            var toEnu = sensor.position.Unity2Map();
            var linVel = sensor.linearVelocity.Unity2Body();
            var angVel = sensor.angularVelocity.Unity2Body();
            await _streamWriter.WriteAsync(new PoseStreamingRequest
            {
                Address = address,
                Data = new PoseWithCovarianceStamped
                {
                    Header = new Header
                    {
                        FrameId = sensor.FrameId,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000.0
                    },
                    Pose = new PoseWithCovariance
                    {
                        Pose = new Geometry.Pose()
                        {
                            Position = new Point
                            {
                                X = toEnu.x,
                                Y = toEnu.y,
                                Z = toEnu.z

                            },
                            Orientation = new Geometry.Quaternion
                            {
                                W = orientation.w,
                                X = orientation.x,
                                Y = orientation.y,
                                Z = orientation.z
                            }
                        }
                    }
                }
            });
        }
    }
}
