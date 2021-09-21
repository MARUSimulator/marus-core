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
    public class Sonar2D : SensorBase<LidarStreamingRequest>
    {

        /// Instantiates 3 Jobs: 
        /// 1) RaycastCommand creation - create raycast commands <see cref="RaycastCommand"> for lidar FoV
        /// 2) RaycastCommand execution
        /// 3) RaycastHit data interpretation - extract points, distances etc.



        /// <summary>
        /// Material set for point cloud display
        /// </summary>
        public Material ParticleMaterial;

        public int Resolution = 31;

        public float MaxDistance = float.MaxValue;
        public float MinDistance = 0;
        public float FieldOfView = 30;
        public float RayIntensity = 30;

        public ComputeShader pointCloudShader;
        private NativeArray<Vector3> _pointsCopy;

        const float PIOVERTWO = Mathf.PI / 2;
        const float TWOPI = Mathf.PI * 2;
        const float WATER_LEVEL = 0;


        // Start is called before the first frame update
        PointCloudManager _pointCloudManager;
        RaycastJobHelper<SonarReading> _raycastHelper;

        void Start()
        {

            int totalRays = Resolution;
            streamHandle = streamingClient.StreamLidarSensor(cancellationToken: RosConnection.Instance.cancellationToken);
            if (string.IsNullOrEmpty(address))
                address = transform.name + "/sonar3d";

            _pointsCopy = new NativeArray<Vector3>(totalRays, Allocator.Persistent);

            var directionsLocal = RaycastJobHelper.EvenlyDistributeRays(Resolution, 1, FieldOfView, 0);
            _raycastHelper = new RaycastJobHelper<SonarReading>(gameObject, directionsLocal, OnSonarHit, OnFinish);

            _pointCloudManager = PointCloudManager.CreatePointCloud(name + "_PointClout", totalRays, ParticleMaterial, pointCloudShader);

            StartCoroutine(_raycastHelper.RaycastInLoop());
        }

        private void OnFinish(NativeArray<Vector3> points, NativeArray<SonarReading> reading)
        {
            _pointCloudManager.UpdatePointCloud(points);
            points.CopyTo(_pointsCopy);
            hasData = true;
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

        public SonarReading OnSonarHit(RaycastHit hit, Vector3 direction, int i)
        {
            var distance = hit.distance;
            var sonarReading = new SonarReading();
            if (distance < MinDistance || hit.point.y > WATER_LEVEL) // if above water, it is not hit!
            {
                sonarReading.Valid = false;
            }
            else
            {
                sonarReading.Valid = true;
                sonarReading.Distance = hit.distance;
                sonarReading.Intensity = RayIntensity * (Vector3.Dot(direction, hit.normal)) / Mathf.Pow(hit.distance, 4);
            }
            return sonarReading;
        }

    }

}