using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Labust.Networking;
using Labust.Sensors;
using Labust.Sensors.Core;
using Labust.Visualization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Sensorstreaming;
using System.Threading;
using Labust.Core;

namespace Labust.Sensors
{

    /// <summary>
    /// Lidar that cast N rays evenly distributed in configured field of view.
    /// Implemented using IJobParallelFor on CPU
    /// Can drop performance
    /// </summary>
    public class RaycastLidar : SensorBase<LidarStreamingRequest>
    {

        /// Instantiates 3 Jobs:
        /// 1) RaycastCommand creation - create raycast commands <see cref="RaycastCommand"> for lidar FoV
        /// 2) RaycastCommand execution
        /// 3) RaycastHit data interpretation - extract points, distances etc.



        /// <summary>
        /// Material set for point cloud display
        /// </summary>
        public Material ParticleMaterial;

        public int WidthRes = 1024;

        public int HeightRes = 16;
        public float MaxDistance = 100;
        public float MinDistance = 0.2f;
        public float FieldOfView = 30;

        public ComputeShader pointCloudShader;

        private NativeArray<Vector3> _pointsCopy;

        const float PIOVERTWO = Mathf.PI / 2;
        const float TWOPI = Mathf.PI * 2;




        // Start is called before the first frame update
        PointCloudManager _pointCloudManager;
        RaycastJobHelper<LidarReading> _raycastHelper;

        void Start()
        {
            int totalRays = WidthRes * HeightRes;
            streamHandle = streamingClient.StreamLidarSensor(cancellationToken: RosConnection.Instance.cancellationToken);
            if (string.IsNullOrEmpty(address))
                address = transform.name + "/lidar";

            _pointsCopy = new NativeArray<Vector3>(WidthRes * HeightRes, Allocator.Persistent);

            var directionsLocal = RaycastJobHelper.EvenlyDistributeRays(WidthRes, HeightRes, 360, FieldOfView);

            _raycastHelper = new RaycastJobHelper<LidarReading>(gameObject, directionsLocal, OnLidarHit, OnFinish);

            _pointCloudManager = PointCloudManager.CreatePointCloud(name + "_PointClout", totalRays, ParticleMaterial, pointCloudShader);

            StartCoroutine(_raycastHelper.RaycastInLoop());
        }

        private void OnFinish(NativeArray<Vector3> points, NativeArray<LidarReading> reading)
        {
            _pointCloudManager.UpdatePointCloud(points);
            points.CopyTo(_pointsCopy);
            hasData = true;
        }

        private LidarReading OnLidarHit(RaycastHit hit, Vector3 direction, int index)
        {
            return new LidarReading();
        }

        void OnDestroy()
        {
            _raycastHelper.Dispose();
            _pointsCopy.Dispose();
        }

        public async override void SendMessage()
        {
            Sensor.PointCloud _pointCloud = new Sensor.PointCloud();
            foreach (Vector3 point in _pointsCopy)
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
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000.0
            };

            var msg = new LidarStreamingRequest()
            {
                Data = _pointCloud,
                Address = address
            };
            await _streamWriter.WriteAsync(msg);
            hasData = false;
        }
    }


    internal struct LidarReading
    {
    }
}