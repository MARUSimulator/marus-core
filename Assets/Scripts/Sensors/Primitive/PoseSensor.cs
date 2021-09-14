﻿using System.Collections;
using System.Collections.Generic;
using Labust.Networking;
using Sensorstreaming;
using UnityEngine;
using Labust.Utils;
using Labust.Core;

namespace Labust.Sensors.Primitive
{
    /// <summary>
    /// Pose sensor implementation
    /// </summary>
    public class PoseSensor : SensorBase<PoseStreamingRequest>
    {
        [Header("Pose")] 
        public Vector3 position;
        public Quaternion orientation;
        [Header("Twist")] 
        public Vector3 linearVelocity;
        public Vector3 angularVelocity;

        Rigidbody measuredObject;
      
        void Start()
        {
            measuredObject = Helpers.GetParentRigidBody(transform);
            streamHandle = streamingClient.StreamPoseSensor(cancellationToken:RosConnection.Instance.cancellationToken);
            AddSensorCallback(SensorCallbackOrder.Last, Refresh);
            if (string.IsNullOrEmpty(address))
                address = vehicle.name + "/pose";
        }

        public void Refresh()
        {
            position = measuredObject.position;
            orientation = measuredObject.rotation;
            linearVelocity = measuredObject.transform.InverseTransformVector(measuredObject.velocity);
            angularVelocity = measuredObject.angularVelocity;
            Log(new { position, orientation });
            hasData = true;
        }

        public async override void SendMessage()
        {
            var toRad = orientation.eulerAngles * Mathf.Deg2Rad;
            await _streamWriter.WriteAsync(new PoseStreamingRequest
            {
                Address = address,
                Pose = new Common.Pose
                {
                    Position = position.Unity2Standard().AsMsg(),
                    Orientation = toRad.Unity2Standard().AsMsg()
                }
            });
            hasData = false;
        }
    }
}
