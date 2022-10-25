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

using UnityEngine;
using Google.Protobuf;
using Sensorstreaming;
using System;
using Marus.Networking;
using Marus.Core;
using static Sensorstreaming.SensorStreaming;

namespace Marus.Sensors
{
    /// <summary>
    /// Camera sensor implementation
    /// </summary>
    [RequireComponent(typeof(CameraSensor))]
    public class CameraSensorROS : SensorStreamer<SensorStreamingClient, CameraStreamingRequest>
    {
        CameraSensor sensor;
        new void Start()
        {
            base.Start();
            sensor = GetComponent<CameraSensor>();
            if (string.IsNullOrEmpty(address))
                address = sensor.vehicle.name + "/" + sensor.gameObject.name;
            StreamSensor(sensor,
                streamingClient.StreamCameraSensor);
        }

        protected override CameraStreamingRequest ComposeMessage()
        {
            return new CameraStreamingRequest
            {
                Image = new Sensor.Image
                {
                    Header = new Std.Header
                    {
                        Timestamp = Time.time,
                        FrameId = sensor.frameId
                    },
                    Data = ByteString.CopyFrom(sensor.Data),
                    Height = (uint)(sensor.ImageHeight),
                    Width = (uint)(sensor.ImageWidth)
                },
                Address = address,
            };
        }

    }
}