using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Simulator.Sensors
{
    public class PoseSensor : MonoBehaviour, ISensor
    {
        [Header("Pose")] 
        public Vector3 position;
        public Quaternion orientation;
        [Header("Twist")] 
        public Vector3 linearVelocity;
        public Vector3 angularVelocity;

        public Rigidbody measuredObject;
      
        void Start()
        {
            measuredObject = GetComponentInParent<Rigidbody>();
        }

        public void SampleSensor()
        {
            position = measuredObject.position;
            orientation = measuredObject.rotation;
            linearVelocity = measuredObject.transform.InverseTransformVector(measuredObject.velocity);
            angularVelocity = measuredObject.angularVelocity;
        }
    }
}
