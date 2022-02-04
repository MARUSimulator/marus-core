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
