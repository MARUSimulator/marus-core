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

using Marus.Logger;
using Marus.Utils;
using UnityEngine;
namespace Marus.Actuators
{

    public class DifferentialThruster : MonoBehaviour
    {
        public float AngleSpeed = 5.0f;
        public float AngleLimit = 90;
        public float DiffLimit = 30;
        public ThrusterAsset ThrusterAsset;
        float _currentAngle;
        float _angleRef = 0.0f;
        const float k = 1 / 90f;
        float LastForceRequest;
        float TimeSinceForceRequest = 0.0f;
        Rigidbody _vehicleBody;
        Transform _vehicle;
        GameObjectLogger<LogRecord> _logger;

        Transform vehicle
        {
            get
            {
                if (_vehicleBody != null)
                {
                    return _vehicleBody.transform;
                }

                _vehicle = Helpers.GetVehicle(transform);
                if (_vehicle == null)
                {
                    Debug.Log($@"Cannot get vehicle from sensor {transform.name}.
                        Using sensor as the vehicle transform");
                    return transform;
                }
                _vehicleBody = _vehicle.GetComponent<Rigidbody>();
                return _vehicleBody.transform;
            }
        }

        void Start()
        {
            _currentAngle = 0;
            _logger = DataLogger.Instance.GetLogger<LogRecord>($"{vehicle.transform.name}/{name}");
        }

        /// <summary>
        /// Apply force to the thruster location from datasheet and standardized input
        /// </summary>
        /// <param name="normalizedInput"> -1 - 1 value</param>
        /// <returns></returns>
        public Vector3 ApplyInput(float force, float angle)
        {
            float value = ThrusterAsset.curve.Evaluate(force);
            // from kgf to N
            LastForceRequest = value * 9.80665f;
            TimeSinceForceRequest = 0.0f;
            _angleRef = angle * 180 / Mathf.PI;
            if (_angleRef < -AngleLimit)
            {
                _angleRef = -AngleLimit;
            }
            else if (_angleRef > AngleLimit)
            {
                _angleRef = AngleLimit;
            }
            _logger.Log(new LogRecord { NormalizedInput = force, Force = transform.forward * LastForceRequest});
            return transform.forward * LastForceRequest;
        }

        public float GetInputFromForce(float force)
        {
            // from N to kgf
            force /= 9.80665f;
            var input_value = ThrusterAsset.inversedCurve.Evaluate(force);

            return input_value + 1.0f;
        }

        void FixedUpdate()
        {
            if(TimeSinceForceRequest <= 0.2f) // latch force
            {
                Vector3 force = transform.forward * LastForceRequest;
                _vehicleBody.AddForceAtPosition(force, transform.position, ForceMode.Force);
            }
            TimeSinceForceRequest += Time.fixedDeltaTime;

            var euler = transform.rotation.eulerAngles;
            Quaternion target = Quaternion.Euler(euler.x, _angleRef, euler.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.fixedDeltaTime * AngleSpeed * k);
        }

        private class LogRecord
        {
            public float NormalizedInput { get; set; }
            public Vector3 Force { get; set; }
        }
    }
}