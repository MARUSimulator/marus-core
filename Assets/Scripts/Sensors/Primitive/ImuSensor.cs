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
    public class ImuSensor : SensorBase<ImuStreamingRequest>
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

        private Rigidbody sensor;
        private Vector3 lastVelocity;

        void Start()
        {
            sensor = GetComponent<Rigidbody>();
            if (streamingClient != null)
            {
                StreamSensor(streamingClient.StreamImuSensor(cancellationToken:RosConnection.Instance.cancellationToken));
            }
            if (string.IsNullOrEmpty(address))
                address = $"{vehicle.name}/imu";
        }

        protected override void SampleSensor()
        {
            localVelocity = sensor.transform.InverseTransformVector(sensor.velocity);
            linearAcceleration = (localVelocity - lastVelocity) / Time.fixedDeltaTime;
            angularVelocity = sensor.angularVelocity;
            orientation = sensor.rotation;
            eulerAngles = orientation.eulerAngles;
            lastVelocity = localVelocity;

            if (withGravity)
                linearAcceleration -= sensor.transform.InverseTransformVector(UnityEngine.Physics.gravity);
            Log(new { linearAcceleration, angularVelocity, eulerAngles });
            hasData = true;
        }

        protected override async void SendMessage()
        {
            var imuOut = new Imu()
            {
                Header = new Header
                {
                    FrameId = frameId,
                    Timestamp = TimeHandler.Instance.TimeDouble
                },
                Orientation = orientation.Unity2Map().AsMsg(),
                AngularVelocity = (-angularVelocity).Unity2Map().AsMsg(),
                LinearAcceleration = linearAcceleration.Unity2Map().AsMsg(),
            };
            imuOut.OrientationCovariance.AddRange(orientationCovariance);
            imuOut.LinearAccelerationCovariance.AddRange(linearAccelerationCovariance);
            imuOut.AngularVelocityCovariance.AddRange(angularVelocityCovariance);

            await _streamWriter.WriteAsync(new ImuStreamingRequest
            {
                Data = imuOut,
                Address = address
            });
            hasData = false;
        }
    }
}