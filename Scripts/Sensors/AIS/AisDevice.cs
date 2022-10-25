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
using Marus.Sensors.Primitive;
using System;

namespace Marus.Sensors.AIS
{
    /// <summary>
    ///  This class implements AIS capabilities.
    /// </summary>
    [RequireComponent(typeof(AisSensor))]
    public class AisDevice : MonoBehaviour
    {
        /// <summary>
        /// AIS class type: A or B
        /// More info <see cref="!:https://www.navcen.uscg.gov/?pageName=typesAIS">here.</see>
        /// </summary>
        public AISClassType ClassType = AISClassType.ClassA;

        /// <summary>
        /// Maritime Mobile Service Identity
        /// Unique 9 digit number assigned to radio or AIS unit.
        /// </summary>
        public string MMSI = "";

        /// <summary>
        /// Name of the vessel used in some message types, maximum 20 ASCII characters.
        /// Default as set is standard for undefined.
        /// </summary>
        public string Name = "@@@@@@@@@@@@@@@@@@@@";

        /// <summary>
        /// Set transimission on or off, receiving will be enabled regardless.
        /// </summary>
        public bool ActiveTransmission = true;
        public float Range;
        public event Action<AisMessage> OnReceiveEvent;

        private float period = 0;
        private float delta = 0;
        private PositionReportClassA message;
        private Vector3 lastPosition;
        private AisManager AISMedium;
        private Rigidbody rb;
        private GnssSensor geoSensor;
        private AisSensor aisSensor;


        public void Start()
        {
            if (string.IsNullOrEmpty(MMSI))
            {
                MMSI = MMSIGenerator.GenerateMMSI();
            }
            rb = GetComponent<Rigidbody>();
            SetRange();
            AISMedium = AisManager.Instance;
            AISMedium.Register(this);
            aisSensor = GetComponent<AisSensor>();
            geoSensor = GetComponent<GnssSensor>();
        }

        void Update()
        {
            if (ActiveTransmission)
            {
                period = TimeIntervals.GetInterval(ClassType, aisSensor.SOG);
                if (delta > period)
                {
                    message = new PositionReportClassA();
                    message.SOG = aisSensor.SOG;
                    message.COG = aisSensor.COG;
                    message.TrueHeading = aisSensor.TrueHeading;
                    message.Longitude = geoSensor.point.longitude;
                    message.Latitude = geoSensor.point.latitude;
                    message.TimeStamp = (uint) System.DateTime.UtcNow.Second;
                    message.sender = this;
                    AISMedium.Broadcast(message);
                    delta = 0;
                }
                delta += Time.deltaTime;
                lastPosition = transform.position;
            }
        }


        public void Receive(AisMessage msg)
        {
            OnReceiveEvent?.Invoke(msg);
        }

        private void SetRange()
        {
            if (ClassType == AISClassType.ClassA)
            {
                // 75km range for 12.5W transponder
                this.Range = 75f * 1000;
            }
            else
            {
                // 15km range for 2W transponder
                this.Range = 15f * 1000;
            }
        }
    }
}
