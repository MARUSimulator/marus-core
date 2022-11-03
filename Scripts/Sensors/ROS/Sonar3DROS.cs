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
using Marus.Core;
using Marus.Networking;
using Sensorstreaming;
using UnityEngine;
using static Sensorstreaming.SensorStreaming;

namespace Marus.Sensors.Primitive
{
    [RequireComponent(typeof(Sonar3D))]
    public class Sonar3DROS : SensorStreamer<SensorStreamingClient, PointCloudStreamingRequest>
    {
        Sonar3D sensor;
        new void Start()
        {
            sensor = GetComponent<Sonar3D>();
            StreamSensor(sensor,
                streamingClient.StreamSonarSensor);
            base.Start();
        }

        protected override PointCloudStreamingRequest ComposeMessage()
        {
            Sensor.PointCloud _pointCloud = new Sensor.PointCloud();
            foreach (Vector3 point in sensor.pointsCopy)
            {
                var tmp = TfExtensions.Unity2Map(point);
                Geometry.Point p = new Geometry.Point()
                {
                    X = tmp.x,
                    Y = tmp.y,
                    Z = tmp.z
                };
                _pointCloud.Points.Add(p);
            }

            _pointCloud.Header = new Std.Header()
            {
                FrameId = sensor.frameId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000.0
            };

            return new PointCloudStreamingRequest()
            {
                Data = _pointCloud,
                Address = address
            };
        }
    }
}