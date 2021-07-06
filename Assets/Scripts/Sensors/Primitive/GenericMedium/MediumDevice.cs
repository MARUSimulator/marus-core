using UnityEngine;

namespace Labust.Sensors.Primitive.GenericMedium
{
    /// <summary>
    /// Abstract class to define behaviour and properties of device in a medium. E.g. nanomodem, ais transponder etc.
    /// </summary>
    public abstract class MediumDevice<T> : MonoBehaviour
    {

        /// <summary>
        /// Transmitting range, in meters (m).
        /// </summary>
        public float Range;

        /// <summary>
        /// Method that receives messages.
        /// </summary>
        public abstract void Receive<T>(MediumMessage<T> message);
    }
}
