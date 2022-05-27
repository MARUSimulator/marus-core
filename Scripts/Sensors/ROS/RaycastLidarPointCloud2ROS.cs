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

using Marus.Networking;
using UnityEngine;
using Sensorstreaming;
using Marus.Core;
using Sensor;
using System.Collections.Generic;
using Unity.Collections;
using System;
using System.Linq;
using static Sensorstreaming.SensorStreaming;

namespace Marus.Sensors
{

    /// <summary>
    /// Lidar that cast N rays evenly distributed in configured field of view.
    /// Implemented using IJobParallelFor on CPU
    /// Can drop performance
    /// </summary>
    [RequireComponent(typeof(RaycastLidar))]
    public class RaycastLidarPointCloud2ROS : SensorStreamer<SensorStreamingClient, PointCloud2StreamingRequest>
    {

        public bool PublishIntensityAndRing = false;
        RaycastLidar sensor;

        void Start()
        {
            sensor = GetComponent<RaycastLidar>();
            UpdateFrequency = Mathf.Min(UpdateFrequency, sensor.SampleFrequency);
            if (string.IsNullOrEmpty(address))
                address = $"{sensor.vehicle?.name}/lidar";
            StreamSensor(sensor, 
                streamingClient.StreamPointCloud2);
        }

        private PointCloud2 GeneratePointCloud2Raw(NativeArray<Vector3> points)
        {
            PointCloud2 pointCloud = new PointCloud2();
            pointCloud.Header = new Std.Header()
            {
                FrameId = sensor.frameId,
                Timestamp = TimeHandler.Instance.TimeDouble
            };
            pointCloud.Height = 1;
            pointCloud.Width = (uint) points.Length;
            pointCloud.Fields.AddRange(
                new List<PointField>()
                {
                    new PointField()
                    {
                        Name = "x",
                        Offset = 0,
                        Datatype = PointField.Types.DataType.Float32 + 1,
                        Count = 1
                    },
                    new PointField()
                    {
                        Name = "z",
                        Offset = 4,
                        Datatype = PointField.Types.DataType.Float32 + 1,
                        Count = 1
                    },
                    new PointField()
                    {
                        Name = "y",
                        Offset = 8,
                        Datatype = PointField.Types.DataType.Float32 + 1,
                        Count = 1
                    }
                }
            );
            var byteLength = points.Length * sizeof(float) * 3;
            pointCloud.IsBigEndian = false;
            pointCloud.PointStep = sizeof(float) * 3;
            byte[] bytes = points.Reinterpret<byte>(12).ToArray();
            pointCloud.RowStep = (uint) byteLength;
            pointCloud.Data = Google.Protobuf.ByteString.CopyFrom(bytes);
            return pointCloud;
        }

        private PointCloud2 GeneratePointCloud2(NativeArray<Vector3> points, NativeArray<LidarReading> readings)
        {
            PointCloud2 pointCloud = new PointCloud2();
            pointCloud.Header = new Std.Header()
            {
                FrameId = sensor.frameId,
                Timestamp = TimeHandler.Instance.TimeDouble
            };
            pointCloud.Fields.AddRange(
                new List<PointField>()
                {
                    new PointField()
                    {
                        Name = "x",
                        Offset = 0,
                        Datatype = PointField.Types.DataType.Float32 + 1,
                        Count = 1
                    },
                    new PointField()
                    {
                        Name = "z",
                        Offset = 4,
                        Datatype = PointField.Types.DataType.Float32 + 1,
                        Count = 1
                    },
                    new PointField()
                    {
                        Name = "y",
                        Offset = 8,
                        Datatype = PointField.Types.DataType.Float32 + 1,
                        Count = 1
                    },
                    new PointField()
                    {
                        Name = "intensity",
                        Offset = 12,
                        Datatype = PointField.Types.DataType.Int32 + 1,
                        Count = 1
                    },
                    new PointField()
                    {
                        Name = "ring",
                        Offset = 16,
                        Datatype = PointField.Types.DataType.Int32 + 1,
                        Count = 1
                    },
                    new PointField()
                    {
                        Name = "time",
                        Offset = 20,
                        Datatype = PointField.Types.DataType.Uint32 + 1,
                        Count = 1
                    }
                }
            );
            var numOfPoints = readings.Count(r => r.IsValid);
            var pointSize = sizeof(float) * 3 + sizeof(int)*3;
            var byteLength = numOfPoints * pointSize;
            pointCloud.Height = 1;
            pointCloud.Width = (uint) numOfPoints;
            pointCloud.IsBigEndian = false;
            pointCloud.PointStep = (uint) pointSize;
            pointCloud.IsDense = true;
            byte[] bytes = new byte[byteLength];
            int j = 0;
            for(int i = 0; i < points.Length; i++)
            {
                if (!readings[i].IsValid) continue;

                Buffer.BlockCopy( BitConverter.GetBytes( points[i].x ), 0, bytes, j*pointSize, 4 );
                Buffer.BlockCopy( BitConverter.GetBytes( points[i].y ), 0, bytes, j*pointSize + 4, 4 );
                Buffer.BlockCopy( BitConverter.GetBytes( points[i].z ), 0, bytes, j*pointSize + 8, 4 );
                Buffer.BlockCopy( BitConverter.GetBytes( (int) readings[i].Intensity ), 0, bytes, j*pointSize + 12, 4);
                Buffer.BlockCopy( BitConverter.GetBytes( (int) readings[i].Ring ), 0, bytes, j*pointSize + 16, 4);
                Buffer.BlockCopy( BitConverter.GetBytes( (int) readings[i].Time ), 0, bytes, j*pointSize + 20, 4);
                j++;
            }
            pointCloud.RowStep = (uint) byteLength;
            pointCloud.Data = Google.Protobuf.ByteString.CopyFrom(bytes);
            return pointCloud;
        }
        protected async override void SendMessage()
        {
            PointCloud2 _pointCloud;
            if (PublishIntensityAndRing)
            {
                _pointCloud = GeneratePointCloud2(sensor.Points, sensor.Readings);
            }
            else
            {
                _pointCloud = GeneratePointCloud2Raw(sensor.Points);
            }
            var msg = new PointCloud2StreamingRequest()
            {
                Data = _pointCloud,
                Address = address
            };
            await _streamWriter.WriteAsync(msg);
        }
    }
}
