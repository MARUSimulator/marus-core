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
using Grpc.Core;
using System.Collections.Generic;
using Sensorstreaming;
using static Sensorstreaming.SensorStreaming;
using Marus.Utils;
using Marus.Networking;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Marus.Visualization
{
    static class PointFields
    {
        public const int X = 0;
        public const int Y = 1;
        public const int Z = 2;
        public const int NORMAL_X = 3;
        public const int NORMAL_Y = 4;
        public const int NORMAL_Z = 5;
        public const int RGB = 6;
        public const int RGBA = 7;
    }
    public class PointCloudRosVisualizer : Singleton<PointCloudRosVisualizer>
    {

        public string Address = "/unity/pointcloud_visualizer";
        ServerStreamer<PointCloud2StreamingRequest> _serverStreamer;

        Vector3[] points;
        Color[] colors;
        Vector3[] normals;

        PointCloud pc;
        protected override void Initialize()
        {
            int MAX_POINTS = 1000000;

            _serverStreamer = new ServerStreamer<PointCloud2StreamingRequest>(UpdatePointCloud);
            _serverStreamer.mode = MessageHandleMode.Sequential;
            transform.parent = RosConnection.Instance.transform;
            RosConnection.Instance.OnConnected += OnConnected;
            points = new Vector3[MAX_POINTS];
            colors = new Color[MAX_POINTS];
            normals = new Vector3[MAX_POINTS];

        }

        public void OnConnected(Channel channel)
        {
            var rosConn = RosConnection.Instance;
            if (!_serverStreamer.IsStreaming)
            {
                var pcClient = rosConn.GetClient<SensorStreamingClient>();
                var pcStream = pcClient.RequestPointCloud2(new Std.StandardRequest()
                {
                    Address = Address
                });
                _serverStreamer.StartStream(pcStream);
            }
        }

        void Update()
        {
            _serverStreamer.HandleNewMessages();
        }

        private async void UpdatePointCloud(PointCloud2StreamingRequest pointcloudRequest)
        {
            await Task.Run(() => PointCloud2ToPointCloud(pointcloudRequest.Data));
            var _pc = (Primitives.PointcloudMesh) Visualizer.Instance.GetElementById("unity_pointcloud_ros");
            if (_pc == null)
            {
                _pc = Visualizer.Instance.AddPointCloud(pc, "unity_pointcloud");
                _pc.Id = "unity_pointcloud_ros";
                //_pc.Lifetime = 10f;
            }
            _pc.myMesh.vertices = pc.Points;
        }

        /// <summary>
        /// Converts PointCloud2 object defined by proto message to C# Pointcloud object
        /// by converting raw bytes to corresponding point fields.
        /// </summary>
        /// <param name="pointcloud"></param>
        /// <returns></returns>
        public void PointCloud2ToPointCloud(Sensor.PointCloud2 pointcloud)
        {
            List<(int, int, int)> fields = new List<(int, int, int)>();
            var fieldKeys = new Dictionary<string, int>()
            {
                {"x", PointFields.X},
                {"y", PointFields.Y},
                {"z", PointFields.Z},
                {"normal_x", PointFields.NORMAL_X},
                {"normal_y", PointFields.NORMAL_Y},
                {"normal_z", PointFields.NORMAL_Z},
                {"rgb", PointFields.RGB},
                {"rgba", PointFields.RGBA},
            };
            foreach (var pf in pointcloud.Fields)
            {
                var value = 55;
                fieldKeys.TryGetValue(pf.Name.ToLower(), out value);
                var fieldSize = Helpers.PointFieldDataTypeToBytes(pf.Datatype - 1);
                fields.Add((value, fieldSize, (int) pf.Offset));
            }
            var numOfPoints = pointcloud.Height * pointcloud.Width;
            float x = 0f;
            float y = 0f;
            float z = 0f;

            float normal_x = 0f;
            float normal_y = 0f;
            float normal_z = 0f;

            float r = 0f;
            float g = 1f;
            float b = 0f;
            float a = 1f;
            bool hasNormals = false;

            int i = 0;
            var buffer = pointcloud.Data.ToByteArray();
            var offset = 0;
            while (i < numOfPoints)
            {
                foreach ((int field, int size, int fieldOffset) in fields)
                {
                    switch (field)
                    {
                        case PointFields.X:
                        {
                            if (size == 4)
                            {
                                x = BitConverter.ToSingle(buffer, offset);
                            }
                            else if (size == 8)
                            {
                                x = (float) BitConverter.ToDouble(buffer, offset);
                            }
                            break;
                        }

                        case PointFields.Y:
                        {
                            if (size == 4)
                            {
                                y = BitConverter.ToSingle(buffer, offset);
                            }
                            else if (size == 8)
                            {
                                y = (float) BitConverter.ToDouble(buffer, offset);
                            }
                            break;
                        }

                        case PointFields.Z:
                        {
                            if (size == 4)
                            {
                                z = BitConverter.ToSingle(buffer, offset);
                            }
                            else if (size == 8)
                            {
                                z = (float) BitConverter.ToDouble(buffer, offset);
                            }
                            break;
                        }

                        case PointFields.NORMAL_X:
                        {
                            if (size == 4)
                            {
                                normal_x = BitConverter.ToSingle(buffer, offset);
                            }
                            else if (size == 8)
                            {
                                normal_x = (float) BitConverter.ToDouble(buffer, offset);
                            }
                            break;
                        }

                        case PointFields.NORMAL_Y:
                        {
                            if (size == 4)
                            {
                                normal_y = BitConverter.ToSingle(buffer, offset);
                            }
                            else if (size == 8)
                            {
                                normal_y = (float) BitConverter.ToDouble(buffer, offset);
                            }
                            break;
                        }

                        case PointFields.NORMAL_Z:
                        {
                            if (size == 4)
                            {
                                normal_z = BitConverter.ToSingle(buffer, offset);
                            }
                            else if (size == 8)
                            {
                                normal_z = (float) BitConverter.ToDouble(buffer, offset);
                            }
                            hasNormals = true;
                            break;
                        }

                        case PointFields.RGB:
                        {
                            r = (float) (buffer[offset] / 255.0f);
                            g = (float) (buffer[offset + 1] / 255.0f);
                            b = (float) (buffer[offset + 2] / 255.0f);
                            break;
                        }

                        case PointFields.RGBA:
                        {
                            r = (float) (buffer[offset] / 255.0f);
                            g = (float) (buffer[offset + 1] / 255.0f);
                            b = (float) (buffer[offset + 2] / 255.0f);
                            a = (float) (buffer[offset + 3]);
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                    offset = i * (int) pointcloud.PointStep + fieldOffset;
                }
                points[i] = new Vector3(x, z, y);
                normals[i] = new Vector3(normal_x, normal_z, normal_y);
                colors[i] = new Color(b, g, r, a);
                i++;
            }

            pc = new PointCloud(points);
            pc.Colors = colors;
            if (hasNormals)
            {
                pc.Normals = normals;
            }
        }
    }
}
