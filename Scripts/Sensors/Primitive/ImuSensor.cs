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

using System;
using Marus.NoiseDistributions;
using Marus.Utils;
using Std;
using UnityEngine;

namespace Marus.Sensors.Primitive
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
        public NoiseParameters AccelerometerNoise;
        [ReadOnly] public Vector3 LinearAcceleration;
        [ReadOnly] public Vector3 LocalVelocity;

        [Header("Gyro")]
        public NoiseParameters GyroNoise;
        [ReadOnly] public Vector3 AngularVelocity;

        [Header("Orientation")]
        public NoiseParameters OrientationNoise;
        [ReadOnly] public Vector3 EulerAngles;
        [ReadOnly] public Quaternion Orientation;
        private Rigidbody rb;
        private Vector3 lastVelocity;
        private double _lastSampleTime;


        new void Reset()
        {
            rb = GetComponent<Rigidbody>();

            base.Reset();
            UpdateVehicle();
        }

        new void UpdateVehicle()
        {
            base.UpdateVehicle();

            var veh = vehicle;

            // get vehicle rigidbody (either this gameObject or vehicle tag)
            // This is needed for IMU to work as expected
            Rigidbody veh_rb = veh.GetComponent<Rigidbody>();

            // if vehicle tag not found or no rigidbody found on tagged vehicle
            // find top parent with rigidbody
            if(veh == transform || (veh_rb is null))
            {
                veh_rb = Helpers.GetComponentInParents<Rigidbody>(veh.parent?.gameObject);
            }

            // if parent rigidbody found, attach fixed joint to it
            if(veh_rb)
            {
                // attach fixed joint to it
                // This is needed for IMU to work as expected
                FixedJoint fj = gameObject.GetComponent<FixedJoint>();
                if(!fj) fj = gameObject.AddComponent<FixedJoint>();
                fj.connectedBody = veh_rb;
            }
        }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        protected override void SampleSensor()
        {
            double timeElapsed = Time.timeAsDouble - _lastSampleTime;
            _lastSampleTime = Time.timeAsDouble;
            
            localVelocity = rb.transform.InverseTransformVector(rb.velocity);
            localVelocity[0]+=Noise.Sample(AccelerometerNoise);
            localVelocity[1]+=Noise.Sample(AccelerometerNoise);
            localVelocity[2]+=Noise.Sample(AccelerometerNoise);
            linearAcceleration = ((localVelocity - lastVelocity) / (float) timeElapsed);

            angularVelocity = rb.angularVelocity;
            angularVelocity[0]+=Noise.Sample(GyroNoise);
            angularVelocity[1]+=Noise.Sample(GyroNoise);
            angularVelocity[2]+=Noise.Sample(GyroNoise);

            orientation = rb.rotation;
            orientation[0]+=Noise.Sample(OrientationNoise);
            orientation[1]+=Noise.Sample(OrientationNoise);
            orientation[2]+=Noise.Sample(OrientationNoise);

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