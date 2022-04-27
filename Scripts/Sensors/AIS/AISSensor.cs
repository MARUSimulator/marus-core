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

using System.Collections;
using System.Collections.Generic;
using Marus.Networking;
using Marus.Sensors.Primitive;
using Sensorstreaming;
using UnityEngine;
using System;
using Unity;

namespace Marus.Sensors.AIS
{
    [RequireComponent(typeof(GnssSensor))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AisDevice))]
    public class AisSensor : SensorBase
    {
        public uint TrueHeading;
        public uint SOG;
        public uint COG;

        public GnssSensor geoSensor;
        private Rigidbody rb;
        private AisDevice aisDevice;
        private Vector3 lastPosition;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            geoSensor = GetComponent<GnssSensor>();
            aisDevice = GetComponent<AisDevice>();
        }

        protected override void SampleSensor()
        {
            SetCOG();
            SetSOG();
            SetTrueHeading();
            lastPosition = transform.position;
        }

        private void SetTrueHeading()
        {
            float myHeading = transform.eulerAngles.y;
            float northHeading = Input.compass.magneticHeading;

            float dif = myHeading - northHeading;
            if (dif < 0) dif += 360f;
            TrueHeading =  (uint) Mathf.Round(dif);
        }

        private void SetCOG()
        {
            Vector3 d = transform.position - lastPosition;
            Vector3 direction = new Vector3(d.x, 0, d.z);
            if (direction != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                float r = rotation.eulerAngles.y;
                if (r < 0)
                {
                    r += 360f;
                }
                COG = (uint) Mathf.Round(r*10);
            }
        }

        private void SetSOG()
        {
            // TODO see if we can remove rigidbody dependency
            float conversion = 1.94384f; // m/s to kn conversion constant
            float velocity = rb.velocity.magnitude * conversion;
            SOG = (uint) Mathf.Round(velocity * 10);
        }
    }
}
