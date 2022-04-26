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
using Auv;
using Marus.Networking;
using Sensorstreaming;
using UnityEngine;
using Marus.Core;
using Std;

namespace Marus.Sensors.Primitive
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
            StreamSensor(streamingClient?.StreamPoseSensor(cancellationToken:RosConnection.Instance.CancellationToken));
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
