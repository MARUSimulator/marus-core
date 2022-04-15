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

using Marus.Actuators.Datasheets;
using Marus.Logger;
using Marus.Utils;
using UnityEngine;

namespace Marus.Actuators
{
    public class PwmThruster : MonoBehaviour
    {

        public enum AllowedVoltages
        {
            V10 = 10,
            V12 = 12,
            V14 = 14,
            V16 = 16,
            V18 = 18,
            V20 = 20
        };

        int _voltage;
        public AllowedVoltages voltage = AllowedVoltages.V10;
        float[] sheetData;
        float sheetStep;

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
                _vehicleBody = _vehicle.GetComponent<Rigidbody>();
                if (_vehicle == null)
                {
                    Debug.Log($@"Cannot get vehicle from sensor {transform.name}. 
                        Using sensor as the vehicle transform");
                    return transform;
                }
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
                    sheetData = T200ThrusterDatasheet.V10;
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
            int step = (int)((pwmIn+1) / sheetStep); // push it to the range 0-2

            // from kgf to N       
            float value = sheetData[step] * 9.80665f;

            Vector3 force = transform.forward * value;
            _vehicleBody.AddForceAtPosition(force, transform.position, ForceMode.Force);
            _logger.Log(new PwmLogRecord { PwmIn = pwmIn, Force = force});
            return force;
        }

        public float GetPwmForForce(float force)
        {
            // from N to kgf
            force /= 9.80665f;
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