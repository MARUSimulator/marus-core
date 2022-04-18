// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System;

namespace Marus.Communications.Acoustics
{
    /// <summary>
    /// This class serves as base for a medium for sending/receiving messages.
    /// </summary>
    [RequireComponent(typeof(BoxVolume))]
    public abstract class MediumBase : MonoBehaviour
    {

        BoxVolume _boxVolume;

        protected void Awake()
        {
            _boxVolume = GetComponent<BoxVolume>();
        }

        // /// <summary>
        // /// Registers devices so messages can be broadcast to them.
        // /// </summary>
        // public virtual void Register(AcousticDevice<T> device)
        // {
        //     RegisteredDevices.Add(device);
        // }

        // /// <summary>
        // /// Broadcasts message to all registered objects
        // /// </summary>
        // /// <param name="msg">Message object to be sent.</param>
        // public virtual void Broadcast(T msg)
        // {
        //     foreach (var device in RegisteredDevices)
        //     {
        //         if (msg.sender != device)
        //         {
        //             this.Transmit(msg, device);
        //         }
        //     }
        // }

        // /// <summary>
        // /// Transmit message to single other object.
        // /// Message will only be sent if other object is in range of the sender device. 
        // /// Method assumes distance and range are of the same unit and magnitude. Default is meters (m).
        // /// </summary>
        // /// <param name="msg">Message object to be sent.</param>
        // /// <param name="receiver">Object which message is sent to.</param>
        // /// <returns>True if transmission succeeded, false if not (not in range).</returns>
        // public virtual Boolean Transmit(T msg, AcousticDevice<T> receiver)
        // {
        //     AcousticDevice<T> sender = msg.sender;
        //     float distance = DistanceFromTo(sender, receiver);
        //     if (sender.Range >= distance)
        //     {
        //         receiver.Receive(msg);
        //         return true;
        //     }
        //     return false;
        // }

        public bool IsPointInside(Transform transform)
        {
            if (_boxVolume.Type == BoxVolume.BoxType.World)
                return true;
            
            var pos = transform.position;
            if (_boxVolume.Type == BoxVolume.BoxType.HalfSpace)
            {
                var rotation = _boxVolume.rotate;
                rotation.ToAngleAxis(out _, out Vector3 axis);
                var center = _boxVolume.bounds.center;
                var relativeP = pos - center;
                return Vector3.Dot(relativeP, axis) > 0;
            }
            if (_boxVolume.Type == BoxVolume.BoxType.Box)
            {
                throw new NotImplementedException("Box type not yet supported");
            }
            throw new Exception("Invalid medium box type");
        }
    }
}
