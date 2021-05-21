using System;
using System.Collections.Generic;
using Simulator.Sensors;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public class Nanomodem : MonoBehaviour
{
    public float maxRange = 500;
    public int id = 0;
    public AcousticMedium medium;
    
    private Transform sensor;
    protected void Start()
    {
        sensor = GetComponent<Transform>();
    }

    public (bool, float) GetRangeAndValidityToId(int id)
    {
        if (!medium.modems.ContainsKey(id))
        {
            Debug.LogError("ID:" + id + " unknown.");
            return (false, -1.0f);
        }
        
        var range = Vector3.Distance(medium.modems[id].transform.position, sensor.position);

        if (range > maxRange)
        {
            Debug.Log("ID:" + id + " is too far.");
            return (false, range);
        }
        
        return (true, range);
    }
}