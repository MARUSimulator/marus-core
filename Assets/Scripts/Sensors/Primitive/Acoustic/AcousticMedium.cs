using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Labust.Sensors.Primitive.GenericMedium;

namespace Labust.Sensors.Primitive.Acoustic
{
	public class AcousticMedium : MediumBase<AcousticMessage>
	{
		protected AcousticMedium() { }
		
		public string Name = "Acoustic medium";

		/// <summary>
		/// Speed of sound in medium (in meters per second)
		/// Default is average speed of sound in sea water.
		/// </summary>
		public float C = 1500f;

		public override void Broadcast(AcousticMessage msg)
		{	
			foreach (var device in RegisteredDevices)
			{
				if (msg.sender != device)
				{
					StartCoroutine(SendMsg(msg, device));
				}
			}
		}

		public override bool Transmit(AcousticMessage message, MediumDeviceBase<AcousticMessage> receiver)
		{
			if (DistanceFromTo(message.sender, receiver) <= message.sender.Range){
				StartCoroutine(SendMsg(message, receiver));
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary> 
		/// Delay message by message duration + time to target
		/// </summary>
		IEnumerator SendMsg(AcousticMessage msg, MediumDeviceBase<AcousticMessage> receiver)
		{	
			// Emulate delay time based on speed of sound in medium and distance
			float delayTime = msg.MessageDuration() + (DistanceFromTo(msg.sender, receiver) / C);
			yield return new WaitForSeconds(delayTime);
			base.Transmit(msg, receiver);
		}

		/// <summary> 
		/// Returns Nanomodem object based on id.
		/// </summary>
		public Nanomodem GetNanomodemById(uint id)
		{
			foreach(Nanomodem modem in RegisteredDevices)
			{
				if (modem.Id == id) return modem;
			}
			return null;
		}
	}
}
