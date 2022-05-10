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

using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using System.IO;
using Marus.Utils;
using System.Linq;

namespace Marus.Sensors
{
    /// <summary>
    /// Custom editor for RaycastLidar component.
    /// Enables lidar configuration loading, saving and modifying.
    /// </summary>
    [CustomEditor(typeof(RaycastLidar))]
    public class RaycastLidarEditor : Editor
    {
        List<LidarConfig> Configs = null;
        string[] _choices;
        int _confingIndex = 0;
        int _oldConfigIndex = 0;
        RaycastLidar lidarObj;
        bool showConfig = false;
        string _configName;

        float _infoMsgStamp = float.MinValue;
        string _infoMsg = "";
        float _infoMsgTimeout = 10f;

        float _errorMsgStamp = float.MinValue;
        string _errorMsg = "";
        float _errorMsgTimeout = 10f;
        LidarConfig currentConfig;
        bool _configsChanged = false;

        public void Reset()
        {
            InitAvailableLidarConfigs();
            lidarObj = target as RaycastLidar;
            if (lidarObj.ConfigIndex >= Configs.Count)
            {
                lidarObj.ConfigIndex = 0;
            }
            currentConfig = Configs[lidarObj.ConfigIndex];
            lidarObj.ApplyLidarConfig();
        }

        public override void OnInspectorGUI()
        {
            if (Configs == null || _configsChanged)
            {
                InitAvailableLidarConfigs();
                _configsChanged = false;
            }

            lidarObj = target as RaycastLidar;
            _oldConfigIndex = lidarObj.ConfigIndex;
            var label = new GUIContent("Lidar Configuration");
            lidarObj.Configs = Configs;
            lidarObj.ConfigIndex = EditorGUILayout.Popup(label, lidarObj.ConfigIndex, _choices);

            currentConfig = Configs[lidarObj.ConfigIndex];
            var placeHolderText = (currentConfig.Name == "Custom" || currentConfig.Name == "Uniform") ? "" : currentConfig.Name;

            if (_oldConfigIndex != lidarObj.ConfigIndex)
            {
                currentConfig = Configs[lidarObj.ConfigIndex];
                _configName = placeHolderText;
            }

            _configName = EditorGUILayout.TextField("Config name: ", _configName);

            if ((Time.time - _infoMsgStamp) < _infoMsgTimeout)
            {
                EditorGUILayout.HelpBox(_infoMsg, MessageType.Info);
            }

            if ((Time.time - _errorMsgStamp) < _errorMsgTimeout)
            {
                EditorGUILayout.HelpBox(_errorMsg, MessageType.Error);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button ("Save changes", EditorStyles.miniButtonLeft))
            {
                SaveConfig(currentConfig.Name, _configName, false);
            }
            if (GUILayout.Button ("Remove", EditorStyles.miniButtonRight))
            {
                RemoveConfig(currentConfig.Name);
            }
            EditorGUILayout.EndHorizontal();


            var level = EditorGUI.indentLevel;
            if (lidarObj.Configs[lidarObj.ConfigIndex].Type == RayDefinitionType.Intervals)
            {
                var list = serializedObject.FindProperty("_rayIntervals");
                showConfig = EditorGUILayout.Foldout(showConfig, "Custom Ray Intervals");
                if (showConfig)
                {
                    serializedObject.Update();
                    CheckIntervals();
                    EditorGUI.indentLevel += 2;
                    EditorGUILayout.LabelField("Custom Ray Configuration", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(list, true);
                    EditorUtility.SetDirty(lidarObj);
                    serializedObject.ApplyModifiedProperties();
                }
            }

            else if (lidarObj.Configs[lidarObj.ConfigIndex].Type == RayDefinitionType.Angles)
            {
                showConfig = EditorGUILayout.Foldout(showConfig, "Custom Ray Angles");
                if (showConfig)
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUILayout.LabelField("Custom Ray Angles", EditorStyles.boldLabel);
                    var i = 1;
                    foreach (var angle in currentConfig.ChannelAngles)
                    {
                        EditorGUILayout.LabelField($"Ray {i++}:     {angle}");
                    }
                }
            }

            if (EditorGUI.indentLevel != level) EditorGUI.indentLevel = level;

            serializedObject.DrawInspectorExcept(new string[]{ "_rayIntervals", "Script", "_rayType"});

            if (GUILayout.Button ("Save as new", EditorStyles.miniButton))
            {
                SaveConfig(currentConfig.Name, _configName, true);
            }

            if (lidarObj.ConfigIndex != _oldConfigIndex)
            {
                RefreshLidar();
            }
        }
        public void InitAvailableLidarConfigs()
        {
            var jsonText = File.ReadAllText(JsonConfigPath());
            Configs = JsonConvert.DeserializeObject<List<LidarConfig>>(jsonText);
            lidarObj = target as RaycastLidar;
            lidarObj.Configs = Configs;
            _choices = new string[Configs.Count];
            var i = 0;
            foreach(var cfg in Configs)
            {
                _choices[i++] = cfg.Name;
            }
        }

        public void RefreshLidar()
        {
            lidarObj = target as RaycastLidar;
            lidarObj.Configs = Configs;
            Undo.RegisterCompleteObjectUndo(lidarObj.gameObject, "refresh lidar");
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
            lidarObj.ApplyLidarConfig();
            EditorUtility.SetDirty(lidarObj);
        }

        public void CheckIntervals()
        {
            lidarObj = target as RaycastLidar;
            var list = lidarObj._rayIntervals;
            if (list.Count == 0) return;
            lidarObj.HeightRes = lidarObj._rayIntervals.Sum(x => x.NumberOfRays);
            for (int i = 0; i < list.Count; i++)
            {

                if ((i < list.Count - 1) && list[i].EndingAngle != list[i+1].StartingAngle)
                {
                    EditorGUILayout.HelpBox("Starting angle of interval must be equal to ending angle of previous one.", MessageType.Error);
                    return;
                }
                if (list[i].NumberOfRays <= 0)
                {
                    EditorGUILayout.HelpBox("Number of rays must be at least 1!", MessageType.Error);
                    return;
                }
                if (list[i].StartingAngle >= list[i].EndingAngle)
                {
                    EditorGUILayout.HelpBox("Ending angle must be greater than starting angle!", MessageType.Error);
                    return;
                }
            }

        }

        /// <summary>
        /// Save lidar configuration to local json file.
        /// </summary>
        /// <param name="oldName">Previous config name</param>
        /// <param name="newName">New config name</param>
        /// <param name="saveAsNew">True if saving new configuration, false when overwriting old one.</param>
        /// <returns>True if saved, false if not saved.</returns>
        public bool SaveConfig(string oldName, string newName, bool saveAsNew)
        {
            if (string.IsNullOrEmpty(newName))
            {
                //Reject empty name
                _errorMsg = $"You must enter a name for configuration!";
                _errorMsgStamp = Time.time;
                return false;
            }

            if ((oldName == "Custom" || oldName == "Uniform") && !saveAsNew)
            {
                //Reject overwriting custom and uniform templates
                _errorMsg = $"Can only save {oldName} as new configuration!";
                _errorMsgStamp = Time.time;
                return false;
            }

            lidarObj = target as RaycastLidar;
            var jsonTextFile = File.ReadAllText(JsonConfigPath());

            var index = Configs.FindIndex(a => a.Name == newName);
            var oldIdx = Configs.FindIndex(a => a.Name == oldName);
            var oldConfig = Configs[oldIdx];
            if (index != -1 && saveAsNew)
            {
                //Reject saving new config with same name
                _errorMsg = $"Config with name {newName} already exists!";
                _errorMsgStamp = Time.time;
                return false;
            }

            var newCfg = new LidarConfig();
            newCfg.Name = newName;
            if (lidarObj._rayIntervals.Count != 0)
            {
                newCfg.RayIntervals = lidarObj._rayIntervals;
            }
            else if (oldConfig.ChannelAngles.Count != 0)
            {
                newCfg.ChannelAngles = oldConfig.ChannelAngles;
            }
            newCfg.MaxRange = lidarObj.MaxDistance;
            newCfg.MinRange = lidarObj.MinDistance;
            newCfg.HorizontalResolution = lidarObj.WidthRes;
            newCfg.VerticalResolution = lidarObj.HeightRes;
            newCfg.Frequency = lidarObj.SampleFrequency;
            newCfg.HorizontalFieldOfView = lidarObj.HorizontalFieldOfView;
            newCfg.VerticalFieldOfView = lidarObj.VerticalFieldOfView;
            newCfg.FrameId = lidarObj.frameId;
            if (!saveAsNew)
            {
                Configs[oldIdx] = newCfg;
            }
            else
            {
                Configs.Add(newCfg);
            }

            using (StreamWriter file = File.CreateText(JsonConfigPath()))
            {
                var s = JsonConvert.SerializeObject(Configs, Formatting.Indented);
                file.Write(s);
            }
            _infoMsg = $"Config with name {newName} saved!";
            _infoMsgStamp = Time.time;
            InitAvailableLidarConfigs();
            RefreshLidar();
            lidarObj.ApplyLidarConfig();
            _configsChanged = true;
            return true;
        }

        /// <summary>
        /// Deletes configuration from local json file.
        /// </summary>
        /// <param name="configName">Name of config to be removed.</param>
        /// <returns></returns>
        public bool RemoveConfig(string configName)
        {
            lidarObj = target as RaycastLidar;
            if ((configName == "Custom" || configName == "Uniform"))
            {
                _errorMsg = $"Cannot delete {configName}!";
                _errorMsgStamp = Time.time;
                return false;
            }

            var index = Configs.FindIndex(a => a.Name == configName);
            if (index == -1)
            {
                _errorMsg = $"Cannot delete {configName}, error occured!";
                _errorMsgStamp = Time.time;
                return false;
            }
            Configs.RemoveAt(index);
            using (StreamWriter file = File.CreateText(JsonConfigPath()))
            {
                var s = JsonConvert.SerializeObject(Configs, Formatting.Indented);
                file.Write(s);
            }
            _infoMsg = $"Config with name {configName} removed!";
            _infoMsgStamp = Time.time;
            _configsChanged = true;
            lidarObj.ConfigIndex--;
            return true;
        }

        string JsonConfigPath()
        {
            var jsonTextFile = Resources.Load<TextAsset>("Configs/lidars");
            return Path.Combine(Path.GetDirectoryName(Application.dataPath), AssetDatabase.GetAssetPath(jsonTextFile));
        }
    }
}

#endif