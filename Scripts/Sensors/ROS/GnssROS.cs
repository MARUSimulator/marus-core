using System;
using System.Collections;
using System.Collections.Generic;
using Labust.Core;
using Labust.Networking;
using Labust.Sensors.Primitive;
using Sensor;
using Sensorstreaming;
using Std;
using UnityEngine;

namespace Labust.Sensors.ROS
{
    [RequireComponent(typeof(GnssSensor))]
    public class GnssROS : SensorStreamer<GnssStreamingRequest>
    {
        GnssSensor sensor;
        void Start()
        {
            var sensor = GetComponent<GnssSensor>();
            if (string.IsNullOrEmpty(address))
            {
                address = $"{sensor.vehicle?.name}/gps";
            }
            StreamSensor(streamingClient?.StreamGnssSensor(cancellationToken:RosConnection.Instance.cancellationToken));
        }

        void Update()
        {
            hasData = sensor.hasData;
            base.Update();
        }

        protected override async void SendMessage()
        {
            var msg = new GnssStreamingRequest
            {
                Address = address,
                Data = new NavSatFix
                {
                    Header = new Header
                    {
                        FrameId = sensor.frameId,
                        Timestamp = TimeHandler.Instance.TimeDouble
                    },
                    Status = new NavSatStatus
                    {
                        Service = NavSatStatus.Types.Service.Gps,
                        Status = sensor.isRTK ? NavSatStatus.Types.Status.GbasFix : NavSatStatus.Types.Status.SbasFix
                    },
                    Latitude = sensor.point.latitude,
                    Longitude = sensor.point.longitude,
                    Altitude = sensor.point.altitude
                }
            };
            if (transform.position.y > -sensor.maximumOperatingDepth) 
                await _streamWriter.WriteAsync(msg);
            hasData = false;
        }
    }
}