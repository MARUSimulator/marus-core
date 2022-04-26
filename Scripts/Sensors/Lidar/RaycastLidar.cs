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
using System;

namespace Marus.Sensors
{

    /// <summary>
    /// Lidar that cast N rays evenly distributed in configured field of view.
    /// Implemented using IJobParallelFor on CPU
    /// Can drop performance
    /// </summary>
    public class RaycastLidar : SensorBase
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

        public event Action<NativeArray<Vector3>, NativeArray<LidarReading>> OnFinishEvent;
        public NativeArray<Vector3> pointsCopy;


        PointCloudManager _pointCloudManager;
        RaycastJobHelper<LidarReading> _raycastHelper;
        Coroutine _coroutine;

        void Start()
        {
            int totalRays = WidthRes * HeightRes;

            pointsCopy = new NativeArray<Vector3>(WidthRes * HeightRes, Allocator.Persistent);

            var directionsLocal = RaycastJobHelper.EvenlyDistributeRays(WidthRes, HeightRes, 360, FieldOfView);

            _raycastHelper = new RaycastJobHelper<LidarReading>(gameObject, directionsLocal, OnLidarHit, OnFinish);

            _pointCloudManager = PointCloudManager.CreatePointCloud(name + "_PointCloud", totalRays, ParticleMaterial, pointCloudShader);
            _coroutine = StartCoroutine(_raycastHelper.RaycastInLoop());
        }

        protected override void SampleSensor()
        {
            _pointCloudManager.UpdatePointCloud(pointsCopy);
        }

        private void OnFinish(NativeArray<Vector3> points, NativeArray<LidarReading> reading)
        {
            points.CopyTo(pointsCopy);
            OnFinishEvent?.Invoke(pointsCopy, reading);
            Log(new {points});
            hasData = true;
        }

        private LidarReading OnLidarHit(RaycastHit hit, Vector3 direction, int index)
        {
            var reading = new LidarReading();
            reading.hit = hit;
            return reading;
        }

        void OnDestroy()
        {
            _raycastHelper?.Dispose();
            pointsCopy.Dispose();
        }

    }

    public struct LidarReading
    {
        public RaycastHit hit;
    }
}
