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

using UnityEngine.UI;
using System.IO;

namespace Marus.Sensors
{
    /// <summary>
    /// Camera sensor implementation
    /// </summary>
    [RequireComponent(typeof(CameraSensor))]
    public class CameraSensorROS : SensorStreamer<SensorStreamingClient, CameraStreamingRequest>
    {
        Texture2D imgFromFile;
        public byte[] byteArray;

        CameraSensor sensor;
        new void Start()
        {
            imgFromFile = new Texture2D(2, 2, TextureFormat.RGB24, false);
            sensor = GetComponent<CameraSensor>();
            StreamSensor(sensor,
                streamingClient.StreamCameraSensor);
            base.Start();
        }

        protected override CameraStreamingRequest ComposeMessage()
        {
            // example for path to image inside Unity
            byteArray = File.ReadAllBytes("Assets/marus-core/Scripts/Hackathon/image_00.png"); 

            // example for path to image on local disk
            //byteArray = File.ReadAllBytes("/home/mbat/Pictures/image_00.png");   
            imgFromFile.LoadImage(byteArray);
            byteArray = GetAs24(imgFromFile);

            //sensor.Data = byteArray;
            return new CameraStreamingRequest
            {
                Image = new Sensor.Image
                {
                    Header = new Std.Header
                    {
                        Timestamp = Time.time,
                        FrameId = sensor.frameId
                    },
                    
                    Data = ByteString.CopyFrom(byteArray), // old = sensor.Data
                    Height = (uint)(imgFromFile.height), //sensor.ImageHeight
                    Width = (uint)(imgFromFile.width) // sensor.ImageWidth
                },
                Address = address,
            };
        }

        private byte[] GetAs24(Texture2D imgFromFile)
        {
            byte[] newArray = new byte[imgFromFile.width * imgFromFile.height * 3];
            var raw = imgFromFile.GetRawTextureData();
            var w = imgFromFile.width;
            var h = imgFromFile.height;
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    newArray[i*w*3 + j*3 + 0] = raw[i*w*4 + j*4 +1];
                    newArray[i*w*3 + j*3 + 1] = raw[i*w*4 + j*4 +2];
                    newArray[i*w*3 + j*3 + 2] = raw[i*w*4 + j*4 +3];
                }
            }
            
            return newArray;
        }
    }
}
