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
using static Sensorstreaming.SensorStreaming;

namespace Marus.Sensors
{
    /// <summary>
    /// Camera sensor implementation
    /// </summary>
    [RequireComponent(typeof(CameraSensorFix))]
    public class CameraSensorROSFix : SensorStreamer<SensorStreamingClient, CameraStreamingRequest>
    {
        CameraSensorFix sensor;
        void Start()
        {
            sensor = GetComponent<CameraSensorFix>();
            if (string.IsNullOrEmpty(address))
                address = sensor.vehicle.name + "/camera";
            StreamSensor(sensor,
                streamingClient.StreamCameraSensor);
        }

        protected async override void SendMessage()
        {
            try
            {
                await _streamWriter.WriteAsync(new CameraStreamingRequest
                {
                    Data = ByteString.CopyFrom(sensor.Data),
                    TimeStamp = Time.time,
                    Address = address,
                    Height = (uint)(sensor.ImageHeight),
                    Width = (uint)(sensor.ImageWidth)
                });
            }
            catch (Exception e)
            {
                Debug.Log("Possible message overflow.");
                Debug.LogError(e);
            }
        }

    }
}