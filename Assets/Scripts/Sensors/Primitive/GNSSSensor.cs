using System.Collections;
using System.Collections.Generic;
using Labust.Core;
using Labust.Networking;
using Sensorstreaming;
using UnityEngine;

namespace Labust.Sensors.Primitive
{
    public class GNSSSensor : SensorBase<GnssStreamingRequest>
    {
        [Header("Position")]
        public GeographicFrame origin;
        public GeoPoint point;

        [Header("Precision")]
        public double[] covariance;
        public bool isRTK = true;


        void Start()
        {
            covariance = new double[] { 0.1, 0, 0, 0, 0.1, 0, 0, 0, 0.1 };
            AddSensorCallback(SensorCallbackOrder.Last, Refresh);
            streamHandle = streamingClient.StreamGnssSensor(cancellationToken: RosConnection.Instance.cancellationToken);
            if (string.IsNullOrEmpty(address))
                address = vehicle.name + "/gps";
        }

        void Refresh()
        {
            var world = RosConnection.Instance.WorldFrame;
            point = world.Unity2Geo(transform.position);
            Log(new { point.latitude, point.longitude, point.altitude });
            hasData = true;
        }

        public async override void SendMessage()
        {
            var msg = new GnssStreamingRequest
            {
                Address = address,
                Point = new Common.GeoPoint
                {
                    Latitude = point.latitude,
                    Longitude = point.longitude,
                    Altitude = point.altitude
                }
            };
            await _streamWriter.WriteAsync(msg);
            hasData = false;
        }
    }
}