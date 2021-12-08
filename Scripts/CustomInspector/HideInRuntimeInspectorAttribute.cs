using UnityEngine;
using System;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: -

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class HideInRuntimeInspectorAttribute : PropertyAttribute
{

    // Use this for initialization
    public HideInRuntimeInspectorAttribute()
    {
    }
}