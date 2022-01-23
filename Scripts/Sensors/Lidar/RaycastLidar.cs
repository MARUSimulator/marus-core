using Labust.Visualization;
using Unity.Collections;
using UnityEngine;
namespace Labust.Sensors
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
            Log(new {points});
            hasData = true;
        }

        private LidarReading OnLidarHit(RaycastHit hit, Vector3 direction, int index)
        {
            return new LidarReading();
        }

        void OnDestroy()
        {
            _raycastHelper?.Dispose();
            pointsCopy.Dispose();
        }

    }

    internal struct LidarReading
    {
    }
}
