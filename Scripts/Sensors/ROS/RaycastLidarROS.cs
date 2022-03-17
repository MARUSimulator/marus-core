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

using Labust.Networking;
using UnityEngine;
using Sensorstreaming;
using Labust.Core;
using Sensor;
using Unity.Collections;

namespace Labust.Sensors
{

    /// <summary>
    /// Lidar that cast N rays evenly distributed in configured field of view.
    /// Implemented using IJobParallelFor on CPU
    /// Can drop performance
    /// </summary>
    [RequireComponent(typeof(RaycastLidar))]
    public class RaycastLidarROS : SensorStreamer<PointCloudStreamingRequest>
    {
        RaycastLidar sensor;

        void Start()
        {
            sensor = GetComponent<RaycastLidar>();
            UpdateFrequency = Mathf.Min(UpdateFrequency, sensor.SampleFrequency);
            if (string.IsNullOrEmpty(address))
                address = transform.name + "/lidar";
            StreamSensor(streamingClient?.StreamLidarSensor(cancellationToken: RosConnection.Instance.cancellationToken));
        }

        new void Update()
        {
            hasData = sensor.hasData;
            base.Update();
        }

        private static PointCloud GeneratePointCloud(NativeArray<Vector3> pointcloud)
        {
            PointCloud _pointCloud = new PointCloud();
            foreach (Vector3 point in pointcloud)
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
                FrameId = RosConnection.Instance.OriginFrameName,
                Timestamp = TimeHandler.Instance.TimeDouble
            };

            return _pointCloud;
        }

        protected async override void SendMessage()
        {
            PointCloud _pointCloud = GeneratePointCloud(sensor.pointsCopy);
            var msg = new PointCloudStreamingRequest()
            {
                Data = _pointCloud,
                Address = address
            };
            await _streamWriter.WriteAsync(msg);
            hasData = false;
        }
    }
}
