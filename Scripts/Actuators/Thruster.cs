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
using System.Collections.Generic;
using Marus.Logger;
using Marus.Utils;
using UnityEditor;
using UnityEngine;
namespace Marus.Actuators
{

    public class Thruster : MonoBehaviour
    {
        //Calback for change in voltage
        public float lastForceRequest;
        public float timeSinceForceRequest = 0.0f;

        Rigidbody _vehicleBody;
        Transform _vehicle;
        GameObjectLogger<PwmLogRecord> _logger;
        public ThrusterAsset[] thrusters;
        public ThrusterAsset selectedThruster;
        public AnimationCurve currentCurve = new AnimationCurve();
        public int previousThrusterIndex = -1;
        public int selectedThrusterIndex = 0;

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
            _logger = DataLogger.Instance.GetLogger<PwmLogRecord>($"{vehicle.transform.name}/{name}");

        }

        /// <summary>
        /// Apply force to the thruster location from datasheet and standardized pwm input
        /// </summary>
        /// <param name="pwmIn"> -1 - 1 value</param>
        /// <returns></returns>
        public Vector3 ApplyPwm(float pwmIn)
        {
            float value = selectedThruster.curve.Evaluate(pwmIn);
            // from kgf to N
            lastForceRequest = value * 9.80665f;
            timeSinceForceRequest = 0.0f;

            _logger.Log(new PwmLogRecord { PwmIn = pwmIn, Force = transform.forward * lastForceRequest});
            return transform.forward * lastForceRequest;
        }

        public float GetPwmForForce(float force)
        {
            // from N to kgf
            force /= 9.80665f;
            var pwm_value = selectedThruster.inversedCurve.Evaluate(force);

            return pwm_value + 1.0f;
        }

        void FixedUpdate()
        {
            if(timeSinceForceRequest <= 0.2f)
            {
                Vector3 force = transform.forward * lastForceRequest;
                _vehicleBody.AddForceAtPosition(force, transform.position, ForceMode.Force);
            }
            timeSinceForceRequest += Time.fixedDeltaTime;
        }

        private class PwmLogRecord
        {
            public float PwmIn { get; set; }
            public Vector3 Force { get; set; }
        }
    }
}