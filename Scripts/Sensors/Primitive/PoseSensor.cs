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
    public class PoseSensor : SensorBase<PoseStreamingRequest>
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
            streamHandle = streamingClient?.StreamPoseSensor(cancellationToken:RosConnection.Instance.cancellationToken);
            if (string.IsNullOrEmpty(address))
                address = vehicle.name + "/pose";
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

        protected async override void SendMessage()
        {
            var toRad = orientation.eulerAngles * Mathf.Deg2Rad;
            var toEnu = position.Unity2Map();
            await _streamWriter.WriteAsync(new PoseStreamingRequest
            {
                Address = address,
                Data = new NavigationStatus
                {
                    Header = new Header
                    {
                        FrameId = frameId,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000.0
                    },
                    Position = new NED
                    {
                        North = toEnu.y,
                        East = toEnu.x,
                        Depth = - toEnu.z + verticalOffset
                    },
                    Orientation = toRad.Unity2Map().AsMsg(),
                    SeafloorVelocity = linearVelocity.Unity2Body().AsMsg(),
                    BodyVelocity = linearVelocity.Unity2Body().AsMsg(),
                    OrientationRate = angularVelocity.Unity2Body().AsMsg()
                }
            });
            hasData = false;
        }
    }
}
