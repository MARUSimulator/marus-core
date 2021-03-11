using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class EnumPropertyAttribute : PropertyAttribute
{
    public Type Type;
    public string[] Options;
    public string MethodName;
    public string PropertyName;


    public EnumPropertyAttribute(params string[] options) 
    {
        Options = options;
    }

    public EnumPropertyAttribute(Type type, string getOptionsMethodName)
    {
        Type = type;
        MethodName = getOptionsMethodName;
    }

    /// <summary>
    /// Used for singleton instances
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <param name="getOptionsMethodName"></param>
    public EnumPropertyAttribute(Type type, string propertyName, string getOptionsMethodName) : this(type, getOptionsMethodName)
    {
        PropertyName = propertyName;
    }
}
