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

using Marus.Visualization;
using Unity.Collections;
using UnityEngine;

namespace Marus.Sensors
{

    /// <summary>
    /// Lidar that cast N rays evenly distributed in configured field of view.
    /// Implemented using IJobParallelFor on CPU 
    /// Can drop performance
    /// </summary>
    public class Sonar2D : SensorBase
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
        public NativeArray<Vector3> pointsCopy;

        const float PIOVERTWO = Mathf.PI / 2;
        const float TWOPI = Mathf.PI * 2;
        const float WATER_LEVEL = 0;


        // Start is called before the first frame update
        PointCloudManager _pointCloudManager;
        RaycastJobHelper<SonarReading> _raycastHelper;
        Coroutine _coroutine;
        void Start()
        {

            int totalRays = Resolution;

            pointsCopy = new NativeArray<Vector3>(totalRays, Allocator.Persistent);

            var directionsLocal = RaycastJobHelper.EvenlyDistributeRays(Resolution, 1, FieldOfView, 0);
            _raycastHelper = new RaycastJobHelper<SonarReading>(gameObject, directionsLocal, OnSonarHit, OnFinish);

            _pointCloudManager = PointCloudManager.CreatePointCloud(name + "_PointClout", totalRays, ParticleMaterial, pointCloudShader);
            _coroutine = StartCoroutine(_raycastHelper.RaycastInLoop());

        }

        protected override void SampleSensor()
        {
            _pointCloudManager.UpdatePointCloud(pointsCopy);
        }

        private void OnFinish(NativeArray<Vector3> points, NativeArray<SonarReading> reading)
        {
            points.CopyTo(pointsCopy);
            hasData = true;
        }

        void OnDestroy()
        {
            _raycastHelper.Dispose();
            pointsCopy.Dispose();
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