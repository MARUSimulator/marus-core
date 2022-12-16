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
    [RequireComponent(typeof(Sonar2D))]
    public class Sonar2DROS : SensorStreamer<SensorStreamingClient, PointCloudStreamingRequest>
    {
        Sonar2D sensor;
        void Start()
        {
            sensor = GetComponent<Sonar2D>();
            if (string.IsNullOrEmpty(address))
                address = transform.name + "/sonar3d";
            StreamSensor(sensor, 
                streamingClient.StreamPointCloud);
        }

        protected async override void SendMessage()
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
                FrameId = sensor.FrameId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000.0
            };

            var msg = new PointCloudStreamingRequest()
            {
                Data = _pointCloud,
                Address = address
            };
            await _streamWriter.WriteAsync(msg);
        }
    }
}