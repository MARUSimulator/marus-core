using UnityEngine;
using Labust.Networking;

namespace Labust.Sensors.Primitive.GenericMedium
{
	/// <summary>
	/// Abstract class to define behaviour and properties of device in a medium. E.g. nanomodem, ais transponder etc.
	/// </summary>
	public abstract class MediumDeviceBase<T> : MonoBehaviour where T: MediumMessage<T> 
	{

		/// <summary>
		/// Transmitting range, in meters (m).
		/// </summary>
		public float Range;

		/// <summary>
		/// Processing received messages.
		/// </summary>
		public virtual void Receive(T message)
		{
		}
	}
}
