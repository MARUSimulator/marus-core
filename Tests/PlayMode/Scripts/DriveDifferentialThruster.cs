using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Marus.Actuators;

public class DriveDifferentialThruster : MonoBehaviour
{

    float _totalTime;
    public float swTime = 5;
    public DifferentialThrusterController controller;
    float[] values;
    int k = 1;

    // Start is called before the first frame update
    void Start()
    {
        values = new float[4] {0, 0, 0, 0};
    }

    // Update is called once per frame
    void Update()
    {
        _totalTime += Time.deltaTime;
        if (_totalTime > swTime)
        {
            k *= -1;
            _totalTime = 0;
        }

        values[0] = 0.0f;
        values[2] = 0.0f;
        values[1] = k * Mathf.PI / 2;
        values[3] = -k * Mathf.PI / 2;
        controller.ApplyInput(values);
    }
}
