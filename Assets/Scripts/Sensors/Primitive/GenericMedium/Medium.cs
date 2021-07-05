using UnityEngine;
using System;
using System.Collections.Generic;
using Labust.Sensors.AIS;
using Labust.Other;

namespace Labust.Sensors.Primitive.GenericMedium
{
    /// <summary>
    /// This class serves as base for a medium for sending/receiving messages.
    /// </summary>
    public abstract class Medium<T> : GenericSingleton<Medium<T>>
    {
        public List<MediumDevice<T>> RegisteredDevices = new List<MediumDevice<T>>();

        /// <summary>
        /// Registers devices so messages can be broadcast to them.
        /// </summary>
        public void Register(MediumDevice<T> device)
        {
            RegisteredDevices.Add(device);
        }

        /// <summary>
        /// Broadcasts message to all registered objects
        /// </summary>
        /// <param name="msg">Message object to be sent.</param>
        public void Broadcast(MediumMessage<T> msg)
        {
            foreach (MediumDevice<T> device in RegisteredDevices)
            {
                if (msg.sender != device)
                {
                    this.Transmit(msg, device);
                }
            }
        }

        /// <summary>
        /// Transmit message to single other object
        /// </summary>
        /// <param name="msg">Message object to be sent.</param>
        /// <param name="receiver">Object which message is sent to.</param>
        public void Transmit(MediumMessage<T> msg, MediumDevice<T> receiver)
        {   
            MediumDevice<T> sender = msg.sender;
            float distance = Vector3.Distance(receiver.gameObject.transform.position, sender.gameObject.transform.position);
            if (receiver.Range >= distance)
            {
                receiver.Receive(msg);
            }
        }
    }
}
