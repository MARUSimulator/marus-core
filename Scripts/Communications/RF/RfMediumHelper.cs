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

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Marus.Communications.Rf
{
    public static class RfMediumHelper
    {
        static Dictionary<int, (int, List<int>)> _mediumDevicesCache = new Dictionary<int, (int, List<int>)>();
        static RfDevice[] _allDevicesCache;
        static RfDevice[] AllDevicesCache
        {
            get
            {
                var currentFrame = Time.frameCount;
                if (currentFrame == _allDevicesLastFrame)
                {
                    return _allDevicesCache;
                }
                var devices = Object.FindObjectsOfType<RfDevice>(includeInactive: false);
                _allDevicesCache = devices.ToArray();
                return _allDevicesCache;
            }
        }
        static int _allDevicesLastFrame;
        static double _timeChecked;


        public static RfDevice[] GetAllRfDevices()
        {
            return AllDevicesCache;
        }


        public static RfDevice[] GetRfDevicesOnProtocol(string protocol)
        {
            var devices = GetAllRfDevices();
            return devices.Where(x => x.Protocol == protocol).ToArray();
        }
        
        public static RfDevice GetRfDeviceById(int id)
        {
            return AllDevicesCache.FirstOrDefault(x => x.DeviceId == id);
        }

        public static T GetRfDeviceById<T>(int id) where T : RfDevice
        {
            var f = AllDevicesCache.FirstOrDefault(x => x.DeviceId == id);
            if (f != null)
            {
                if (f is T asT)
                {
                    return asT;
                }
            }
            return null;
        }

        /// <summary>
        /// Sends message to every registered rf device.
        /// </summary>
        public static void Broadcast<T>(T msg) where T : RfMessage
        {
            foreach (var device in GetRfDevicesOnProtocol(msg.Protocol))
            {
                if (msg.SenderId != device.DeviceId)
                {
                    _Transmit(msg, device);
                }
            }
        }

        /// <summary>
        /// RfDevice does not have to have the same message as generic. If it knows how to decode
        /// the protocoll, it will receive the message
        /// </summary>
        public static bool Transmit<T>(T message, RfReceiver receiver) where T : RfMessage
        {
            if (message.Protocol == receiver.Protocol)
            {
                return _Transmit(message, receiver);
            }
            return false;
        }

        /// <summary>
        /// Transmit message to single other object.
        /// Message will only be sent if other object is in range of the sender device. 
        /// Method assumes distance and range are of the same unit and magnitude. Default is meters (m).
        /// </summary>
        /// <param name="msg">Message object to be sent.</param>
        /// <param name="receiver">Object which message is sent to.</param>
        /// <returns>True if transmission succeeded, false if not (not in range).</returns>
        static bool _Transmit<T>(T msg, RfReceiver receiver) where T : RfMessage
        {

            // IMPLEMENT COMPLEX PHYSICS
            bool checkRange = CheckRange(msg, receiver);
            if (checkRange)
            {
                receiver.OnReceive(msg);
                return true;
            }
            return false;
        }

        private static bool CheckRange<T>(T msg, RfReceiver receiver) where T : RfMessage
        {
            if (msg.TransmiterParams.MaxRange <= 0 ) return true;
            float distance = Vector3.Distance(msg.TransmiterParams.SourceLocation, receiver.Location);
            return distance < msg.TransmiterParams.MaxRange;
        }

        /// <summary>
        /// Emulates time needed for message to arrive and sends it to other nanomodem.
        ///
        /// AcousticDevice does not have to have the same message as generic. If it knows how to decode
        /// the protocoll, it will receive the message
        /// </summary>
        public static float Range(RfDevice source, RfDevice target)
        {
            if (source.Protocol != target.Protocol)
            {
                return -1;
            }

            var sourceParams = source.GetTransmiterParams();
            var targetParams = target.GetTransmiterParams();
            var sourceLocation = source.transform;
            var targetLocation = target.transform;

            //TODO:  DO SOME PHYSICS

            return Vector3.Distance(sourceLocation.position, targetLocation.position);
        }

    }
}