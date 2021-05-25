using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Labust.Actuators.Datasheets;
using UnityEngine;
using UnityEngine.UI;

namespace Labust.Actuators
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

        Rigidbody _body;
        Rigidbody body
        {
            get
            {
                if (_body == null)
                {
                    var component = GetComponent<Rigidbody>();
                    if (component != null)
                        _body = component;
                    else
                        _body = Utils.Helpers.GetParentRigidBody(transform);
                }
                return _body;
            }
        }

        // Start is called before the first frame update
        void Awake()
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
        }

        /// <summary>
        /// Apply force to the thruster location from datasheet and standardized pwm input
        /// </summary>
        /// <param name="pwmIn"> -1 - 1 value</param>
        /// <returns></returns>
        public bool ApplyPwm(double pwmIn)
        {
            int step = (int)(pwmIn / sheetStep);

            // from kgf to N       
            float value = sheetData[step] * 9.80665f;

            Vector3 force = transform.forward * value;
            _body.AddForceAtPosition(force, transform.position, ForceMode.Force);
            return true;
        }

    }

}