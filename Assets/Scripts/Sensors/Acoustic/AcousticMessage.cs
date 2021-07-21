
namespace Labust.Sensors.Acoustics
{
    public abstract class AcousticMessage
    {
        public int SenderId;
        public string Protocol;
        public TransmitionType TransmitionType { get; set; }

        public PhysicalParams PhysicalParams;

        public float CompositionDuration;
    }
    public enum TransmitionType
    {
        Broadcast,
        Unicast
    }
}
