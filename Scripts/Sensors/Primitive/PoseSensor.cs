using System;
using System.Collections;
using System.Collections.Generic;
using Auv;
using Labust.Networking;
using Sensorstreaming;
using UnityEngine;
using Labust.Utils;
using Labust.Core;
using Std;

namespace Labust.Sensors.Primitive
{
    /// <summary>
    /// Pose sensor implementation
    /// </summary>
    public class PoseSensor : SensorBase
    {
        [Header("Pose")] 
        public Vector3 position;
        public Quaternion orientation;
        public float verticalOffset;
        [Header("Twist")] 
        public Vector3 linearVelocity;
        public Vector3 angularVelocity;

        Rigidbody measuredObject;
      
        void Start()
        {
            measuredObject = Helpers.GetParentRigidBody(transform);
        }

        protected override void SampleSensor()
        {
            position = measuredObject.position;
            orientation = measuredObject.rotation;
            linearVelocity = measuredObject.transform.InverseTransformVector(measuredObject.velocity);
            angularVelocity = measuredObject.angularVelocity;
            Log(new { position, orientation });
            hasData = true;
        }
    }
}
