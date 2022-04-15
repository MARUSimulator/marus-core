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

namespace Marus.Sensors.Acoustics
{
    public static class AcousticMediumHelper
    {
        static Dictionary<int, (int, List<int>)> _mediumDevicesCache = new Dictionary<int, (int, List<int>)>();
        static AcousticDevice[] _allDevicesCache;
        static AcousticDevice[] AllDevicesCache
        {
            get
            {
                var currentFrame = Time.frameCount;
                if (currentFrame == _allDevicesLastFrame)
                {
                    return _allDevicesCache;
                }
                var devices = Object.FindObjectsOfType<AcousticDevice>(includeInactive: false);
                _allDevicesCache = devices.ToArray();
                return _allDevicesCache;
            }
        }
        static AcousticMedium[] _allAcousticMediums;
        static int _allDevicesLastFrame;
        static double _timeChecked;


        public static AcousticDevice[] GetAllAcousticDevices()
        {
            return AllDevicesCache;
        }


        public static AcousticMedium GetAcousticMediumForTransform(Transform transform)
        {
            if (_allAcousticMediums is null)
                _allAcousticMediums = Object.FindObjectsOfType<AcousticMedium>();
            return _allAcousticMediums.FirstOrDefault(x => x.IsPointInside(transform));
        }

        public static AcousticDevice[] GetAcousticDevicesOnProtocol(string protocol)
        {
            var devices = GetAllAcousticDevices();
            return devices.Where(x => x.Protocol == protocol).ToArray();
        }
        
        public static AcousticDevice[] GetDevicesInMeduim(AcousticMedium medium)
        {
            var mediumId = medium.GetInstanceID();
            var currentFrame = Time.frameCount;
            if (_mediumDevicesCache.TryGetValue(mediumId, out var tuple))
            {
                (var frame, var list) = tuple;
                if (frame == currentFrame)
                {
                    var devices = list
                        .Select(x => (AcousticDevice)EditorUtility.InstanceIDToObject(x));
                    return devices.ToArray();
                }
            }

            var accDevices = Object.FindObjectsOfType<AcousticDevice>(includeInactive:false)
                                   .Where(x => medium.IsPointInside(x.transform)).ToArray();

            var devicesList = new List<int>();
            devicesList.AddRange(accDevices.Select(x => x.GetInstanceID()));
            _mediumDevicesCache[mediumId] = (currentFrame, devicesList);

            return accDevices;
        }

        public static AcousticDevice GetAcousticDeviceById(int id)
        {
            return AllDevicesCache.FirstOrDefault(x => x.DeviceId == id);
        }

        public static T GetAcousticDeviceById<T>(int id) where T : AcousticDevice
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

    }
}