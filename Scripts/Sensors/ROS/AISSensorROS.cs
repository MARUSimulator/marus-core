using System.Collections;
using System.Collections.Generic;
using Labust.Networking;
using Labust.Sensors.Primitive;
using Sensorstreaming;
using UnityEngine;
using System;
using Unity;

namespace Labust.Sensors.AIS
{
    [RequireComponent(typeof(AisSensor))]
    [RequireComponent(typeof(AisDevice))]
    public class AisSensorROS : SensorStreamer<AISStreamingRequest>
    {
        
        AisSensor sensor;
        AisDevice device;
        void Start()
        {
            sensor = GetComponent<AisSensor>();
            device = GetComponent<AisDevice>();
            if (string.IsNullOrEmpty(address))
                address = transform.name + "/ais";
            StreamSensor(streamingClient?.StreamAisSensor(cancellationToken:RosConnection.Instance.cancellationToken));
            UpdateFrequency = 1 / TimeIntervals.getInterval(device.ClassType, sensor.SOG);
        }

        protected async override void SendMessage()
        {
            var msg = new AISStreamingRequest
            {
                Address = address,
                AisPositionReport = new Marine.AISPositionReport 
                {
                    Type = (uint) AISMessageType.PositionReportClassA,
                    Mmsi = (uint) Int32.Parse(device.MMSI),
                    Heading = (float) sensor.TrueHeading,
                    Timestamp = (uint) System.DateTime.UtcNow.Second,
                    Geopoint = new Geographic.GeoPoint {
                        Latitude = sensor.geoSensor.point.latitude,
                        Longitude = sensor.geoSensor.point.longitude,
                        Altitude = 0
                    },
                    SpeedOverGround = sensor.SOG,
                    CourseOverGround = sensor.COG
                }
            };
            await _streamWriter.WriteAsync(msg);
            hasData = false;
        }
    }
}
