using UnityEngine;

namespace Labust.Sensors.AIS
{
    /// <summary>
    /// This class serves as a base for AIS message types.
    /// For more reference see <see cref="!:https://www.navcen.uscg.gov/?pageName=AISMessages">here.</see>
    /// </summary>
    public abstract class AisMessage
    {
        /// <summary>
        /// Message type. Is always 1, 2 or 3.
        /// </summary>
        public AISMessageType MessageType { get; set; }

        public AisDevice sender { get; set; }

        /// <summary>
        /// Maritime Mobile Service Identity
        /// Unique 9 digit number assigned to radio or AIS unit.
        /// </summary>
        public int MMSI { get; set; }
    }
}
