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
        public Vector3 originEcef { get; }
        public Transform transform { get; }

        [Header("Debug")] 
        public Vector3 ecef;

        public GeographicFrame(Transform transform, float latitude, float longitude, float altitude)
        {
            this.transform = transform;
            origin = new GeoPoint(latitude, longitude, altitude);
            originEcef = GeoPoint.Geodetic2ecef(origin);
        }        
        
        public GeoPoint Unity2Geo(Vector3 position)
        {
            var enu = position.Unity2Map();

            ecef = GeoPoint.Enu2ECEF(this, enu);
            return new GeoPoint(ecef);
        }

    }
}