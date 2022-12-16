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

using Marus.Core;
using Marus.Networking;
using Marus.Sensors.Primitive;
using Sensor;
using Sensorstreaming;
using Std;
using UnityEngine;
using static Sensorstreaming.SensorStreaming;

namespace Marus.Sensors.ROS
{
    [RequireComponent(typeof(GnssSensor))]
    public class GnssROS : SensorStreamer<SensorStreamingClient, GnssStreamingRequest>
    {
        GnssSensor sensor;
        void Start()
        {
            sensor = GetComponent<GnssSensor>();
            if (string.IsNullOrEmpty(address))
            {
                address = $"{sensor.vehicle?.name}/gps";
            }
            StreamSensor(sensor, 
                streamingClient.StreamGnssSensor);
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
                        FrameId = sensor.FrameId,
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
        }
    }
}