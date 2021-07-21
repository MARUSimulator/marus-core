using System;
using System.Collections;
using System.Collections.Generic;
using Labust.Core;
using Labust.Networking;
using Sensor;
using Sensorstreaming;
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
        [Header("Accelerometer")]
        public Vector3 linearAcceleration;
        public bool withGravity = true;
        public double[] linearAccelerationCovariance = new double[9]; 

        [Header("Gyro")]
        public Vector3 angularVelocity;
        public double[] angularVelocityCovariance = new double[9];

        [Header("Orientation")]
        public Vector3 eulerAngles;
        public Quaternion orientation;
        public double[] orientationCovariance = new double[9];

        [Header("Debug")]
        public Vector3 localVelocity;

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
            Log(new { linearAcceleration, angularVelocity, eulerAngles });
            hasData = true;
        }
    }
}