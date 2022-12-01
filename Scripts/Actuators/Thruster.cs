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

    public class Thruster : MonoBehaviour
    {
        public float LastForceRequest;
        public float TimeSinceForceRequest = 0.0f;
        public ThrusterAsset ThrusterAsset;
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
            _logger = DataLogger.Instance.GetLogger<LogRecord>($"{vehicle.transform.name}/{name}");
        }

        /// <summary>
        /// Apply force to the thruster location from datasheet and standardized input
        /// </summary>
        /// <param name="normalizedInput"> -1 - 1 value</param>
        /// <returns></returns>
        public Vector3 ApplyInput(float normalizedInput)
        {
            float value = ThrusterAsset.curve.Evaluate(normalizedInput);
            // from kgf to N
            LastForceRequest = value * 9.80665f;
            TimeSinceForceRequest = 0.0f;

            _logger.Log(new LogRecord { NormalizedInput = normalizedInput, Force = transform.forward * LastForceRequest});
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
            if(TimeSinceForceRequest <= 0.2f)
            {
                Vector3 force = transform.forward * LastForceRequest;
                _vehicleBody.AddForceAtPosition(force, transform.position, ForceMode.Force);
            }
            TimeSinceForceRequest += Time.fixedDeltaTime;
        }

        private class LogRecord
        {
            public float NormalizedInput { get; set; }
            public Vector3 Force { get; set; }
        }
    }
}