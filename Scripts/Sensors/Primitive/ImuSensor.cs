using System;
using Labust.Utils;
using Std;
using UnityEngine;

namespace Labust.Sensors.Primitive
{
    /// <summary>
    /// Imu sensor implementation
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ImuSensor : SensorBase
    {
        public bool withGravity = true;
        public bool debug = true;
        [NonSerialized] public Vector3 linearAcceleration;
        [NonSerialized] public Vector3 localVelocity;
        [NonSerialized] public double[] linearAccelerationCovariance = new double[9];

        [NonSerialized] public Vector3 angularVelocity;
        [NonSerialized] public double[] angularVelocityCovariance = new double[9];

        [NonSerialized]public Vector3 eulerAngles;
        [NonSerialized]public Quaternion orientation;
        [NonSerialized] public double[] orientationCovariance = new double[9];


        [Header("Accelerometer")]
        [ReadOnly] public Vector3 LinearAcceleration;
        [ReadOnly] public Vector3 LocalVelocity;

        [Header("Gyro")]
        [ReadOnly] public Vector3 AngularVelocity;

        [Header("Orientation")]
        [ReadOnly] public Vector3 EulerAngles;
        [ReadOnly] public Quaternion Orientation;
        private Rigidbody rb;
        private Vector3 lastVelocity;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        protected override void SampleSensor()
        {
            localVelocity = rb.transform.InverseTransformVector(rb.velocity);
            linearAcceleration = (localVelocity - lastVelocity) / Time.fixedDeltaTime;
            angularVelocity = rb.angularVelocity;
            orientation = rb.rotation;
            eulerAngles = orientation.eulerAngles;
            lastVelocity = localVelocity;

            if (withGravity)
                linearAcceleration -= rb.transform.InverseTransformVector(UnityEngine.Physics.gravity);

            if (debug)
            {
                LinearAcceleration = linearAcceleration.Round(2);
                LocalVelocity = localVelocity.Round(2);
                AngularVelocity = angularVelocity.Round(2);
                EulerAngles = eulerAngles.Round(2);
                Orientation = orientation.Round(2);
            }
            Log(new { linearAcceleration, angularVelocity, eulerAngles, localVelocity });
            hasData = true;
        }
    }
}