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

using System;
using System.ComponentModel;
using System.Net.Sockets;
using Labust.Sensors.Primitive;
using UnityEngine;
using UnityEngine.Serialization;

namespace Labust.Core
{
    public class GeographicFrame
    {
        // [Header("Tangent plane origin")] 
        public GeoPoint origin { get; }
        public Geometry.Vector3 originEcef { get; }
        public Transform transform { get; }

        [Header("Debug")] 
        public Geometry.Vector3 ecef;

        public GeographicFrame(Transform transform, double latitude, double longitude, double altitude)
        {
            this.transform = transform;
            origin = new GeoPoint(latitude, longitude, altitude);
            originEcef = GeoPoint.Geodetic2ecef(origin);
        }        
        
        public GeoPoint Unity2Geo(Vector3 position)
        {
            var enu = position.Unity2Map();
            ecef = GeoPoint.Enu2Ecef(this, enu);
            return new GeoPoint(ecef);
        }

    }
}