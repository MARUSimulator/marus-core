using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;

namespace Labust.Sensors
{
    // TODO
    public class DvlSensor : MonoBehaviour
    {
        [Header("Core measurements")]
        public Vector3 groundVelocity = new Vector3();
        public float altitude;

        [Header("Beams")]
        public float[] beamRanges;
        public RangeSensor[] beams;

        private Transform sensor;
        private Vector3 lastPosition;

        void Start()
        {
            sensor = GetComponent<Transform>();
            beams = GetComponentsInChildren<RangeSensor>();
            beamRanges = new float[beams.Length];
            lastPosition = sensor.position;
        }

        void FixedUpdate()
        {
            var position = sensor.position;
            groundVelocity = sensor.worldToLocalMatrix * ((position - lastPosition) / Time.fixedDeltaTime);

            lastPosition = position;
        }

        public void SampleSensor()
        {
            altitude = Single.MaxValue;
            for (int i = 0; i < beams.Length; ++i)
            {
                beams[i].SampleSensor();

                if (beams[i].range < altitude)
                    altitude = beams[i].range;

                beamRanges[i] = beams[i].range;
            }
        }
    }
}
