using System.Collections;
using System.Collections.Generic;
using Labust.Networking;
using Sensorstreaming;
using UnityEngine;

namespace Labust.Sensors
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
            if (string.IsNullOrEmpty(sensorId))
                sensorId = vehicle.name + "/imu";
        }


        void CalculateAccelerationAsVelocityDerivative()
        {
            localVelocity = sensor.transform.InverseTransformVector(sensor.velocity);
            linearAcceleration = (localVelocity - lastVelocity) / Time.deltaTime;
            lastVelocity = localVelocity;

            if (withGravity)
                linearAcceleration -= sensor.transform.InverseTransformVector(UnityEngine.Physics.gravity);
            hasData = true;
        }

        public async override void SendMessage()
        {
            var acc = new Common.Acceleration 
            { 
                X = linearAcceleration.x, 
                Y = linearAcceleration.y, 
                Z = linearAcceleration.z 
            };
            var angVel = new Common.AngularVelocity
            {
                X = angularVelocity.x,
                Y = angularVelocity.y,
                Z = angularVelocity.z
            };
            var o = new Common.Orientation
            {
                Roll = eulerAngles.x,
                Pitch = eulerAngles.y,
                Yaw = eulerAngles.z
            };
            await streamWriter.WriteAsync(new ImuStreamingRequest
            {
                Acceleration = acc,
                AngularVelocity = angVel,
                Orientation = o,
                SensorId = sensorId
            });
            hasData = false;
        }
    }
}