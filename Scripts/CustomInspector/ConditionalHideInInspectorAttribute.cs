using UnityEngine;
using System;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: -

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ConditionalHideInInspectorAttribute : PropertyAttribute
{
    public readonly string ConditionalSourceField;
    public bool HideInInspector = false;
    public bool Inverse = false;
    public object Value;

    // Use this for initialization
    public ConditionalHideInInspectorAttribute(string conditionalSourceField, bool inverse=false)
    {
        ConditionalSourceField = conditionalSourceField;
        Inverse = inverse;
    }

    public ConditionalHideInInspectorAttribute(string conditionalSourceField, bool inverse=false, object value=null)
    {
        ConditionalSourceField = conditionalSourceField;
        Inverse = inverse;
        Value = value;
    }
}