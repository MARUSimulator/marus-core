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
using UnityEngine;

namespace Labust.Sensors.Acoustics
{

    public class AcousticMedium : MediumBase
    {
        protected AcousticMedium() { }

        public string Name = "Acoustic medium";

        /// <summary>
        /// Speed of sound in medium (in meters per second)
        /// Default is average speed of sound in sea water.
        /// </summary>
        public float C = 1500f;

        /// <summary>
        /// Sends message to every registered nanomodem device.
        /// </summary>
        public void Broadcast<T>(T msg) where T : AcousticMessage
        {
            foreach (var device in AcousticMediumHelper.GetAcousticDevicesOnProtocol(msg.Protocol))
            {
                if (msg.SenderId != device.DeviceId)
                {
                    StartCoroutine(SendMsg(msg, device));
                }
            }
        }

        /// <summary>
        /// Emulates time needed for message to arrive and sends it to other nanomodem.
        ///
        /// AcousticDevice does not have to have the same message as generic. If it knows how to decode
        /// the protocoll, it will receive the message
        /// </summary>
        public bool Transmit<T>(T message, AcousticReceiver receiver) where T : AcousticMessage
        {
            if (message.Protocol == receiver.Protocol)
            {
                StartCoroutine(SendMsg(message, receiver));
                return true;
            }
            return false;
        }


        /// <summary>
        /// Delay message by message duration + time to target
        /// </summary>
        IEnumerator SendMsg(AcousticMessage msg, AcousticReceiver receiver)
        {
            // Emulate delay time based on speed of sound in medium and distance
            float delayTime = msg.CompositionDuration + 
                (Vector3.Distance(msg.PhysicalParams.SourceLocation, receiver.Location) / C);
            yield return new WaitForSeconds(delayTime);

            _Transmit(msg, receiver);
        }


        /// <summary>
        /// Transmit message to single other object.
        /// Message will only be sent if other object is in range of the sender device. 
        /// Method assumes distance and range are of the same unit and magnitude. Default is meters (m).
        /// </summary>
        /// <param name="msg">Message object to be sent.</param>
        /// <param name="receiver">Object which message is sent to.</param>
        /// <returns>True if transmission succeeded, false if not (not in range).</returns>
        bool _Transmit<T>(T msg, AcousticReceiver receiver) where T : AcousticMessage
        {
            // IMPLEMENT COMPLEX PHYSICS
            float distance = Vector3.Distance(msg.PhysicalParams.SourceLocation, receiver.Location);
            if (msg.PhysicalParams.MaxRange >= distance)
            {
                receiver.OnReceive(msg);
                return true;
            }
            return false;
        }

    }
}
