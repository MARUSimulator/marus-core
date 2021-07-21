using Labust.Sensors.Primitive.GenericMedium;
using System;

namespace Labust.Sensors.Primitive.Acoustic
{
	public class AcousticMessage : MediumMessage<AcousticMessage>
	{	
		/// <summary>
		/// String message used by nanomodems.
		/// eg. $P001, $Q? etc.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Nanomodem device which sent the message.
		/// </summary>
		public MediumDeviceBase<AcousticMessage> sender { get; set; }

		public AcousticMessage() {
		}

		/// <summary>
		/// Message generatoin duration which is dependant on message length
		/// header duration + duration of actual message
		/// </summary>
		public float MessageDuration()
		{
			float byteDuration = 0.0125f;
			float headerDuration = 0.105f;
			if(Message.StartsWith("$B"))
			{
				int numOfBytes = Int32.Parse(Message.Substring(2, 2));
				return headerDuration + (numOfBytes + 16) * byteDuration;
			}
			else if(Message.StartsWith("$E") || Message.StartsWith("$U") || Message.StartsWith("$M"))
			{
				int numOfBytes = Int32.Parse(Message.Substring(5, 2));
				return headerDuration + (numOfBytes + 16) * byteDuration;
			}

			return headerDuration;
		}

		public override string ToString()
		{
			return Message;
		}
	}
}
