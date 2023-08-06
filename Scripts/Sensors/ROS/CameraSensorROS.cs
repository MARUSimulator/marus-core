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

using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Timers;

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
        public List<byte[]> imageList_1 = new List<byte[]>();
        public List<byte[]> imageList_2 = new List<byte[]>();
        public List<byte[]> imageList_3 = new List<byte[]>();
        public List<byte[]> imageList_4 = new List<byte[]>();
        public List<byte[]> imageList_5 = new List<byte[]>();
        public List<byte[]> imageList_6 = new List<byte[]>();
        public List<byte[]> imageList_7 = new List<byte[]>();
        static int currentIndex = 0;
        static Timer timer;
        
        // Path to your local dataset
        string path = "/home/mbat/dataset";

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

            // List 1 -> no larvae 
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_001.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_021.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_125.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_119.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_097.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_100.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_095.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_089.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_079.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_080.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_077.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_075.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_068.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_060.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_056.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_051.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_052.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_048.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_020.png"));
            imageList_1.Add(File.ReadAllBytes(path + "/other_planktons/single/image_031.png"));

            // List 2 -> single larvae 25%
            imageList_2.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_001.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_121.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_050.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_060.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_007.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_100.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_095.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_089.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_079.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_080.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_077.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_075.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_068.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_060.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_056.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_051.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_052.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_048.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_020.png"));
            imageList_2.Add(File.ReadAllBytes(path + "/other_planktons/single/image_031.png"));

            // List 3 -> single larvae 50%
            imageList_3.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_001.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_121.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_050.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_060.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_058.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_100.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_090.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_095.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_111.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_002.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/other_planktons/single/image_077.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/other_planktons/single/image_075.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/other_planktons/single/image_068.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/other_planktons/single/image_060.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/other_planktons/single/image_056.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/other_planktons/single/image_051.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/other_planktons/single/image_052.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/other_planktons/single/image_048.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/other_planktons/single/image_020.png"));
            imageList_3.Add(File.ReadAllBytes(path + "/other_planktons/single/image_031.png"));

            // List 4 -> single larvae 75%
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_001.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_121.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_050.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_060.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_058.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_100.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_090.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_095.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_111.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_002.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_065.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_070.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_020.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_045.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/oyster_larvae/single/image_030.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/other_planktons/single/image_051.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/other_planktons/single/image_052.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/other_planktons/single/image_048.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/other_planktons/single/image_020.png"));
            imageList_4.Add(File.ReadAllBytes(path + "/other_planktons/single/image_031.png"));

            // List 5 -> grouped larvae 25%
            imageList_5.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_001.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_002.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_003.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_004.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_005.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_100.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_095.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_089.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_079.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_080.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_077.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_075.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_068.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_060.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_056.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_051.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_052.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_048.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_020.png"));
            imageList_5.Add(File.ReadAllBytes(path + "/other_planktons/single/image_031.png"));

            // List 6 -> grouped larvae 50%
            imageList_6.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_001.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_002.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_003.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_004.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_005.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_006.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_007.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_008.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_009.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_010.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/other_planktons/single/image_077.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/other_planktons/single/image_075.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/other_planktons/single/image_068.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/other_planktons/single/image_060.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/other_planktons/single/image_056.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/other_planktons/single/image_051.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/other_planktons/single/image_052.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/other_planktons/single/image_048.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/other_planktons/single/image_020.png"));
            imageList_6.Add(File.ReadAllBytes(path + "/other_planktons/single/image_031.png"));

            // List 7 -> grouped larvae 75%
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_001.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_002.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_003.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_004.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_005.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_006.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_007.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_008.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_009.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_010.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_011.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_012.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_013.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_014.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/oyster_larvae/grouped/image_018.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/other_planktons/single/image_051.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/other_planktons/single/image_052.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/other_planktons/single/image_048.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/other_planktons/single/image_020.png"));
            imageList_7.Add(File.ReadAllBytes(path + "/other_planktons/single/image_031.png"));

            // Create and configure the timer
            timer = new Timer();
            timer.Interval = 1000; // 1 second
            timer.Elapsed += TimerElapsed;
            timer.Start();

            // Bounding box 1
            if((transform.position.x >= 4310 && transform.position.x <= 4800) && (transform.position.z >= 5320 && transform.position.z <= 5700))
            {
                if(- transform.position.y >= 5 && - transform.position.y < 10){
                    
                    byteArray = imageList_2[currentIndex];
                } else if(- transform.position.y >= 10 && - transform.position.y < 20){
                    
                    byteArray = imageList_4[currentIndex];
                } else if(- transform.position.y >= 20 && - transform.position.y < 30){
                    
                    byteArray = imageList_3[currentIndex];
                } else {
                    
                    byteArray = imageList_1[0];
                }
            // Bounding box 2
            } else if((transform.position.x >= 6600 && transform.position.x <= 6900) && (transform.position.z >= 2400 && transform.position.z <= 3000))
            {
                if(- transform.position.y >= 5 && - transform.position.y < 10){
                    
                    byteArray = imageList_5[currentIndex];
                } else if(- transform.position.y >= 10 && - transform.position.y < 20){
                    
                    byteArray = imageList_7[currentIndex];
                } else if(- transform.position.y >= 20 && - transform.position.y < 30){
                    
                    byteArray = imageList_6[currentIndex];
                } else {
                    
                    byteArray = imageList_1[0];
                }
            } else {
                
                byteArray = imageList_1[1];
            }

            imgFromFile.LoadImage(byteArray);

            // PNG only
            byteArray = GetAs24(imgFromFile);

            return new CameraStreamingRequest
            {
                Image = new Sensor.Image
                {
                    Header = new Std.Header
                    {
                        Timestamp = Time.time,
                        FrameId = sensor.frameId
                    },
                        
                    Data = ByteString.CopyFrom(byteArray), 
                    Height = (uint)(imgFromFile.height), 
                    Width = (uint)(imgFromFile.width)
                },
                Address = address,
            };

            timer.Stop();
            timer.Dispose();
            
        }
        // Transform texture into RGB24 format
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
        public void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Print the current image byte array
            if (currentIndex < imageList_1.Count)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
                timer.Stop();
            }
        }
    }
}
