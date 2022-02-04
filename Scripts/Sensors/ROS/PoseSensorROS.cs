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
    [RequireComponent(typeof(PoseSensor))]
    public class PoseSensorROS : SensorStreamer<PoseStreamingRequest>
    {
        PoseSensor sensor;

        void Start()
        {
            sensor = GetComponent<PoseSensor>();
            if (string.IsNullOrEmpty(address))
                address = sensor.vehicle.name + "/pose";
            StreamSensor(streamingClient?.StreamPoseSensor(cancellationToken:RosConnection.Instance.cancellationToken));
        }

        new void Update()
        {
            hasData = sensor.hasData;
            base.Update();
        }

        protected async override void SendMessage()
        {
            var toRad = sensor.orientation.eulerAngles * Mathf.Deg2Rad;
            var toEnu = sensor.position.Unity2Map();
            await _streamWriter.WriteAsync(new PoseStreamingRequest
            {
                Address = address,
                Data = new NavigationStatus
                {
                    Header = new Header
                    {
                        FrameId = sensor.frameId,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000.0
                    },
                    Position = new NED
                    {
                        North = toEnu.y,
                        East = toEnu.x,
                        Depth = - toEnu.z
                    },
                    Orientation = toRad.Unity2Map().AsMsg()
                }
            });
            hasData = false;
        }
    }
}
