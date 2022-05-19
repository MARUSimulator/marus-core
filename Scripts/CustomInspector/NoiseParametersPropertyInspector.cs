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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Marus.NoiseDistributions;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NoiseParameters))]
public class NoiseParametersPropertyInspector : PropertyDrawer
{

    string[] _noiseOptions;
    float _colapsedHeight;
    float deltaLine;

    BindingFlags _fieldFlags = 
        BindingFlags.Instance | BindingFlags.Public;


    static Dictionary<string, bool> _isExpanded = 
        new Dictionary<string, bool>();
    static Dictionary<string, int> _selectedIndex =
        new Dictionary<string, int>();
    static Dictionary<string, Dictionary<string, List<string>>> _currentValues =
        new Dictionary<string, Dictionary<string, List<string>>>();

    static Dictionary<string, string> _previousSelectedNoise =
        new Dictionary<string, string>();
    Dictionary<string, List<FieldInfo>> _noiseTypeFields;

    // used for detection that noise type changed


    public NoiseParametersPropertyInspector() : base()
    {
        deltaLine = EditorGUIUtility.singleLineHeight + 3;
        _colapsedHeight = 1 * deltaLine;

        _noiseOptions = Noise.NoiseTypes.Select(x => x.Name).ToArray();
        InitNoiseTypeFields();
    }

    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        var id = GetPropertyId(property);
        int selectedIndex = 0;
        if (!_selectedIndex.TryGetValue(id, out selectedIndex))
        {
            _selectedIndex[id] = 0;
        }
        if (_noiseOptions.Length == 0
            || selectedIndex == -1)
        {
            return _colapsedHeight;
        }
        var selectedNoiseType = Noise.NoiseTypes[selectedIndex];
        var fields = _noiseTypeFields[selectedNoiseType.FullName];
        var expandedHeight = fields.Count * deltaLine + 3 * _colapsedHeight;
        if (_isExpanded.TryGetValue(id, out var isExpanded))
        {
            return isExpanded ? expandedHeight : _colapsedHeight;
        }
        return _colapsedHeight;
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        var id = GetPropertyId(property);
        if (!_currentValues.TryGetValue(id, out var val))
        {
            InitCurrentValuesWithProperty(property);
        }

        var prop = property.FindPropertyRelative("NoiseTypeFullName");
        int selectedIndex = 0;
        if (!_selectedIndex.TryGetValue(id, out selectedIndex))
        {
            _selectedIndex[id] = 0;
        }
        if (!string.IsNullOrEmpty(prop.stringValue))
        {
            selectedIndex = Array.IndexOf(
                    Noise.NoiseTypes.Select(x => x.FullName).ToArray(), prop.stringValue);
        }

        bool isExpanded;
        if (!_isExpanded.TryGetValue(id, out isExpanded))
        {
            isExpanded = false;
            _isExpanded[id] = isExpanded;
        }


        if (isExpanded)
        {
            position.Set(position.x, position.y, position.width, deltaLine + EditorGUIUtility.singleLineHeight);
        }
        isExpanded = EditorGUI.Foldout(position, isExpanded, label, true);
        position.Set(position.x, position.y + deltaLine, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.indentLevel = 1;
        if (isExpanded)
        {
            position.Set(position.x, position.y + deltaLine, position.width, EditorGUIUtility.singleLineHeight);
            selectedIndex = EditorGUI.Popup(position, "NoiseType", selectedIndex, _noiseOptions);
            selectedIndex = selectedIndex < 0 ? 0 : selectedIndex;
            var selectedNoiseType = Noise.NoiseTypes[selectedIndex];
            // Debug.Log(string.Join(" ", _currentValues[selectedNoiseType.FullName]));
            if (selectedIndex > -1)
            {
                prop.stringValue = selectedNoiseType.FullName;

                var paramKeys = property.FindPropertyRelative("ParameterKeys");
                var paramValues = property.FindPropertyRelative("ParameterValues");
                var fields = _noiseTypeFields[selectedNoiseType.FullName];

                var prevNoise = _previousSelectedNoise.GetValueOrDefault(id);
                if (selectedNoiseType.FullName != prevNoise)
                {
                    // Debug.Log($"{paramKeys.arraySize}  {fields.Count}");
                    paramKeys.ClearArray();
                    paramValues.ClearArray();
                    paramKeys.arraySize = fields.Count;
                    paramValues.arraySize = fields.Count;

                    for (var i = 0; i < fields.Count; i++)
                    {
                        var el = paramKeys.GetArrayElementAtIndex(i);
                        el.stringValue = fields[i].Name;
                        var list = _currentValues[id][selectedNoiseType.FullName];
                        if (list.Count > 0)
                        {
                            el = paramValues.GetArrayElementAtIndex(i);
                            el.stringValue = list[i];
                        }
                    }
                }

                var valueList = _currentValues[id][selectedNoiseType.FullName];
                valueList.Clear();
                // Debug.Log($"Keys {paramKeys.arraySize} Values {paramValues.arraySize}");
                for (var i = 0; i < fields.Count; i++)
                {
                    var f = fields[i];
                    var el = paramValues.GetArrayElementAtIndex(i);
                    position.Set(position.x, position.y + deltaLine, position.width, EditorGUIUtility.singleLineHeight);
                    bool parsed;
                    if (f.FieldType == typeof(int))
                    {
                        parsed = int.TryParse(el.stringValue, out var v);
                        var newInt = EditorGUI.IntField(position, f.Name, parsed ? v : 0);
                        el.stringValue = newInt.ToString();
                    }
                    else if (f.FieldType == typeof(float))
                    {
                        parsed = float.TryParse(el.stringValue, out var v);
                        var newFloat = EditorGUI.FloatField(position, f.Name, parsed ? v : 0);
                        el.stringValue = newFloat.ToString();
                    }
                    else if (f.FieldType == typeof(string))
                    {
                        var newText = EditorGUI.TextField(position, f.Name, el.stringValue);
                        el.stringValue = newText;
                    }
                    valueList.Add(el.stringValue);
                }
            }
            _selectedIndex[id] = selectedIndex;
            _previousSelectedNoise[id] = selectedNoiseType.FullName;
        }
        _isExpanded[id] = isExpanded;
        var name = Noise.NoiseTypes[selectedIndex].FullName;
        var p = new NoiseParameters
        {
            NoiseTypeFullName = name,
            ParameterKeys = _noiseTypeFields[name].Select(x => x.Name).ToList(),
            ParameterValues = _currentValues[id][name]
        };
        EditorGUI.EndProperty();
    }

    private void InitCurrentValuesWithProperty(SerializedProperty prop)
    {
        var id = GetPropertyId(prop);
        _currentValues[id] = new Dictionary<string, List<string>>();

        foreach (var typ in Noise.NoiseTypes)
        {
            var name = typ.FullName;
            if (!_currentValues[id].ContainsKey(name))
            {
                _currentValues[id][name] = new List<string>();
            }
        }
    }

    private void InitNoiseTypeFields()
    {
        _noiseTypeFields = new Dictionary<string, List<FieldInfo>>();

        foreach (var typ in Noise.NoiseTypes)
        {
            var name = typ.FullName;
            _noiseTypeFields[name] = new List<FieldInfo>();
            var fields = typ.GetFields(_fieldFlags)
                .Where(x => Noise.IsAllowedNoiseType(x.FieldType));
            foreach (var f in fields)
            {
                _noiseTypeFields[name].Add(f);
            }
        }
    }

    private string GetPropertyId(SerializedProperty property)
    {
        return property.serializedObject.targetObject.GetInstanceID().ToString() + property.name;
    }


}
#endif