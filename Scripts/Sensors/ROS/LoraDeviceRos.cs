using System;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity;
using Marus.Utils;
using Marus.Logger;
using Marus.Networking;
using Acoustictransmission;
using Rfcommunication;
using static Rfcommunication.LoraTransmission;
using Marus.Core;
using Google.Protobuf;

namespace Marus.Communications.Rf
{
    [RequireComponent(typeof(LoraDevice))]
    public class LoraDeviceRos : MonoBehaviour
    {

        public bool IsTransmiter = true;
        public bool IsReceiver = true;

        public string Address;

        LoraDevice _loraDevice;
        ServerStreamer<LoraMsg> _transmitRequestStream;
        LoraTransmissionClient _receiveClient;

        // Start is called before the first frame update
        void Awake()
        {
            _loraDevice = GetComponent<LoraDevice>();
            if (string.IsNullOrEmpty(Address))
            {
                Address = $"LoraDevice_{_loraDevice.DeviceId}";
            }
            if (IsReceiver)
            {
                _loraDevice.OnReceiveEvent += OnMsgReceive;
                _receiveClient = RosConnection.Instance.GetClient<LoraTransmissionClient>();
            }
            if (IsTransmiter)
            {
                _transmitRequestStream = new ServerStreamer<LoraMsg>(OnTransmitRequest);
                _transmitRequestStream.StartStream(
                    _receiveClient.ReceiveLoraMessages(
                        new ReceiveStreamRequest { Address = $"{Address}_transmit"}
                    )
                );
            }

            // GET GRPC STREAMING CLIENT AND OPEN RESPONSE STREAM
        }

        private void Update() {
            _transmitRequestStream.HandleNewMessages();
        }

        private void OnMsgReceive(LoraDevice device, LoraMessage obj)
        {
            _receiveClient.SendLoraMessage(
                new LoraMsg
                {
                    Address = Address + "_receive",
                    SourceId = (uint)obj.SenderId,
                    TargetId = (uint)obj.ReceiverId,
                    Header = new Std.Header { Timestamp = TimeHandler.Instance.TimeDouble },
                    Msg = ByteString.CopyFrom(obj.Message, Encoding.ASCII)
                }
            );
        }

        private void OnTransmitRequest(LoraMsg request)
        {
            var sender = RfMediumHelper.GetRfDeviceById((int)request.SourceId);
            var receiver = RfMediumHelper.GetRfDeviceById((int)request.TargetId);
            Debug.Log(request.Msg.ToString());
            if (sender != null && receiver != null)
            {
                sender.Send(
                    new LoraMessage
                    {
                        ReceiverId = receiver.DeviceId,
                        TransmitionType = TransmitionType.Unicast,
                        Message = request.Msg.ToString(Encoding.ASCII),
                    }
                );
            }
            else
            {
                var template = "Cannot transfer message. {} does not exist.";
                Debug.Log($@"{(sender == null ? string.Format(template, "Sender") : "")}
                        {(receiver == null ? string.Format(template, "Receiver") : "")}");
            }
        }
    }
}
