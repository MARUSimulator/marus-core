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
    /// <summary>
    /// Imu sensor implementation
    /// </summary>
    [RequireComponent(typeof(ImuSensor))]
    public class ImuROS : SensorStreamer<ImuStreamingRequest>
    {
        ImuSensor sensor;
        void Start()
        {
            sensor = GetComponent<ImuSensor>();
            if (string.IsNullOrEmpty(address))
                address = $"{sensor.vehicle.name}/imu";
            StreamSensor(streamingClient?.StreamImuSensor(cancellationToken:RosConnection.Instance.cancellationToken));
        }

        new void Update()
        {
            hasData = sensor.hasData;
            base.Update();
        }

        protected override async void SendMessage()
        {
            var imuOut = new Imu()
            {
                Header = new Header
                {
                    FrameId = sensor.frameId,
                    Timestamp = TimeHandler.Instance.TimeDouble
                },
                Orientation = sensor.orientation.Unity2Map().AsMsg(),
                AngularVelocity = (-sensor.angularVelocity).Unity2Map().AsMsg(),
                LinearAcceleration = sensor.linearAcceleration.Unity2Map().AsMsg(),
            };
            imuOut.OrientationCovariance.AddRange(sensor.orientationCovariance);
            imuOut.LinearAccelerationCovariance.AddRange(sensor.linearAccelerationCovariance);
            imuOut.AngularVelocityCovariance.AddRange(sensor.angularVelocityCovariance);

            await _streamWriter.WriteAsync(new ImuStreamingRequest
            {
                Data = imuOut,
                Address = address
            });
            hasData = false;
        }
    }
}