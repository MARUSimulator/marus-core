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
using Acoustictransmission;
using Marus.Networking;
using static Acoustictransmission.AcousticTransmission;
using Marus.Core;
using Labust;

namespace Marus.Communications.Acoustics
{

    /// <summary>
    /// Receives commands and handles responses
    /// </summary>
    [RequireComponent(typeof(Nanomodem))]
    public class NanomodemROS : MonoBehaviour
    {

        Nanomodem nanomodem;
        AcousticTransmissionClient client;
        ServerStreamer<AcousticRequest> streamer;
        [SerializeField] private string payloadAddress;
        [SerializeField] private string rangeAddress;
        [SerializeField] private string requestAddress;

        void Awake()
        {
            nanomodem = GetComponent<Nanomodem>();
            streamer = new ServerStreamer<AcousticRequest>(TransmitCommand);
            
            if (string.IsNullOrEmpty(payloadAddress))
                payloadAddress = $"nanomodem{nanomodem.DeviceId}/nanomodem_payload";
            if (string.IsNullOrEmpty(rangeAddress))
                rangeAddress = $"nanomodem{nanomodem.DeviceId}/nanomodem_range";
            if (string.IsNullOrEmpty(requestAddress))
                requestAddress = $"nanomodem{nanomodem.DeviceId}/nanomodem_request";

            var client = RosConnection.Instance.GetClient<AcousticTransmissionClient>();
            // Server to Unity stream
            streamer.StartStream(client.StreamAcousticRequests(
                new CommandRequest { Address = requestAddress},
                cancellationToken: RosConnection.Instance.CancellationToken));
            
        }

        void Update()
        {
            streamer.HandleNewMessages();
        }

        /// <summary>
        /// This method is called on every NanomodemRequest from ROS side
        /// </summary>
        void TransmitCommand(AcousticRequest request)
        {
            ParseAndExecuteCommand(request.Request);
        }


        /// <summary>
        /// Parses reqest from ROS side, communicates with other nanomodems if needed
        /// and sends back response/s to ros side if needed.
        /// </summary>
        public void ParseAndExecuteCommand(NanomodemRequest req)
        {
            var acousticPayload = new AcousticResponse();
            var payload = GetEmptyNanomodemPayload();
            string message = req.Msg;

            int targetId = (int)req.Id;
            Debug.Log($"ReqType: {req.ReqType} TargetId: {targetId} Message: {message}");

            var msg = new NanomodemMessage();

            // Ping (range)
            if (req.ReqType == NanomodemRequest.Types.Type.Pingid)
            {
                msg.Message = $"$P{targetId:D3}";
                msg.ReceiverId = targetId;

                //Immediately send $Pxxx to acknowledge command
                var confirmPayload = GetEmptyNanomodemPayload();
                confirmPayload.MsgType = NanomodemPayload.Types.Type.Unicst;
                confirmPayload.Msg = $"$P{targetId:D3}";
                SendResponse(new AcousticResponse()
                {
                    Payload = confirmPayload,
                    Address = payloadAddress
                });

                // if transmition is confirmed (in range)
                Action<NanomodemMessage> onSuccess = (msg) => {
                    // send NanomodemRange to ROS
                    var rangeMsg = new NanomodemRange();
                    rangeMsg.Header = new Std.Header()
                    {
                        FrameId = "",
                        Timestamp = TimeHandler.Instance.TimeDouble
                    };
                    float range = nanomodem.RangeTo(targetId);
                    int rangeTransformed = nanomodem.GetRangeTransformed(targetId);
                    rangeMsg.Range = rangeTransformed;
                    rangeMsg.RangeM = range;
                    rangeMsg.Id = (int) nanomodem.DeviceId;

                    SendResponse(new AcousticResponse()
                    {
                        Address = rangeAddress,
                        Range = rangeMsg
                    });

                    // send classic #RxxxTyyyyy msg
                    payload.MsgType = NanomodemPayload.Types.Type.Unicst;
                    payload.Msg = String.Format("#R{0}T{1}", targetId.ToString("D3"), rangeTransformed.ToString("D5"));
                    payload.SenderId = (int) nanomodem.DeviceId;

                    acousticPayload.Address = payloadAddress;
                    acousticPayload.Payload = payload;
                    SendResponse(acousticPayload);
                };

                Action<NanomodemMessage> onFail = (msg) =>
                {
                    // send timeout msg if out of range
                    payload.MsgType = NanomodemPayload.Types.Type.Unicst;
                    payload.Msg = "#TO";
                    payload.SenderId = (int) nanomodem.DeviceId;

                    acousticPayload.Address = payloadAddress;
                    acousticPayload.Payload = payload;
                    SendResponse(acousticPayload);
                };

                nanomodem.Send(msg, onSuccess, onFail);
            }

            // Change nanomodem id
            else if(req.ReqType == NanomodemRequest.Types.Type.Chngid)
            {
                int newId = (int) req.Id;
                nanomodem.ChangeId(newId);

                payload.MsgType = NanomodemPayload.Types.Type.Unicst;
                payload.Msg = $"#A{newId:D3}";
                payload.SenderId = (int) newId;

                acousticPayload.Address = payloadAddress;
                acousticPayload.Payload = payload;
                SendResponse(acousticPayload);

                // re-initiate communication with ROS server with new id
                streamer.StopStream();
                Awake();
            }

            // Broadcast message
            else if(req.ReqType == NanomodemRequest.Types.Type.Brdcst)
            {
                msg.Message = $"$B{message.Length:D2}{message}";
                msg.TransmitionType = TransmitionType.Broadcast;
                nanomodem.Send(msg);

                payload.MsgType = NanomodemPayload.Types.Type.Brdcst;
                payload.Msg = msg.Message.Substring(0, 4);// $Bnn
                payload.SenderId = nanomodem.DeviceId;

                acousticPayload.Address = payloadAddress;
                acousticPayload.Payload = payload;
                SendResponse(acousticPayload);
            }

            // Unicast message
            else if(req.ReqType == NanomodemRequest.Types.Type.Unicst)
            {
                msg.Message = $"$U{targetId:D3}{message.Length:D2}{message}";
                msg.ReceiverId = targetId;

                nanomodem.Send(msg);

                payload.MsgType = NanomodemPayload.Types.Type.Unicst;
                payload.Msg = msg.Message.Substring(0, 7); // $Uxxxnn
                payload.SenderId = (int) nanomodem.DeviceId;
                acousticPayload.Address = payloadAddress;
                acousticPayload.Payload = payload;

                SendResponse(acousticPayload);
            }

            // Test message
            else if(req.ReqType == NanomodemRequest.Types.Type.Testmsg)
            {
                msg.Message = $"$B64Hello! This is a Nanomodem v3 DSSS test transmission at 640 bps.";
                msg.TransmitionType = TransmitionType.Broadcast;
                nanomodem.Send(msg);

                payload.MsgType = NanomodemPayload.Types.Type.Unicst;
                payload.Msg = $"$T{nanomodem.DeviceId:D3}";
                payload.SenderId = (int) nanomodem.DeviceId;

                acousticPayload.Address = payloadAddress;
                acousticPayload.Payload = payload;
                SendResponse(acousticPayload);
            }

            // Query status
            else if(req.ReqType == NanomodemRequest.Types.Type.Status)
            {
                payload.MsgType = NanomodemPayload.Types.Type.Unicst;
                payload.Msg = String.Format("#A{0:D3}V{1:D5}", nanomodem.DeviceId, nanomodem.GetConvertedVoltage());

                payload.SenderId = (int) nanomodem.DeviceId;

                acousticPayload.Address = payloadAddress;
                acousticPayload.Payload = payload;
                SendResponse(acousticPayload);
            }

            // Get supply voltage from other modem
            else if(req.ReqType == NanomodemRequest.Types.Type.Voltid)
            {
                msg.Message = $"$V{targetId:D3}";
                msg.TransmitionType = TransmitionType.Unicast;
                msg.ReceiverId = targetId;

                // ack response message
                payload.MsgType = NanomodemPayload.Types.Type.Unicst;
                payload.Msg = $"$V{targetId:D3}";

                payload.SenderId = (int) nanomodem.DeviceId;

                acousticPayload.Address = payloadAddress;
                acousticPayload.Payload = payload;
                SendResponse(acousticPayload);

                Action<NanomodemMessage> onSucess = (msg) =>
                {
                    var sender = AcousticMediumHelper.GetAcousticDeviceById<Nanomodem>(msg.SenderId);
                    if (sender == null) return;

                    int voltage = sender.GetConvertedVoltage();
                    NanomodemPayload payload2 = GetEmptyNanomodemPayload();
                    payload2.MsgType = NanomodemPayload.Types.Type.Unicst;
                    payload2.Msg = String.Format("#B{0:D3}06V{1:D5}", sender.DeviceId, voltage);
                    payload2.SenderId = (int) nanomodem.DeviceId;

                    SendResponse(new AcousticResponse()
                    {
                        Address = payloadAddress,
                        Payload = payload2
                    });
                };

                nanomodem.Send(msg, onSucess);
            }

            // Echo message
            else if(req.ReqType == NanomodemRequest.Types.Type.Echomsg)
            {
                msg.Message = $"$E{targetId:D3}{message.Length:D2}{msg}";
                msg.ReceiverId = targetId;
                nanomodem.Send(msg);

                payload.MsgType = NanomodemPayload.Types.Type.Unicst;
                payload.Msg = msg.Message.Substring(0, 7); // $Exxxnn
                payload.SenderId = (int) nanomodem.DeviceId;

                acousticPayload.Address = payloadAddress;
                acousticPayload.Payload = payload;
                SendResponse(acousticPayload);
            }

            // quality check
            else if(req.ReqType == NanomodemRequest.Types.Type.Quality)
            {
                payload.MsgType = NanomodemPayload.Types.Type.Unicst;
                payload.Msg = "$C0";
                payload.SenderId = (int) nanomodem.DeviceId;

                acousticPayload.Address = payloadAddress;
                acousticPayload.Payload = payload;
                SendResponse(acousticPayload);
            }

            // unicast and acknowledge
            else if(req.ReqType == NanomodemRequest.Types.Type.Unistack)
            {
                msg.Message = message;
                msg.ReceiverId = targetId;

                payload.MsgType = NanomodemPayload.Types.Type.Unicst;
                payload.Msg = message.Substring(0, 7); // $Mxxxnn
                payload.SenderId = (int) nanomodem.DeviceId;

                acousticPayload.Address = payloadAddress;
                acousticPayload.Payload = payload;
                SendResponse(acousticPayload);


                Action<NanomodemMessage> onSuccess = (msg) =>
                {
                    // send #RxxxTyyyyy msg
                    var responsePayload = GetEmptyNanomodemPayload();
                    responsePayload.MsgType = NanomodemPayload.Types.Type.Unicst;
                    float range = nanomodem.RangeTo(targetId);
                    int rangeTransformed = nanomodem.GetRangeTransformed(targetId);
                    responsePayload.Msg = String.Format("#R{0:3D}T{1:5D}", targetId, rangeTransformed);
                    responsePayload.SenderId = (int) nanomodem.DeviceId;

                    acousticPayload.Address = payloadAddress;
                    acousticPayload.Payload = responsePayload;
                    SendResponse(acousticPayload);
                };

                nanomodem.Send(msg);
            }

        }

        void SendResponse(AcousticResponse payload)
        {
            client.ReturnAcousticPayload(
                payload,
                cancellationToken: RosConnection.Instance.CancellationToken);
        }

        private NanomodemPayload GetEmptyNanomodemPayload()
        {
            NanomodemPayload  payload = new NanomodemPayload();
            payload.Header = new Std.Header()
            {
                FrameId = "",
                Timestamp = TimeHandler.Instance.TimeDouble
            };

            return payload;
        }


        // /// <summary>
        // /// Handles messages from other nanomodems
        // /// </summary>
        // public void ExecuteNanomodemResponse(AcousticMessage request)
        // {
        //     string message = request.Message;
        //     NanomodemPayload payload = GetEmptyNanomodemPayload();
        //     AcousticPayload acousticPayload = new AcousticPayload();

        //     // Handle broadcast message
        //     if (message.StartsWith("$B")) {
        //         payload.Msg = request.Message.Substring(4);
        //         // return #Bxxxnnddd to acknowledge received message
        //         //payload.Msg = String.Format("#B{0:D3}{1}", ((Nanomodem) request.sender).Id, request.Message.Substring(2));
        //         payload.SenderId = (int) ((Nanomodem) request.sender).Id;
        //         payload.MsgType = NanomodemPayload.Types.Type.Brdcst;

        //         acousticPayload.Address = payloadAddress;
        //         acousticPayload.Payload = payload;
        //     }

        //     // handle unicast message
        //     else if (message.StartsWith("$U") || message.StartsWith("$M")) {
        //         payload.Msg = request.Message.Substring(7);
        //         // return #Unnddd to acknowledge received message
        //         //payload.Msg = String.Format("#U{0}", request.Message.Substring(5));
        //         payload.SenderId = (int) ((Nanomodem) request.sender).Id;
        //         payload.MsgType = NanomodemPayload.Types.Type.Unicst;

        //         acousticPayload.Address = payloadAddress;
        //         acousticPayload.Payload = payload;
        //     }
        //     else
        //     {
        //         return;
        //     }

        //     streamHandle = acousticsClient.ReturnAcousticPayload(
        //         acousticPayload,
        //         cancellationToken: RosConnection.Instance.cancellationToken);
        // }
    }
}
