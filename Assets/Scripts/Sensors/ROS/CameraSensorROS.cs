using UnityEngine;
using UnityEngine.Rendering;
using Google.Protobuf;
using Sensorstreaming;
using Labust.Sensors.Core;
using System;
using Labust.Networking;

namespace Labust.Sensors
{
    /// <summary>
    /// Camera sensor implementation
    /// </summary>
    [RequireComponent(typeof(CameraSensor))]
    public class CameraSensorROS : SensorStreamer<CameraStreamingRequest>
    {
        CameraSensor sensor;
        void Start()
        {
            sensor = GetComponent<CameraSensor>();
            if (string.IsNullOrEmpty(address))
                address = sensor.vehicle.name + "/camera";
            StreamSensor(streamingClient?.StreamCameraSensor(cancellationToken:RosConnection.Instance.cancellationToken));
        }

        protected async override void SendMessage()
        {
            try
            {
                await _streamWriter.WriteAsync(new CameraStreamingRequest
                {
                    Data = ByteString.CopyFrom(sensor.Data),
                    TimeStamp = Time.time,
                    Address = address,
                    Height = (uint)(sensor.PixelHeight / sensor.ImageCrop),
                    Width = (uint)(sensor.PixelWidth / sensor.ImageCrop)
                });
                hasData = false;
            }
            catch (Exception e)
            {
                Debug.Log("Possible message overflow.");
                Debug.LogError(e);
            }
        }

    }
}