﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labust.Sensors.Primitive
{
    //TODO
    public class GNSSSensor : MonoBehaviour
    {
        [Header("Position")]
        public GeographicFrame origin;
        public GeoPoint fix;

        [Header("Precision")]
        public double[] covariance;
        public bool isRTK = true;

        private Transform sensor;

        void Start()
        {
            sensor = GetComponent<Transform>();
            covariance = new double[] { 0.1, 0, 0, 0, 0.1, 0, 0, 0, 0.1 };
        }

        public void SampleSensor()
        {
            fix = origin.Unity2Geo(sensor.position);
        }
    }
}