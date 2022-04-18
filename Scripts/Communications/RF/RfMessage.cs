
namespace Marus.Communications.Rf
{
    public abstract class RfMessage
    {
        public int SenderId;
        public string Protocol;
        public TransmitionType TransmitionType { get; set; }

        public RfTransmiterParams TransmiterParams;
    }
    public enum TransmitionType
    {
        Broadcast,
        Unicast
    }
}
