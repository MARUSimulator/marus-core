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
using UnityEngine;

namespace Marus.Sensors.Acoustics
{
    public class Nanomodem : AcousticDevice<NanomodemMessage>
    {
        /// <summary>
        /// Unique identifier of nanomodem
        /// 0-255
        /// </summary>
        [SerializeField] int Id;

        public event Action<NanomodemMessage> OnReceiveEvent;

        /// <summary>
        /// Nanomodem's supply voltage in Volts (V) 3-6.5V
        /// </summary>
        [Tooltip("Supply voltage in volts (V)")]
        public double SupplyVoltage;

        [Tooltip("Ranging increment in cm")]
        public float RangingIncrement = 4.7f;

        [Tooltip("Ranging variance in cm")]
        public float RangingVariance = 6.0f;

        public override int DeviceId => Id;
        public override string Protocol => "nanomodem_protocol";

        // NanomodemRosController rosController;

        public void Awake()
        {
            // medium = (AcousticMedium) AcousticMedium.Instance;
            // // medium.Register(this);
            // rosController = gameObject.AddComponent(typeof(NanomodemRosController)) as NanomodemRosController;
            if (SupplyVoltage == 0)
            {
                SupplyVoltage = GetRandomVoltage();
            }
        }

        public void ChangeId(int id)
        {
            Id = id;   
        }
        public override void OnReceive(NanomodemMessage msg)
        {
            OnReceiveEvent?.Invoke(msg);
            Debug.Log($"Nanomodem id: {Id}\nReceived message from nanomodem id: {msg.SenderId}:\n{msg.Message}");
        }

        public override void Send(NanomodemMessage msg, 
                Action<NanomodemMessage> onAcknowledgeCallback=null,
                Action<NanomodemMessage> onTimeoutCallback=null)
        {
            msg.SenderId = DeviceId;
            msg.Protocol = this.Protocol;
            msg.PhysicalParams = GetPhysicalParams();
            msg.CompositionDuration = CompositionDuration(msg.Message);
            var medium = AcousticMediumHelper.GetAcousticMediumForTransform(transform);

            if (msg.TransmitionType == TransmitionType.Unicast)
            {
                medium.Transmit(msg, 
                    AcousticMediumHelper.GetAcousticDeviceById(msg.ReceiverId));
            }
            else if (msg.TransmitionType == TransmitionType.Broadcast)
            {
                msg.ReceiverId = -1; // receiver must not be set
                medium.Broadcast(msg);
            }
            // Debug.Log($"Nanomodem id: {Id}\nReceived message from nanomodem id: {((Nanomodem) msg.sender).Id}:\n{msg.Message}");
            // rosController.ExecuteNanomodemResponse(msg);
        }

        private PhysicalParams GetPhysicalParams()
        {
            return new PhysicalParams
            {
                SourceLocation = transform.position,
                MaxRange = Range
            };
        }

        /// <summary>
        /// Message generation duration which is dependent on message length
        /// header duration + duration of actual message
        /// </summary>
        float CompositionDuration(string message)
        {
            float byteDuration = 0.0125f;
            float headerDuration = 0.105f;
            if(message.StartsWith("$B"))
            {
                int numOfBytes = Int32.Parse(message.Substring(2, 2));
                return headerDuration + (numOfBytes + 16) * byteDuration;
            }
            else if(message.StartsWith("$E") || message.StartsWith("$U") || message.StartsWith("$M"))
            {
                int numOfBytes = Int32.Parse(message.Substring(5, 2));
                return headerDuration + (numOfBytes + 16) * byteDuration;
            }

            return headerDuration;
        }

        /// <summary>
        /// Returns range to nanomodem by id in meters.
        /// </summary>
        /// <param name="id">Id of nanomodem to ping.</param>
        /// <param name="addNoise">If true adds Gaussian noise to range measurement.</param>
        /// <returns></returns>
        public float RangeTo(int id, bool addNoise = true)
        {
            //if id is not found, it will return 0 distance
            Nanomodem target = this;
            // List<AcousticDevice<AcousticMessage>> nanomodems = medium.RegisteredDevices;
            // foreach (Nanomodem nanomodem in nanomodems)
            // {
            //     if (nanomodem.Id == id){
            //         target = nanomodem;
            //         break;
            //     }
            // }
            // var distance = medium.DistanceFromTo(this, target);
            // if (addNoise)
            // {
            //     distance += Helpers.NextGaussian(0.0f, RangingVariance / 100f);
            // }
            // distance = Mathf.Round(distance / (RangingIncrement / 100)) * (RangingIncrement / 100);
            // return distance;
            return 0;
        }


        /// </summary>
        /// Returns range to nanomodem by id recalculated for usage with nanomodem messages.
        /// R = yyyyy * c * 3.125e-05
        /// Returns yyyyy calculated from R (range in meters) and C (speed of sound in medium)
        /// </summary>
        public int GetRangeTransformed(int id)
        {
            float range = RangeTo(id);
            // return (int) Mathf.Round((float) (range / (medium.C * 3.125 * 0.00001)));
            return 0;
        }

        /// <summary>
        /// Returns random double in range 3 - 6.5 V
        /// This is used for setting initial supply voltage
        /// </summary>
        private double GetRandomVoltage()
        {
            // Supply voltage is in range 3-6.5V
            var minVoltage = 3.0;
            var maxVoltage = 6.5;
            return new System.Random().NextDouble() * (maxVoltage - minVoltage) + minVoltage;
        }

        /// <summary>
        /// Returns voltage converted for usage in nanomodem messages
        /// v = yyyyy * 15/65536
        /// Returns yyyyy calculated from v (SupplyVoltage)
        /// </summary>
        public int GetConvertedVoltage()
        {
            // Convert to integer
            return (int) Mathf.Round((float) (SupplyVoltage * 65536 / 15));
        }

    }
}
