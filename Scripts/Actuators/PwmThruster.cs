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
using Marus.Actuators.Datasheets;
using Marus.Logger;
using Marus.Ocean;
using Marus.Utils;
using UnityEngine;

namespace Marus.Actuators
{
    public class PwmThruster : MonoBehaviour
    {

        const float G = 9.80665f;

        public enum AllowedVoltages
        {
            V10 = 10,
            V12 = 12,
            V14 = 14,
            V16 = 16,
            V20 = 20,
            V20x10 = 21,
            V20x50 = 22,
            V20x100 = 23
        };

        int _voltage;
        public AllowedVoltages voltage = AllowedVoltages.V10;
        float[] sheetData;
        float sheetStep;

        [ReadOnly, SerializeField]
        private float force;
        [ReadOnly, SerializeField]

        Rigidbody _vehicleBody;
        Transform _vehicle;
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
                    Debug.Log($@"Cannot get vehicle from object {transform.name}.
                        Using sensor as the vehicle transform");
                    return transform;
                }
                _vehicleBody = _vehicle.GetComponent<Rigidbody>();
                return _vehicleBody.transform;
            }
        }

        GameObjectLogger<PwmLogRecord> _logger;

        void Start()
        {
            // set voltage and thruster sheet
            _voltage = (int)voltage;
            sheetStep = T200ThrusterDatasheet.step;
            switch (voltage)
            {
                case AllowedVoltages.V10:
                    sheetData = T200ThrusterDatasheet.V10;
                    break;
                case AllowedVoltages.V12:
                    sheetData = T200ThrusterDatasheet.V10;
                    break;
                case AllowedVoltages.V14:
                    sheetData = T200ThrusterDatasheet.V10;
                    break;
                case AllowedVoltages.V16:
                    sheetData = T200ThrusterDatasheet.V16;
                    break;
                case AllowedVoltages.V20:
                    sheetData = T200ThrusterDatasheet.V20;
                    break;
                case AllowedVoltages.V20x10:
                    sheetData = T200ThrusterDatasheet.V20x10;
                    break;
                case AllowedVoltages.V20x50:
                    sheetData = T200ThrusterDatasheet.V20x50;
                    break;
                case AllowedVoltages.V20x100:
                    sheetData = T200ThrusterDatasheet.V20x100;
                    break;
            }
            _logger = DataLogger.Instance.GetLogger<PwmLogRecord>($"{vehicle.transform.name}/{name}");
        }

        /// <summary>
        /// Apply force to the thruster location from datasheet and standardized pwm input
        /// </summary>
        /// <param name="pwmIn"> -1 - 1 value</param>
        /// <returns></returns>
        public Vector3 ApplyPwm(float pwmIn)
        {
            var level = WaterHeightSampler.Instance.GetWaterLevel(transform.position);
            if (transform.position.y > level)
            {
                force = 0f;
                return Vector3.zero;
            }

            pwmIn = Math.Max(-1, Math.Min(1, pwmIn));

            int step = (int)((pwmIn+1) / sheetStep); // push it to the range 0-2


            step = Math.Max(0, Math.Min(sheetData.Length - 1, step));
            // from kgf to N
            force = sheetData[step] * G;

            Vector3 forceVec = transform.forward * force;
            _vehicleBody.AddForceAtPosition(forceVec, transform.position, ForceMode.Force);

            _logger.Log(new PwmLogRecord { PwmIn = pwmIn, Force = forceVec});
            return forceVec;
        }

        public float GetPwmForForce(float force)
        {
            // from N to kgf
            force /= G;
            var closestIndex = BinarySearch(sheetData, force);

            return closestIndex * sheetStep - 1;
        }

        public static int BinarySearch(float[] a, float item)
        {
            int first = 0;
            int last = a.Length - 1;
            int mid = 0;
            do
            {
                mid = first + (last - first) / 2;
                if (item > a[mid])
                    first = mid + 1;
                else
                    last = mid - 1;
                if (a[mid] == item)
                    return mid;
            } while (first <= last);
            return mid;
        }

        private class PwmLogRecord
        {
            public float PwmIn { get; set; }
            public Vector3 Force { get; set; }
        }
    }

}