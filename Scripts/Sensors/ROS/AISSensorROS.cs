// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Marus.Networking;
using Sensorstreaming;
using UnityEngine;
using System;
using static Sensorstreaming.SensorStreaming;

namespace Marus.Sensors.AIS
{
    [RequireComponent(typeof(AisSensor))]
    [RequireComponent(typeof(AisDevice))]
    public class AisSensorROS : SensorStreamer<SensorStreamingClient, AISStreamingRequest>
    {
        AisSensor sensor;
        AisDevice device;
        void Start()
        {
            sensor = GetComponent<AisSensor>();
            device = GetComponent<AisDevice>();
            if (string.IsNullOrEmpty(address))
                address = transform.name + "/ais";
            StreamSensor(sensor,
                streamingClient.StreamAisSensor);
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
        }
    }
}
