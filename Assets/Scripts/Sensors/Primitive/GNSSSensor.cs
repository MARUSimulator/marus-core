using System;
using System.Collections;
using System.Collections.Generic;
using Labust.Core;
using Labust.Networking;
using Sensor;
using Sensorstreaming;
using Std;
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
        public float maximumOperatingDepth = 0.5f;


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
            var world = TfHandler.Instance.OriginGeoFrame;
            point = world.Unity2Geo(transform.position);
            Log(new { point.latitude, point.longitude, point.altitude });
            hasData = true;
        }

        public override async void SendMessage()
        {
            var msg = new GnssStreamingRequest
            {
                Address = address,
                Data = new NavSatFix
                {
                    Header = new Header
                    {
                        FrameId = frameId,
                        Timestamp = TimeHandler.Instance.TimeDouble
                    },
                    Status = new NavSatStatus
                    {
                        Service = NavSatStatus.Types.Service.Gps,
                        Status = isRTK?NavSatStatus.Types.Status.GbasFix:NavSatStatus.Types.Status.SbasFix
                    },
                    Latitude = point.latitude,
                    Longitude = point.longitude,
                    Altitude = point.altitude
                }
            };
            if (transform.position.y > -maximumOperatingDepth) await _streamWriter.WriteAsync(msg);
            hasData = false;
        }
    }
}