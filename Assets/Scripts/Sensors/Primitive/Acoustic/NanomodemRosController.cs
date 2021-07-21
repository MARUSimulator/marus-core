using System;
using UnityEngine;
using Labust.Actuators;
using AcousticTransmission;
using Labust.Networking;
using System.Collections.Generic;

namespace Labust.Sensors.Primitive.Acoustic
{
	
	/// <summary>
	/// Receives commands and handles responses
	/// </summary>
	public class NanomodemRosController : AcousticRosControllerBase
	{

		private Nanomodem nanomodem;

		void Awake()
		{	
			nanomodem = GetComponent<Nanomodem>();
			mode = MessageHandleMode.Sequential;
			
			streamHandle = acousticsClient.GetAcousticRequest(
                new CommandRequest { Address = "nanomodem/" + nanomodem.Id },
                cancellationToken: RosConnection.Instance.cancellationToken);

            HandleResponse(TransmitCommand);
		}

		/// <summary>
		/// This method is called on every NanomodemRequest from ROS side
		/// </summary>
		void TransmitCommand(AcousticResponse request)
		{
			NanomodemRequest req = request.Request;			
			List<AcousticPayload> payloadList = ParseAndExecuteCommand(req);
			foreach(AcousticPayload payload in payloadList)
			{
				streamHandle = acousticsClient.ReturnAcousticPayload(
					payload,
					cancellationToken: RosConnection.Instance.cancellationToken);
			}
		}

		/// <summary>
		/// Parses reest from ROS side, communicates with other nanomodems if needed
		/// and sends back response/s to ros side if needed.
		/// </summary>
		public List<AcousticPayload> ParseAndExecuteCommand(NanomodemRequest req)
		{
			List<AcousticPayload> payloadList = new List<AcousticPayload>();
			string message = req.Msg;
			NanomodemPayload payload;
			AcousticPayload acousticPayload;
			
			// Ping (range)
			if (req.ReqType == NanomodemRequest.Types.Type.Pingid)
			{
				int targetId = (int) req.Id;
				float range = nanomodem.Range(targetId);
				int rangeTransformed = nanomodem.GetRangeTransformed(targetId);

				AcousticMessage msg = new AcousticMessage();
				msg.Message = message;
				msg.sender = nanomodem;
				bool sent = nanomodem.medium.Transmit(msg, nanomodem.medium.GetNanomodemById((uint) targetId));
				
				// if transmition is confirmed (in range)
				if (sent) {
					// send NanomodemRange to ROS
					NanomodemRange rangeMsg = new NanomodemRange();
					rangeMsg.Range = (uint) targetId;
					rangeMsg.RangeM = range;
					rangeMsg.Id = (uint) nanomodem.Id;

					AcousticPayload rangePayload = new AcousticPayload();
					rangePayload.Address = "nanomodem/" + nanomodem.Id;
					rangePayload.Range = rangeMsg;
					payloadList.Add(rangePayload);

					// send classic #RxxxTyyyyy msg
					payload = new NanomodemPayload();
					payload.MsgType = NanomodemPayload.Types.Type.Unicst;
					payload.Msg = String.Format("#R{0:3D}T{1:5D}", targetId, rangeTransformed);
					payload.SenderId = (uint) nanomodem.Id;

					acousticPayload = new AcousticPayload();
					acousticPayload.Address = "nanomodem/" + nanomodem.Id;
					acousticPayload.Payload = payload;
					payloadList.Add(acousticPayload);
				}

				// send timeout msg if out of range
				payload = new NanomodemPayload();
				payload.MsgType = NanomodemPayload.Types.Type.Unicst;
				payload.Msg = "#TO";
				payload.SenderId = (uint) nanomodem.Id;

				acousticPayload = new AcousticPayload();
				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
				payloadList.Add(acousticPayload);
			}
			
			// Change nanomodem id
			else if(req.ReqType == NanomodemRequest.Types.Type.Chngid)
			{
				int newId = (int) req.Id;
				nanomodem.Id = newId;
				payload = new NanomodemPayload();
				payload.MsgType = NanomodemPayload.Types.Type.Unicst;
				payload.Msg = "#A" + message.Substring(2, 3);
				payload.SenderId = (uint) newId;

				acousticPayload = new AcousticPayload();
				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
				payloadList.Add(acousticPayload);

				// re-initiate communication with ROS server with new id
				Awake();
			}
			
			// Broadcast message
			else if(req.ReqType == NanomodemRequest.Types.Type.Brdcst)
			{
				AcousticMessage msg = new AcousticMessage();
				msg.Message = message;
				msg.sender = nanomodem;
				nanomodem.medium.Broadcast(msg);

				payload = new NanomodemPayload();
				payload.MsgType = NanomodemPayload.Types.Type.Brdcst;
				payload.Msg = message.Substring(0, 4); // $Bnn
				payload.SenderId = (uint) nanomodem.Id;
				
				acousticPayload = new AcousticPayload();
				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
				payloadList.Add(acousticPayload);
			}
			
			// Unicast message
			else if(req.ReqType == NanomodemRequest.Types.Type.Unicst)
			{
				AcousticMessage msg = new AcousticMessage();
				msg.Message = message;
				msg.sender = nanomodem;
				Nanomodem targetModem = nanomodem.medium.GetNanomodemById(req.Id);
				nanomodem.medium.Transmit(msg, targetModem);

				payload = new NanomodemPayload();
				payload.MsgType = NanomodemPayload.Types.Type.Unicst;
				payload.Msg = message.Substring(0, 7); // $Uxxxnn
				payload.SenderId = (uint) nanomodem.Id;
				
				acousticPayload = new AcousticPayload();
				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
				payloadList.Add(acousticPayload);
			}
			
			// Test message
			else if(req.ReqType == NanomodemRequest.Types.Type.Testmsg)
			{
				AcousticMessage msg = new AcousticMessage();
				msg.Message = "Hello! This is a Nanomodem v3 DSSS test transmission at 640 bps.";
				msg.sender = nanomodem;
				nanomodem.medium.Broadcast(msg);

				payload = new NanomodemPayload();
				payload.MsgType = NanomodemPayload.Types.Type.Unicst;
				payload.Msg = message;
				payload.SenderId = (uint) nanomodem.Id;
				
				acousticPayload = new AcousticPayload();
				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
				payloadList.Add(acousticPayload);
			}
			
			// Query status
			else if(req.ReqType == NanomodemRequest.Types.Type.Status)
			{
				payload = new NanomodemPayload();
				payload.MsgType = NanomodemPayload.Types.Type.Unicst;
				payload.Msg = String.Format("#A{0:D3}V{1:D5}", nanomodem.Id, nanomodem.GetConvertedVoltage());
				
				payload.SenderId = (uint) nanomodem.Id;
				
				acousticPayload = new AcousticPayload();
				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
				payloadList.Add(acousticPayload);
			}
			
			// Get supply voltage from other modem
			else if(req.ReqType == NanomodemRequest.Types.Type.Voltid)
			{
				AcousticMessage msg = new AcousticMessage();
				msg.Message = req.Msg;
				msg.sender = nanomodem;
				Nanomodem targetModem = nanomodem.medium.GetNanomodemById(req.Id);
				nanomodem.medium.Transmit(msg, targetModem);

				payload = new NanomodemPayload();
				payload.MsgType = NanomodemPayload.Types.Type.Unicst;
				payload.Msg = req.Msg;
				
				payload.SenderId = (uint) nanomodem.Id;
				
				acousticPayload = new AcousticPayload();
				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
				payloadList.Add(acousticPayload);

				int voltage = targetModem.GetConvertedVoltage();
				NanomodemPayload payload2 = new NanomodemPayload();
				payload2.MsgType = NanomodemPayload.Types.Type.Unicst;
				payload2.Msg = String.Format("#B{0:D3}06V{1:D5}", targetModem.Id, voltage);
				
				payload2.SenderId = (uint) nanomodem.Id;
				
				AcousticPayload acousticPayload2 = new AcousticPayload();
				acousticPayload2.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload2.Payload = payload2;
				payloadList.Add(acousticPayload2);
			}

			// Echo message
			else if(req.ReqType == NanomodemRequest.Types.Type.Echomsg)
			{
				AcousticMessage msg = new AcousticMessage();
				msg.Message = message;
				msg.sender = nanomodem;
				nanomodem.medium.Transmit(msg, nanomodem.medium.GetNanomodemById(req.Id));

				payload = new NanomodemPayload();
				payload.MsgType = NanomodemPayload.Types.Type.Unicst;
				payload.Msg = message.Substring(0, 7); // $Exxxnn
				payload.SenderId = (uint) nanomodem.Id;
				
				acousticPayload = new AcousticPayload();
				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
				payloadList.Add(acousticPayload);
			}
			
			// quality check
			else if(req.ReqType == NanomodemRequest.Types.Type.Quality)
			{
				payload = new NanomodemPayload();
				payload.MsgType = NanomodemPayload.Types.Type.Unicst;
				payload.Msg = "$C0";
				payload.SenderId = (uint) nanomodem.Id;
				
				acousticPayload = new AcousticPayload();
				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
				payloadList.Add(acousticPayload);
			}

			// unicast and acknowledge
			else if(req.ReqType == NanomodemRequest.Types.Type.Unistack)
			{
				payload = new NanomodemPayload();
				payload.MsgType = NanomodemPayload.Types.Type.Unicst;
				payload.Msg = message.Substring(0, 7); // $Mxxxnn
				payload.SenderId = (uint) nanomodem.Id;
				
				acousticPayload = new AcousticPayload();
				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
				payloadList.Add(acousticPayload);


				AcousticMessage msg = new AcousticMessage();
				msg.Message = message;
				msg.sender = nanomodem;
				bool sent = nanomodem.medium.Transmit(msg, nanomodem.medium.GetNanomodemById((uint) req.Id));
				
				if (sent)
				{
					// send #RxxxTyyyyy msg
					int targetId = (int) req.Id;
					int rangeTransformed = nanomodem.GetRangeTransformed(targetId);
					NanomodemPayload responsePayload = new NanomodemPayload();
					responsePayload.MsgType = NanomodemPayload.Types.Type.Unicst;
					responsePayload.Msg = String.Format("#R{0:3D}T{1:5D}", targetId, rangeTransformed);
					responsePayload.SenderId = (uint) nanomodem.Id;

					acousticPayload = new AcousticPayload();
					acousticPayload.Address = "nanomodem/" + nanomodem.Id;
					acousticPayload.Payload = responsePayload;
					payloadList.Add(acousticPayload);
				}
			}
			return payloadList;
		}
	

		/// <summary>
		/// Handles messages from other nanomodems
		/// </summary>
		public void ExecuteNanomodemResponse(AcousticMessage request)
		{
			string message = request.Message;
			NanomodemPayload payload = new NanomodemPayload();
			payload.MsgType = NanomodemPayload.Types.Type.Unicst;
			

			AcousticPayload acousticPayload = new AcousticPayload();
			
			// Handle broadcast message
			if (message.StartsWith("$B")) {
				// return #Bxxxnnddd to acknowledge received message
				payload.Msg = String.Format("#B{0:D3}{1}", ((Nanomodem) request.sender).Id, request.Message.Substring(2));
				payload.SenderId = (uint) nanomodem.Id;

				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
			}

			// handle unicast message
			else if (message.StartsWith("$U") || message.StartsWith("$M")) {
				// return #Unnddd to acknowledge received message
				payload.Msg = String.Format("#U{0}", request.Message.Substring(5));
				payload.SenderId = (uint) nanomodem.Id;

				acousticPayload.Address = "nanomodem/" + nanomodem.Id;
				acousticPayload.Payload = payload;
			}

			streamHandle = acousticsClient.ReturnAcousticPayload(
				acousticPayload,
				cancellationToken: RosConnection.Instance.cancellationToken);
		}
	}
}
