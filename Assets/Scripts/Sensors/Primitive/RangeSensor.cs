using System;
using UnityEngine;

namespace Labust.Sensors
{
    // TODO
    public class RangeSensor : MonoBehaviour
    {
        public float range;
        public float maxRange = 120;

        private Transform sensor;

        void Start()
        {
            sensor = GetComponent<Transform>();
        }

        public void SampleSensor()
        {
            RaycastHit hit;
            if (UnityEngine.Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit,
                maxRange))
            {
                range = hit.distance;
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.up) * hit.distance,
                    Color.yellow);
            }
            else
            {
                range = Single.NaN;
            }
        }
    }
}