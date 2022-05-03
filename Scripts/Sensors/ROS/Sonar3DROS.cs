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

using System;
using Marus.Core;
using Marus.Networking;
using Sensorstreaming;
using Google.Protobuf;
using UnityEngine;
using static Sensorstreaming.SensorStreaming;

namespace Marus.Sensors.Primitive
{
    [RequireComponent(typeof(Sonar3D))]
    public class Sonar3DROS : SensorStreamer<SensorStreamingClient, CompressedImageStreamingRequest>
    {
        Sonar3D sensor;
        new void Start()
        {
            sensor = GetComponent<Sonar3D>();
            StreamSensor(sensor,
                streamingClient.StreamSonarImage);
            base.Start();
        }

        protected override CompressedImageStreamingRequest ComposeMessage()
        {
            return new CompressedImageStreamingRequest
            {
                Data = new Sensor.CompressedImage
                {
                    Header = new Std.Header
                    {
                        FrameId = sensor.frameId,
                        Timestamp = TimeHandler.Instance.TimeDouble
                    },
                    Data = ByteString.CopyFrom((byte []) sensor.sonarCartesianImage.EncodeToPNG()),
                    Format = "png"
                },
                Address = address
            };
        }
    }
}
