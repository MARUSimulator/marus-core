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
using Sensorstreaming;
using Google.Protobuf;
using Grpc.Core;
using System;
using Marus.Networking;
using Marus.Logger;
using Marus.Utils;
using Marus.ROS;
using System.Threading.Tasks;

namespace Marus.Sensors
{
    public abstract class SensorBase : MonoBehaviour
    {
        protected void Awake()
        {
            SensorSampler.Instance.AddSensorCallback(this, SampleSensor);
        }

        //public bool RunRecording = false;

        [Header("Sensor parameters")]
        public String frameId;

        public float SampleFrequency = 20;

        protected Transform _vehicle;
        [NonSerialized] public bool hasData;

        public Transform vehicle
        {
            get
            {
                _vehicle = Helpers.GetVehicle(transform);
                if (_vehicle == null)
                {
                    Debug.Log($@"Cannot get vehicle from sensor {transform.name}.
                        Using sensor as the vehicle transform");
                    return transform;
                }
                return _vehicle;
            }
        }

        #if UNITY_EDITOR
        protected void Reset()
        {
            UpdateVehicle();
        }
        #endif

        public void UpdateVehicle()
        {
            var veh = vehicle;
            // reset frameId to empty if UpdateVehicle is not called from reset
            frameId = "";
            // if not same object, add vehicle name prefix to frame id
            if(veh != transform) frameId = $"{veh.name}/";

            frameId = frameId + gameObject.name + "_frame";
        }

        protected abstract void SampleSensor();

        protected GameObjectLogger Logger;
        protected void Log<W>(W data)
        {
            if (Logger == null)
            {
                Logger = DataLogger.Instance.GetLogger<W>($"{vehicle.name}/{name}");
            }
            (Logger as GameObjectLogger<W>).Log(data);
        }

        void OnEnable()
        {
            SensorSampler.Instance.EnableCallback(this);
        }

        void OnDisable()
        {
            SensorSampler.Instance.DisableCallback(this);
        }

    }
}
