using System;
using Labust.Core;
using Labust.Networking;
using Sensorstreaming;
using UnityEngine;

namespace Labust.Sensors.Primitive
{
    [RequireComponent(typeof(Sonar3D))]
    public class Sonar3DROS : SensorStreamer<PointCloudStreamingRequest>
    {
        Sonar3D sensor;
        void Start()
        {
            sensor = GetComponent<Sonar3D>();
            if (string.IsNullOrEmpty(address))
                address = transform.name + "/sonar3d";
            StreamSensor(streamingClient?.StreamSonarSensor(cancellationToken: RosConnection.Instance.cancellationToken));
        }

        void Update()
        {
            hasData = sensor.hasData;
            base.Update();
        }

        protected async override void SendMessage()
        {
            Sensor.PointCloud _pointCloud = new Sensor.PointCloud();
            foreach (Vector3 point in sensor.pointsCopy)
            {
                var tmp = TfExtensions.Unity2Map(point);
                Geometry.Point p = new Geometry.Point()
                {
                    X = tmp.x,
                    Y = tmp.y,
                    Z = tmp.z
                };
                _pointCloud.Points.Add(p);
            }

            _pointCloud.Header = new Std.Header()
            {
                FrameId = RosConnection.Instance.OriginFrameName,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000.0
            };

            var msg = new PointCloudStreamingRequest()
            {
                Data = _pointCloud,
                Address = address
            };
            await _streamWriter.WriteAsync(msg);
            hasData = false;
        }
    }
}