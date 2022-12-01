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
        SerializedObject _ThrusterSO;
        Thruster _myThruster;
        bool _disableSaving = true;
        List<string> _thursterNames;
        string _newThrusterName;
        string _thrusterFolderPath = "Assets/marus-core/Datasheets/";
        List<ThrusterAsset> _thrusters;
        int _previousThrusterIndex = -1;
        int _selectedThrusterIndex = 0;
        AnimationCurve _currentCurve = new AnimationCurve();

        void OnEnable()
        {
            _ThrusterSO = new SerializedObject(target);
            _myThruster = (Thruster)target;
            _thrusters = GetAllInstances<ThrusterAsset>().ToList();
            _thursterNames = _thrusters.Select(x => x.name).ToList();
        }

        public override void OnInspectorGUI()
        {
            _ThrusterSO.Update();

            _selectedThrusterIndex = GetThrusterIndex(_myThruster);

            _selectedThrusterIndex = EditorGUILayout.Popup("Thruster", _selectedThrusterIndex, _thursterNames.ToArray());

            ///If thruster changes or thruster selected thruster in not yet
            if(_selectedThrusterIndex != _previousThrusterIndex || _myThruster.ThrusterAsset == default(ThrusterAsset) )
            {
                _myThruster.ThrusterAsset = _thrusters[_selectedThrusterIndex];
                _currentCurve = CopyCurve(_myThruster.ThrusterAsset.curve);
                _previousThrusterIndex = _selectedThrusterIndex;
                _newThrusterName = GetInitialSavingName(_myThruster.ThrusterAsset.name);
            }

            ///If curve is edited
            if (_currentCurve.Equals(_myThruster.ThrusterAsset.curve))
            {
                _disableSaving = true;
            }
            else
            {
                _disableSaving = false;
            }
            Rect bounds = new Rect();
            EditorGUILayout.CurveField("Curve", _currentCurve, new Color(1,1,1,1), bounds, GUILayout.Height(50));

            EditorGUI.BeginDisabledGroup(_disableSaving);
            GUILayout.Space(2);
            var undoChanges = GUILayout.Button("Undo changes", EditorStyles.miniButtonRight);
            EditorGUI.EndDisabledGroup();

            if(undoChanges) _currentCurve = CopyCurve(_myThruster.ThrusterAsset.curve);

            EditorGUI.BeginDisabledGroup(_disableSaving);
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Save/Edit");

            _newThrusterName = EditorGUILayout.TextField("Thruster name", _newThrusterName);
            GUILayout.Space(2);
            var saveButton = GUILayout.Button("Save", EditorStyles.miniButtonRight);
            if(saveButton)
            {
                SaveNewThruster(_newThrusterName, _currentCurve);
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Info");
            EditorGUILayout.FloatField("Last force requested", _myThruster.LastForceRequest);
            EditorGUILayout.FloatField("Time since force request", _myThruster.TimeSinceForceRequest);
            EditorGUI.EndDisabledGroup();

            _ThrusterSO.ApplyModifiedProperties();
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
            string path = String.Format("{0}{1}.asset", _thrusterFolderPath, name);
            AssetDatabase.CreateAsset(newThruster, path);

            _thrusters = GetAllInstances<ThrusterAsset>().ToList();
            _selectedThrusterIndex =  _thrusters.ToList().FindIndex((x)=>x.name == name);
            _myThruster.ThrusterAsset = _thrusters[_selectedThrusterIndex];
            _thursterNames = _thrusters.Select(x => x.name).ToList();
            _previousThrusterIndex =-1;
        }

        private string GetInitialSavingName(string thrusterName)
        {
            int i = 1;
            while (_thrusters.Any(x => x.name.Contains(String.Format("{0}({1})", thrusterName, i)))) i++;
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
            int index = _thrusters.IndexOf(thruster.ThrusterAsset);
            if(index == -1) index = 0;
            return index;
        }
    }
}

#endif