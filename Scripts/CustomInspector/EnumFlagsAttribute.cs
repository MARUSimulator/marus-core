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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

public class EnumFlagsAttribute : PropertyAttribute
{
    public Type EnumType;
    public EnumFlagsAttribute(Type t) 
    { 
        if (t.IsEnum)
        {
            EnumType = t;
            return;
        }
        Debug.Log($"Given type {t.Name} is not an enum");
    }

    public static List<string> ReturnSelectedElements(int value, Type enumType)
    {
        var enumNames = Enum.GetNames(enumType);
        var selectedElements = new List<string>();
        for (int i = 0; i < enumNames.Length; i++)
        {
            int layer = 1 << i;
            if ((value & layer) != 0)
            {
                selectedElements.Add(enumNames[i]);
            }
        }
        return selectedElements;
    }
}

[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        if (attribute is EnumFlagsAttribute enumFlags)
        {
            string[] names = Enum.GetNames(enumFlags.EnumType);
            _property.intValue = EditorGUI.MaskField( _position, _label, _property.intValue, names);
        }
    }
}