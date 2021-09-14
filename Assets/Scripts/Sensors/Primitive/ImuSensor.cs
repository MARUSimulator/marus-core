﻿using System.Collections;
using System.Collections.Generic;
using Labust.Core;
using Labust.Networking;
using Sensorstreaming;
using UnityEngine;

namespace Labust.Sensors.Primitive
{
    /// <summary>
    /// Imu sensor implementation
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ImuSensor : SensorBase<ImuStreamingRequest>
    {
        
        [Header("Accelerometer")]
        public Vector3 linearAcceleration;
        public bool withGravity = true;

        [Header("Gyro")]
        public Vector3 angularVelocity;

        [Header("Orientation")]
        public Vector3 eulerAngles;
        public Quaternion quaternion;

        [Header("Debug")]
        public Vector3 localVelocity;

        private Rigidbody sensor;
        private Vector3 lastVelocity;

        void Awake()
        {
            sensor = GetComponent<Rigidbody>();
            streamHandle = streamingClient.StreamImuSensor(cancellationToken:RosConnection.Instance.cancellationToken);
            AddSensorCallback(SensorCallbackOrder.First, CalculateAccelerationAsVelocityDerivative);
            if (string.IsNullOrEmpty(address))
                address = vehicle.name + "/imu";
        }


        void CalculateAccelerationAsVelocityDerivative()
        {
            localVelocity = sensor.transform.InverseTransformVector(sensor.velocity);
            linearAcceleration = (localVelocity - lastVelocity) / Time.deltaTime;
            lastVelocity = localVelocity;

            if (withGravity)
                linearAcceleration -= sensor.transform.InverseTransformVector(UnityEngine.Physics.gravity);
            Log(new { linearAcceleration, angularVelocity, eulerAngles });
            hasData = true;
        }

        public async override void SendMessage()
        {
            var toRad = eulerAngles * Mathf.Deg2Rad;
            await _streamWriter.WriteAsync(new ImuStreamingRequest
            {
                Acceleration = linearAcceleration.Unity2Standard().AsMsg(),
                AngularVelocity = angularVelocity.Unity2Standard().AsMsg(),
                Orientation = toRad.AsMsg(),
                Address = address
            });
            hasData = false;
        }
    }
}