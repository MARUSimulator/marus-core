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