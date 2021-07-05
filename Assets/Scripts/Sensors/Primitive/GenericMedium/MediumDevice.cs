using UnityEngine;

namespace Labust.Sensors.Primitive.GenericMedium
{
    public abstract class MediumDevice<T> : MonoBehaviour
    {
        public float Range;
        public abstract void Receive<T>(MediumMessage<T> message);
    }
}
