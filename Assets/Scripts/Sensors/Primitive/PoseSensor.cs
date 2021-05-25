using System.Collections;
using System.Collections.Generic;
using Labust.Networking;
using Sensorstreaming;
using UnityEngine;
using Utils;

namespace Labust.Sensors
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
            if (string.IsNullOrEmpty(sensorId))
                sensorId = vehicle.name + "/pose";
        }

        public void Refresh()
        {
            position = measuredObject.position;
            orientation = measuredObject.rotation;
            linearVelocity = measuredObject.transform.InverseTransformVector(measuredObject.velocity);
            angularVelocity = measuredObject.angularVelocity;
            hasData = true;
        }

        public async override void SendMessage()
        {
            await streamWriter.WriteAsync(new PoseStreamingRequest
            {
                SensorId = sensorId,
                Pose = new Common.Pose
                {
                    Position = new Common.Position
                    {
                        X = position.x,
                        Y = position.y,
                        Z = position.z
                    }
                }
            });
            hasData = false;
        }
    }
}
