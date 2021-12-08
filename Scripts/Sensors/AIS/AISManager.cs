using UnityEngine;
using System;
using System.Collections.Generic;
using Labust.Utils;

namespace Labust.Sensors.AIS
{
    /// <summary>
    /// This class serves as a radio medium for AIS purposes.
    /// Script should be attached to a GameObject so no singleton is needed.
    /// </summary>
    public class AisManager: Singleton<AisManager> //MediumBase<AISMessage>
    {

        protected AisManager() { }
        public string Name;
        public List<AisDevice> RegisteredDevices;

        protected override void Initialize()
        {
            Name = "Radio Medium for AIS";
            RegisteredDevices = new List<AisDevice>();
        }

        /// <summary>
        /// Registers devices so messages can be broadcast to them.
        /// </summary>
        public virtual void Register(AisDevice device)
        {
            RegisteredDevices.Add(device);
        }

        /// <summary>
        /// Broadcasts message to all registered objects
        /// </summary>
        /// <param name="msg">Message object to be sent.</param>
        public virtual void Broadcast(AisMessage msg)
        {
            foreach (var device in RegisteredDevices)
            {
                if (msg.sender != device)
                {
                    this.Transmit(msg, device);
                }
            }
        }

        /// <summary>
        /// Transmit message to single other object.
        /// Message will only be sent if other object is in range of the sender device. 
        /// Method assumes distance and range are of the same unit and magnitude. Default is meters (m).
        /// </summary>
        /// <param name="msg">Message object to be sent.</param>
        /// <param name="receiver">Object which message is sent to.</param>
        /// <returns>True if transmission succeeded, false if not (not in range).</returns>
        public virtual Boolean Transmit(AisMessage msg, AisDevice receiver)
        {
            var sender = msg.sender;
            float distance = DistanceFromTo(sender, receiver);
            if (sender.Range >= distance)
            {
                receiver.Receive(msg);
                return true;
            }
            return false;
        }

        public float DistanceFromTo(AisDevice deviceA, AisDevice deviceB)
        {
            return Vector3.Distance(deviceA.gameObject.transform.position, deviceB.gameObject.transform.position);
        }
    }
}
