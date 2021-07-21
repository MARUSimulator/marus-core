using UnityEngine;
using System;
using System.Collections.Generic;
using Labust.Sensors.AIS;
using Labust.Utils;

namespace Labust.Sensors.Primitive.GenericMedium
{
	/// <summary>
	/// This class serves as base for a medium for sending/receiving messages.
	/// </summary>
	public abstract class MediumBase<T> : GenericSingleton<MediumBase<T>> where T: MediumMessage<T> 
	{
		public List<MediumDeviceBase<T>> RegisteredDevices = new List<MediumDeviceBase<T>>();

		/// <summary>
		/// Registers devices so messages can be broadcast to them.
		/// </summary>
		public virtual void Register(MediumDeviceBase<T> device)
		{
			RegisteredDevices.Add(device);
		}

		/// <summary>
		/// Broadcasts message to all registered objects
		/// </summary>
		/// <param name="msg">Message object to be sent.</param>
		public virtual void Broadcast(T msg)
		{
			foreach (var device in RegisteredDevices)
			{
				if (msg.sender != device)
				{
					this.Transmit(msg, device);
				}
			}
		}

		/// <summary>
		/// Transmit message to single other object.
		/// Message will only be sent if other object is in range of the sender device. 
		/// Method assumes distance and range are of the same unit and magnitude. Default is meters (m).
		/// </summary>
		/// <param name="msg">Message object to be sent.</param>
		/// <param name="receiver">Object which message is sent to.</param>
		/// <returns>True if transmission succeeded, false if not (not in range).</returns>
		public virtual Boolean Transmit(T msg, MediumDeviceBase<T> receiver)
		{   
			MediumDeviceBase<T> sender = msg.sender;
			float distance = DistanceFromTo(sender, receiver);
			if (sender.Range >= distance)
			{
				receiver.Receive(msg);
				return true;
			}
			return false;
		}

		public float DistanceFromTo(MediumDeviceBase<T> deviceA, MediumDeviceBase<T> deviceB)
		{
			return Vector3.Distance(deviceA.gameObject.transform.position, deviceB.gameObject.transform.position);
		}
	}
}
