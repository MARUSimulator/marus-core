using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumPropertyAttribute))]
public class EnumProperty : PropertyDrawer
{
    private int selectedIndex;
    private List<MethodInfo> _extensionMethods;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
        var attr = attribute as EnumPropertyAttribute;
        var options = attr.Options != null ? attr.Options : GetOptionsFromMethod(attr.Type, attr.PropertyName, attr.MethodName);
        if (options != null) 
        {
            int selectedIndex = Math.Max(0, options.ToList().IndexOf(property.stringValue));
            selectedIndex = EditorGUI.Popup(position, selectedIndex, options);
            property.stringValue = options[selectedIndex];
        }
        else 
            EditorGUI.PropertyField(position, property, label);

    }

    private string[] GetOptionsFromMethod(Type type, string propertyName, string methodName)
    {
        MethodInfo methodInfo;
        if (string.IsNullOrEmpty(propertyName))
        {
            methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                Debug.Log($"Given method '{methodName}' for property options must be public static string[]");
                return null;
            }
            if (methodInfo.ReturnType != typeof(string[]))
            {
                Debug.Log($"Method '{methodName}' defined in '{type.Name}' must have a return type of string[]");
                return null;
            }
            return (string[])methodInfo.Invoke(null, new object[0]);
        }
        // else get method from property
        var propertyInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
        if (propertyInfo == null)
        {
            Debug.Log($"Given property '{propertyName}' on type '{type.Name}' for property options must be public static");
            return null;
        }
        methodInfo = propertyInfo.PropertyType.GetMethod(methodName);
        if (methodInfo == null || !(methodInfo?.ReturnType == typeof(string[])))
        {
            Debug.Log($"Method '{methodName}' defined in '{type.Name}' must be public and have a return type string[]");
            return null;
        }
        var instance = propertyInfo.GetValue(null); // it is static, so it is ok
        return (string[])methodInfo.Invoke(instance, new object[0]);
    }
}
