using Geometry;
using Labust.Core;
using Labust.Networking;
using Labust.Sensors.Primitive;
using Sensorstreaming;
using Std;
using UnityEngine;

namespace Labust.Sensors.ROS
{

    [RequireComponent(typeof(DvlSensor))]
    public class DvlROS : SensorStreamer<DvlStreamingRequest>
    {
        DvlSensor sensor;
        void Start()
        {
            sensor = GetComponent<DvlSensor>();
            if (string.IsNullOrEmpty(address))
                address = sensor.vehicle.name + "/dvl";
            StreamSensor(streamingClient?.StreamDvlSensor(cancellationToken:RosConnection.Instance.cancellationToken));
        }

        protected async override void SendMessage()
        {
            var dvlOut = new TwistWithCovarianceStamped
            {
                Header = new Header()
                {
                    FrameId = sensor.frameId,
                    Timestamp = TimeHandler.Instance.TimeDouble
                },
                Twist = new TwistWithCovariance 
                {
                    Twist = new Twist
                    {
                        Linear = sensor.groundVelocity.Unity2Body().AsMsg()
                    }
                }
            };
            dvlOut.Twist.Covariance.AddRange(sensor.velocityCovariance);
            
            var request = new DvlStreamingRequest
            {
                Address = address,
                Data = dvlOut
            };
            await _streamWriter.WriteAsync(request);
            hasData = false;
        }
    }
}