using System;
using Std;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Labust.Sensors.Primitive
{
    public class DvlSensor : SensorBase
    {
        [Header("Sensor parameters measurements")]
        [Header("Core measurements")]
        public Vector3 groundVelocity = new Vector3();
        public double[] velocityCovariance = new double[9];
        //public Vector3 velocity;
        //public Rigidbody body;
        
        public float altitude;
        public double altitudeCovariance;

        [Header("Debug Beams")]
        public float[] beamRanges;
        RangeSensor[] beams;

        private Vector3 lastPosition;

        void Start()
        {
            beams = GetComponentsInChildren<RangeSensor>();
            beamRanges = new float[beams.Length];
            lastPosition = transform.position;
        }

        protected override void SampleSensor()
        {
            var position = transform.position;
            groundVelocity = transform.worldToLocalMatrix * ((position - lastPosition) / Time.fixedDeltaTime);

            lastPosition = position;

            altitude = Single.MaxValue;
            for (int i = 0; i < beams.Length; ++i)
            {
                beams[i].SampleSensor();

                if (beams[i].range < altitude)
                    altitude = beams[i].range;

                beamRanges[i] = beams[i].range;
            }
            hasData = true;
        }
    }

}
