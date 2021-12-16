using Labust.Core;
using Labust.Networking;
using Std;
using UnityEngine;

namespace Labust.Sensors.Primitive
{
    public class GnssSensor : SensorBase
    {
        [Header("Position")]
        public GeographicFrame origin;
        public GeoPoint point;

        [Header("Precision")]
        public double[] covariance;
        public bool isRTK = true;
        public float maximumOperatingDepth = 0.5f;

        void Start()
        {
            covariance = new double[] { 0.1, 0, 0, 0, 0.1, 0, 0, 0, 0.1 };
        }

        protected override void SampleSensor()
        {
            var world = TfHandler.Instance.OriginGeoFrame;
            point = world.Unity2Geo(transform.position);
            Log(new { point.latitude, point.longitude, point.altitude });
            hasData = true;
        }
    }
}