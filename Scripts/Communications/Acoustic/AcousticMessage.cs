
namespace Marus.Communications.Acoustics
{
    public abstract class AcousticMessage
    {
        public int SenderId;
        public string Protocol;
        public TransmitionType TransmitionType { get; set; }

        public AcousticTransmiterParams TransmiterParams;

        public float CompositionDuration;
    }
    public enum TransmitionType
    {
        Broadcast,
        Unicast
    }
}
