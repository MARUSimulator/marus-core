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
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity;
using Marus.Utils;
using Marus.Logger;

namespace Marus.Communications.Rf
{
    /// <summary>
    /// Implements lora device capabilities
    /// </summary>
    public class LoraDevice : RfDevice<LoraMessage>
    {
        /// <summary>
        /// Unique identifier of lora
        /// 0-255
        /// </summary>
        [SerializeField] int Id;

        public event Action<LoraDevice, LoraMessage> OnReceiveEvent;


        public override int DeviceId => Id;
        public override string Protocol => "lora_protocol";

        // NanomodemRosController rosController;

        public void ChangeId(int id)
        {
            Id = id;
        }
        protected override void OnReceive(LoraMessage msg)
        {
            OnReceiveEvent?.Invoke(this, msg);
            Debug.Log($"Lora id: {Id}\nReceived message from lora id: {msg.SenderId}:\n{msg.Message}");
        }

        public override RfTransmitterParams GetTransmiterParams()
            => new RfTransmitterParams
            {
                SourceLocation = transform.position,
                MaxRange = Range
            };

        protected override void Send(LoraMessage msg,
                Action<LoraMessage> onAcknowledgeCallback=null,
                Action<LoraMessage> onTimeoutCallback=null)
        {
            msg.SenderId = DeviceId;
            msg.Protocol = this.Protocol;
            msg.TransmiterParams = GetTransmiterParams();

            if (msg.TransmitionType == TransmitionType.Unicast)
            {
                RfMediumHelper.Transmit(msg,
                    RfMediumHelper.GetRfDeviceById(msg.ReceiverId));
            }
            else if (msg.TransmitionType == TransmitionType.Broadcast)
            {
                msg.ReceiverId = -1; // receiver must not be set
                RfMediumHelper.Broadcast(msg);
            }
            // Debug.Log($"Nanomodem id: {Id}\nReceived message from nanomodem id: {((Nanomodem) msg.sender).Id}:\n{msg.Message}");
            // rosController.ExecuteNanomodemResponse(msg);
        }

    }
}
