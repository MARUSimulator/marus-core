
namespace Labust.Sensors.Primitive.GenericMedium
{
	public class MediumMessageBase<T>
	{
		/// <summary>
		/// Object which sent the message
		/// </summary>
		public MediumDeviceBase<T> sender;

		/// <summary>
		/// Message payload object of generic type.
		/// </summary>
		public T message;

		public MediumMessageBase(MediumDeviceBase<T> sender, T message)
		{
			this.sender = sender;
			this.message = message;
		}

		public override string ToString()
		{
			return string.Format("Sender: {0}\nMessage: {1}",sender, message);
		}  
	}
}
