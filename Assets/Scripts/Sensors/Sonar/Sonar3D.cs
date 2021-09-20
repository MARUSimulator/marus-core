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



namespace Labust.Sensors
{

    /// <summary>
    /// Lidar that cast N rays evenly distributed in configured field of view.
    /// Implemented using IJobParallelFor on CPU 
    /// Can drop performance
    /// </summary>
    public class Sonar3D : SensorBase<LidarStreamingRequest>
    {

        /// Instantiates 3 Jobs: 
        /// 1) RaycastCommand creation - create raycast commands <see cref="RaycastCommand"> for lidar FoV
        /// 2) RaycastCommand execution
        /// 3) RaycastHit data interpretation - extract points, distances etc.



        /// <summary>
        /// Material set for point cloud display
        /// </summary>
        public Material ParticleMaterial;

        public int WidthRes = 31;

        public int HeightRes = 10;
        public float MaxDistance = float.MaxValue;
        public float MinDistance = 0;
        public float HorizontalFieldOfView = 30;
        public float VerticalFieldOfView = 12;

        public bool IsIdeal = false;

        [ConditionalHideInInspector("IsIdeal", true)]
        public float RayIntensity = 12;

        int NumRaysPerAccusticRay = 1; // 1, 5, 9 TODO
                                       // float rayWidth = 0.001f; // in radians

        public ComputeShader pointCloudShader;

        const float PIOVERTWO = Mathf.PI / 2;
        const float TWOPI = Mathf.PI * 2;
        const float WATER_LEVEL = 0;


        // Start is called before the first frame update
        PointCloudManager _pointCloudManager;
        RaycastJobHelper<SonarReading> _raycastHelper;

        void Start()
        {

            int totalRays = WidthRes * HeightRes * NumRaysPerAccusticRay;
            streamHandle = streamingClient.StreamLidarSensor(cancellationToken: RosConnection.Instance.cancellationToken);

            var directionsLocal = RaycastJobHelper.EvenlyDistributeRays(WidthRes, HeightRes, HorizontalFieldOfView, VerticalFieldOfView);
            _raycastHelper = new RaycastJobHelper<SonarReading>(gameObject, directionsLocal, OnSonarHit, OnFinish, MaxDistance);

            _pointCloudManager = PointCloudManager.CreatePointCloud(name + "_PointClout", totalRays, ParticleMaterial, pointCloudShader);

            StartCoroutine(_raycastHelper.RaycastInLoop());
        }

        private void OnFinish(NativeArray<Vector3> points, NativeArray<SonarReading> reading)
        {
            _pointCloudManager.UpdatePointCloud(points);
        }

        void OnDestroy()
        {
            _raycastHelper.Dispose();
        }

        public override void SendMessage()
        {
            // TBD
            // streamWriter.WriteAsync(new LidarStreamingRequest
            // {

            // });
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