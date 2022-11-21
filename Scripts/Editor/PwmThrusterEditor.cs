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
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Marus.Actuators;
using Marus.Actuators.Datasheets;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Marus.Sensors
{
    /// <summary>
    /// Custom editor for PwmThruster component.
    /// Enables pwm thruster configuration loading, saving and modifying.
    /// </summary>
    [CustomEditor(typeof(PwmThruster))]
    public class PwmThrusterEditor : Editor
    {
        SerializedProperty voltage;
        SerializedObject pwmThrusterSO;
        PwmThruster myThruster;
        List<Type> thursterClasses;
        AnimationCurve previousCurve = new AnimationCurve();
        int previousVoltage = -1;
        int previousThruster = -1;
        void OnEnable()
        {
            pwmThrusterSO = new SerializedObject(target);
            myThruster = (PwmThruster)target;
            myThruster.sheetStep = T200ThrusterDatasheet.step;
            myThruster.sheetData = T200ThrusterDatasheet.V10;
        }

        public override void OnInspectorGUI()
        {
            pwmThrusterSO.Update();

            //Add thruster classes
            myThruster.thrusterType = getThrusterConfigs();
            myThruster.selectedThruster = EditorGUILayout.Popup("Thruster class", myThruster.selectedThruster, myThruster.thrusterType.ToArray()); 

            //Add voltages
            myThruster.allowedVoltages = getVoltages();
            bool customSelected = myThruster.selectedThruster >= thursterClasses.Count;  //if custom is selected
            
            EditorGUI.BeginDisabledGroup(customSelected);
            myThruster.selectedVoltage = EditorGUILayout.Popup("Voltage", myThruster.selectedVoltage, myThruster.allowedVoltages.ToArray()); 
            EditorGUI.EndDisabledGroup();

            //Add thurster Info
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Last force requested", myThruster.lastForceRequest);
            EditorGUILayout.FloatField("Time since force request", myThruster.timeSinceForceRequest);
            EditorGUILayout.FloatField("Pwm step", myThruster.sheetStep);
            EditorGUI.EndDisabledGroup();

            //Add curve
            if (!customSelected)
            {
                bool sdChanged = setSheetData();
                if(sdChanged) setCurveFromData();
            }
            else
            {
                setLinearCurve();
            }
            EditorGUI.BeginDisabledGroup(false);
            EditorGUILayout.CurveField(myThruster.curve, GUILayout.Height(150));
            EditorGUI.EndDisabledGroup();
            
            bool changeHappend = previousVoltage != myThruster.selectedVoltage || previousThruster != myThruster.selectedThruster;
            if(changeHappend)
            {
                previousVoltage = myThruster.selectedVoltage;
                previousThruster = myThruster.selectedThruster;
                pwmThrusterSO.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Setting sheet data form the setup in the config file
        /// </summary>
        bool setSheetData()
        {
            if(previousVoltage == myThruster.selectedVoltage && 
            previousThruster == myThruster.selectedThruster) return false;

            var thrClass = thursterClasses.ElementAt(myThruster.selectedThruster);
            var fieldName = myThruster.allowedVoltages.ElementAt(myThruster.selectedVoltage);
            var thrField = thrClass.GetField(fieldName);
            myThruster.sheetData = (float[])thrField.GetValue(thrClass);

            var stepField = thrClass.GetField("step");
            myThruster.sheetStep = (float)stepField.GetValue(thrClass);

            return true;
        }

        /// <summary>
        /// Setting curves to the inspector from the sheet data
        /// </summary>
        void setCurveFromData()
        { 
            AnimationCurve tempCurve = new AnimationCurve();
            float pwm_value = myThruster.sheetStep;
            foreach (float value in myThruster.sheetData)
            {
                tempCurve.AddKey(pwm_value, value);
                pwm_value += myThruster.sheetStep; 
            }
            myThruster.curve = tempCurve;
        }

        /// <summary>
        /// Get thruster data from the Assets/Scripts/Actuators/Datasheets
        /// </summary>
        private List<string> getThrusterConfigs()
        {
            thursterClasses = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => t.Namespace == "Marus.Actuators.Datasheets")
                        .ToList();
            List<string> thrusterString = new List<String>();

            for (int i = 0; i<thursterClasses.Count; i++)
            {
                thrusterString.Add(thursterClasses[i].Name);
            }
            thrusterString.Add("Custom");

            return thrusterString;
        }

        /// <summary>
        /// Get available voltages from the thruster class
        /// </summary>
        private List<string> getVoltages()
        {
            List<string> returnVoltages = new List<string>();
            if(myThruster.selectedThruster >= thursterClasses.Count)
            {
                return returnVoltages;
            }
            var props = thursterClasses.ElementAt(myThruster.selectedThruster).GetFields();
            for (int i = 0; i<props.Count(); i++)
            {
                if (props[i].Name.StartsWith('V') && (props[i].FieldType == typeof(float[]) || 
                    (props[i].FieldType == typeof(double[]) )))
                {
                    returnVoltages.Add(props[i].Name);
                }
            }

            return returnVoltages;    
        }

        /// <summary>
        /// Set curve as linear, starting point for custom
        /// </summary>
        private void setLinearCurve()
        {
            AnimationCurve tempCurve = new AnimationCurve();
            tempCurve.AddKey(0.0f, 0.0f);
            tempCurve.AddKey(1.0f, 1.0f);
            myThruster.curve = tempCurve;
        }

        

    }
}

#endif