using UnityEngine;
using System;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: -

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ConditionalHideInInspectorAttribute : PropertyAttribute
{
    public readonly string ConditionalSourceField;
    public string ConditionalSourceField2 = "";
    public bool HideInInspector = false;
    public bool Inverse = false;

    // Use this for initialization
    public ConditionalHideInInspectorAttribute(string conditionalSourceField, bool inverse=false)
    {
        ConditionalSourceField = conditionalSourceField;
        Inverse = inverse;
    }
}