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
    /// Enables thruster configuration loading, saving and modifying.
    /// </summary>
    [CustomEditor(typeof(Thruster))]
    public class ThrusterEditor : Editor
    {
        SerializedObject ThrusterSO;
        Thruster myThruster;
        bool disableSaving = true;
        List<string> thursterNames;
        string newThrusterName;
        string thrusterFolderPath = "Assets/marus-core/Datasheets/";
        List<ThrusterAsset> thrusters;
        int previousThrusterIndex = -1;
        int selectedThrusterIndex = 0;
        AnimationCurve currentCurve = new AnimationCurve();

        void OnEnable()
        {
            ThrusterSO = new SerializedObject(target);
            myThruster = (Thruster)target;
            thrusters = GetAllInstances<ThrusterAsset>().ToList();
            thursterNames = thrusters.Select(x => x.name).ToList();
        }

        public override void OnInspectorGUI()
        {
            ThrusterSO.Update();

            selectedThrusterIndex = GetThrusterIndex(myThruster);

            selectedThrusterIndex = EditorGUILayout.Popup("Thruster", selectedThrusterIndex, thursterNames.ToArray());

            ///If thruster changes or thruster selected thruster in not yet
            if(selectedThrusterIndex != previousThrusterIndex || myThruster.thrusterAsset == default(ThrusterAsset) )
            {
                myThruster.thrusterAsset = thrusters[selectedThrusterIndex];
                currentCurve = CopyCurve(myThruster.thrusterAsset.curve);
                previousThrusterIndex = selectedThrusterIndex;
                newThrusterName = GetInitialSavingName(myThruster.thrusterAsset.name);
            }

            ///If curve is edited
            if (currentCurve.Equals(myThruster.thrusterAsset.curve))
            {
                disableSaving = true;
            }
            else
            {
                disableSaving = false;
            }
            Rect bounds = new Rect();
            EditorGUILayout.CurveField("Curve", currentCurve, new Color(1,1,1,1), bounds, GUILayout.Height(50));

            EditorGUI.BeginDisabledGroup(disableSaving);
            GUILayout.Space(2);
            var undoChanges = GUILayout.Button("Undo changes", EditorStyles.miniButtonRight);
            EditorGUI.EndDisabledGroup();

            if(undoChanges) currentCurve = CopyCurve(myThruster.thrusterAsset.curve);

            EditorGUI.BeginDisabledGroup(disableSaving);
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Save/Edit");

            newThrusterName = EditorGUILayout.TextField("Thruster name", newThrusterName);
            GUILayout.Space(2);
            var saveButton = GUILayout.Button("Save", EditorStyles.miniButtonRight);
            if(saveButton)
            {
                SaveNewThruster(newThrusterName, currentCurve);
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

        /// <summary>
        /// Getting all instances of ThrusterAsset in the project
        /// </summary>
        private static T[] GetAllInstances<T>() where T : ScriptableObject
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
        private void SaveNewThruster(string name, AnimationCurve curve)
        {
            ThrusterAsset newThruster = new ThrusterAsset();
            newThruster.name = name;
            newThruster.curve = CopyCurve(curve);
            string path = String.Format("{0}{1}.asset", thrusterFolderPath, name);
            AssetDatabase.CreateAsset(newThruster, path);

            thrusters = GetAllInstances<ThrusterAsset>().ToList();
            selectedThrusterIndex =  thrusters.ToList().FindIndex((x)=>x.name == name);
            myThruster.thrusterAsset = thrusters[selectedThrusterIndex];
            thursterNames = thrusters.Select(x => x.name).ToList();
            previousThrusterIndex =-1;
        }

        private string GetInitialSavingName(string thrusterName)
        {
            int i = 1;
            while (thrusters.Any(x => x.name.Contains(String.Format("{0}({1})", thrusterName, i)))) i++;
            string returnName = String.Format("{0}({1})", thrusterName, i);
            return returnName;
        }

        private static AnimationCurve CopyCurve(AnimationCurve a)
        {
            AnimationCurve newCurve = new AnimationCurve(a.keys);
            newCurve.preWrapMode = a.preWrapMode;
            newCurve.postWrapMode = a.postWrapMode;
            return newCurve;
        }

        private int GetThrusterIndex(Thruster thruster)
        {
            int index = thrusters.IndexOf(thruster.thrusterAsset);
            if(index == -1) index = 0;
            return index;
        }
    }
}

#endif