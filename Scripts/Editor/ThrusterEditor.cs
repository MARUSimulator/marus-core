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
using System.Linq;
using System.Collections.Generic;
using System;

namespace Marus.Sensors
{
    /// <summary>
    /// Custom editor for Thruster component.
    /// Enables pwm thruster configuration loading, saving and modifying.
    /// </summary>
    [CustomEditor(typeof(Thruster))]
    public class ThrusterEditor : Editor
    {
        SerializedProperty voltage;
        SerializedObject ThrusterSO;
        Thruster myThruster;
        List<Type> thursterClasses;
        AnimationCurve previousCurve = new AnimationCurve();
        bool disableSaving = true;

        List<string> thursterNames;
        string newThrusterName;
        string thrusterFolderPath = "Assets/marus-core/Scripts/Actuators/Datasheets/";

        void OnEnable()
        {
            ThrusterSO = new SerializedObject(target);
            myThruster = (Thruster)target;
            myThruster.thrusters = GetAllInstances<ThrusterAsset>();
        }

        public override void OnInspectorGUI()
        {
            ThrusterSO.Update();
            thursterNames = getThursterNames();

            myThruster.selectedThrusterIndex = EditorGUILayout.Popup("Thruster", myThruster.selectedThrusterIndex, thursterNames.ToArray());

            ///If thruster changes
            if(myThruster.selectedThrusterIndex != myThruster.previousThrusterIndex )
            {
                myThruster.thrusters = GetAllInstances<ThrusterAsset>();
                myThruster.selectedThruster = myThruster.thrusters[myThruster.selectedThrusterIndex];
                CopyCurve(myThruster.currentCurve, myThruster.selectedThruster.curve);
                myThruster.previousThrusterIndex = myThruster.selectedThrusterIndex;
                newThrusterName = getInitialSavingName(myThruster.selectedThruster.name);

            }

            ///If curve is edited
            if (AreCurvesDifferent(myThruster.currentCurve, myThruster.selectedThruster.curve))
            {
                disableSaving = false;
            }
            else
            {
                disableSaving = true;
            }

            Rect bounds = new Rect();
            EditorGUILayout.CurveField("Curve", myThruster.currentCurve, new Color(1,1,1,1), bounds, GUILayout.Height(50));
            EditorGUI.BeginDisabledGroup(disableSaving);
            GUILayout.Space(2);
            var undoChanges = GUILayout.Button("Undo changes", EditorStyles.miniButtonRight);
            EditorGUI.EndDisabledGroup();

            if(undoChanges) myThruster.currentCurve.keys = myThruster.selectedThruster.curve.keys;

            EditorGUI.BeginDisabledGroup(disableSaving);
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Save/Edit");

            newThrusterName = EditorGUILayout.TextField("Thruster name", newThrusterName);
            GUILayout.Space(2);
            var saveButton = GUILayout.Button("Save", EditorStyles.miniButtonRight);
            if(saveButton)
            {
                SaveNewThruster(newThrusterName, myThruster.currentCurve);
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Info");
            EditorGUILayout.FloatField("Last force requested", myThruster.lastForceRequest);
            EditorGUILayout.FloatField("Time since force request", myThruster.timeSinceForceRequest);
            EditorGUI.EndDisabledGroup();

            ThrusterSO.ApplyModifiedProperties();
        }

        private List<string> getThursterNames()
        {
            List<string> nameList = new List<string>();
            foreach(var element in myThruster.thrusters)
            {
                nameList.Add(element.name);
            }
            return nameList;
        }

        /// <summary>
        /// Getting all instances of ThrusterAsset in the project
        /// </summary>
        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:"+ typeof(T).Name);
            T[] a = new T[guids.Length];
            for(int i =0;i<guids.Length;i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return a;
        }

        /// <summary>
        /// Saving new thruster if current by new name. Adding (n)
        /// to the name of the thruster if current name allready exists
        /// </summary>
        public void SaveNewThruster(string name, AnimationCurve curve)
        {
            ThrusterAsset newThruster = new ThrusterAsset();
            newThruster.name = name;
            CopyCurve(newThruster.curve, curve);
            string path = thrusterFolderPath + name + ".asset";
            AssetDatabase.CreateAsset(newThruster, path);
            myThruster.thrusters = GetAllInstances<ThrusterAsset>();
            myThruster.selectedThrusterIndex =  myThruster.thrusters.ToList().FindIndex((x)=>x.name == name);
            myThruster.previousThrusterIndex =-1;
        }

        public bool AreCurvesDifferent(AnimationCurve a1, AnimationCurve a2)
        {
            if (a1.length != a2.length) return true;
            for (int i = 0; i<a2.length; i++)
            {
                if(a1.keys[i].value != a2.keys[i].value || a1.keys[i].time != a2.keys[i].time)
                {
                    return true;
                }
            }
            return false;
        }

        public string getInitialSavingName(string thrusterName)
        {
            int i = 1;
            while (true)
            {
                if(!myThruster.thrusters.Where(x => x.name.Contains(thrusterName + "(" + i + ")")).Any())
                {
                    break;
                }
                else
                {
                    i++;
                }
            }
            string returnName = thrusterName + "(" + i + ")";
            return returnName;
        }

        public void CopyCurve(AnimationCurve a1, AnimationCurve a2)
        {
            a1.keys = a2.keys;
            a1.preWrapMode = a2.preWrapMode;
            a1.postWrapMode = a2.postWrapMode;
        }
    }
}

#endif