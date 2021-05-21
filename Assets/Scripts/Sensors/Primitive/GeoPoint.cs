using UnityEngine;

namespace Simulator.Sensors
{
    [System.Serializable]
    public class GeoPoint
    {
        public double latitude;
        public double longitude;
        public double altitude;

        public GeoPoint(double latitude, double longitude, double altitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = altitude;
        }
    }
}