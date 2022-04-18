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

using Geometry;
using Marus.Core;
using Marus.Networking;
using Marus.Sensors.Primitive;
using Sensorstreaming;
using Std;
using UnityEngine;

namespace Marus.Sensors.ROS
{

    [RequireComponent(typeof(DvlSensor))]
    public class DvlROS : SensorStreamer<DvlStreamingRequest>
    {
        DvlSensor sensor;
        void Start()
        {
            sensor = GetComponent<DvlSensor>();
            if (string.IsNullOrEmpty(address))
                address = sensor.vehicle.name + "/dvl";
            StreamSensor(streamingClient?.StreamDvlSensor(cancellationToken:RosConnection.Instance.CancellationToken));
        }

        new void Update()
        {
            hasData = sensor.hasData;
            base.Update();
        }

        protected async override void SendMessage()
        {
            var dvlOut = new TwistWithCovarianceStamped
            {
                Header = new Header()
                {
                    FrameId = sensor.frameId,
                    Timestamp = TimeHandler.Instance.TimeDouble
                },
                Twist = new TwistWithCovariance 
                {
                    Twist = new Twist
                    {
                        Linear = sensor.groundVelocity.Unity2Body().AsMsg()
                    }
                }
            };
            dvlOut.Twist.Covariance.AddRange(sensor.velocityCovariance);
            
            var request = new DvlStreamingRequest
            {
                Address = address,
                Data = dvlOut
            };
            await _streamWriter.WriteAsync(request);
            hasData = false;
        }
    }
}