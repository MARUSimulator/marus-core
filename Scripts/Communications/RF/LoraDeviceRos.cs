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

namespace Marus.Communications.Rf
{
    public class LoraDeviceRos : MonoBehaviour
    {
        
        public bool IsTransmiter = true;
        public bool IsReceiver = true;

        LoraDevice _loraDevice;
        ServerStreamer<AcousticRequest> _transmitRequestStream;
        ServerStreamer<AcousticRequest> _responseRequestStream;

        // Start is called before the first frame update
        void Awake()
        {
            _loraDevice = GetComponent<LoraDevice>();
            _loraDevice.OnReceiveEvent += OnMsgReceive;
            _transmitRequestStream = new ServerStreamer<AcousticRequest>(OnTransmitRequest);

            // GET GRPC STREAMING CLIENT AND OPEN RESPONSE STREAM
        }

        private void Update() {
            _transmitRequestStream.HandleNewMessages();
        }

        private void OnMsgReceive(LoraMessage obj)
        {
            // WRITE RESPONSE TO GRPC STREAM
        }

        private void OnTransmitRequest(AcousticRequest request)
        {
            // PARSE GRPC MESSAGE TO LORA MSG

            // _loraDevice.Send();//
        }
    }
}
