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
    public class LoraDevice : RfDevice<LoraMessage>
    {
        /// <summary>
        /// Unique identifier of nanomodem
        /// 0-255
        /// </summary>
        [SerializeField] int Id;

        public event Action<LoraMessage> OnReceiveEvent;


        public override int DeviceId => Id;
        public override string Protocol => "lora_protocol";

        // NanomodemRosController rosController;

        public void ChangeId(int id)
        {
            Id = id;   
        }
        public override void OnReceive(LoraMessage msg)
        {
            OnReceiveEvent?.Invoke(msg);
            Debug.Log($"Nanomodem id: {Id}\nReceived message from nanomodem id: {msg.SenderId}:\n{msg.Message}");
        }

        public override RfTransmiterParams GetTransmiterParams()
            => new RfTransmiterParams
            {
                SourceLocation = transform.position,
                MaxRange = -1
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
