
namespace Labust.Sensors.Primitive.GenericMedium
{
	public interface MediumMessage<T> where T: MediumMessage<T>
	{
		/// <summary>
		/// Device which sent the message
		/// </summary>
		public MediumDeviceBase<T> sender { get; set; }

	}
}
