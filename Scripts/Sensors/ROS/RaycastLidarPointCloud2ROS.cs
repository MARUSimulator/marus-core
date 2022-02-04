using Labust.Networking;
using UnityEngine;
using Sensorstreaming;
using Labust.Core;
using Sensor;
using System.Collections.Generic;
using Unity.Collections;

namespace Labust.Sensors
{

    /// <summary>
    /// Lidar that cast N rays evenly distributed in configured field of view.
    /// Implemented using IJobParallelFor on CPU
    /// Can drop performance
    /// </summary>
    [RequireComponent(typeof(RaycastLidar))]
    public class RaycastLidarPointCloud2ROS : SensorStreamer<PointCloud2StreamingRequest>
    {
        RaycastLidar sensor;

        void Start()
        {
            sensor = GetComponent<RaycastLidar>();
            UpdateFrequency = Mathf.Min(UpdateFrequency, sensor.SampleFrequency);
            if (string.IsNullOrEmpty(address))
                address = transform.name + "/lidar2";
            StreamSensor(streamingClient?.StreamPointCloud2(cancellationToken: RosConnection.Instance.cancellationToken));
        }

        new void Update()
        {
            hasData = sensor.hasData;
            base.Update();
        }

        private static PointCloud2 GeneratePointCloud2(NativeArray<Vector3> points)
        {
            PointCloud2 pointCloud = new PointCloud2();
            pointCloud.Header = new Std.Header()
            {
                FrameId = RosConnection.Instance.OriginFrameName,
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
                        Datatype = PointField.Types.DataType.Float64,
                        Count = 1
                    },
                    new PointField()
                    {
                        Name = "z",
                        Offset = 4,
                        Datatype = PointField.Types.DataType.Float64,
                        Count = 1
                    },
                    new PointField()
                    {
                        Name = "y",
                        Offset = 8,
                        Datatype = PointField.Types.DataType.Float64,
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

        protected async override void SendMessage()
        {
            PointCloud2 _pointCloud = GeneratePointCloud2(sensor.pointsCopy);
            var msg = new PointCloud2StreamingRequest()
            {
                Data = _pointCloud,
                Address = address
            };
            await _streamWriter.WriteAsync(msg);
            hasData = false;
        }
    }
}
