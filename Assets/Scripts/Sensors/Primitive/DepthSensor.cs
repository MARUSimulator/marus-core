using Labust.Networking;
using Labust.Sensors;
using Sensorstreaming;
using UnityEngine;

namespace Labust.Sensors.Primitive
{
    /// <summary>
    /// Depth sensor implementation
    /// </summary>
    public class DepthSensor : SensorBase<DepthStreamingRequest>
    {
        float depth;

        public void Awake()
        {
            streamHandle =  streamingClient.StreamDepthSensor(cancellationToken:RosConnection.Instance.cancellationToken);
            AddSensorCallback(SensorCallbackOrder.Last, Refresh);
            if (string.IsNullOrEmpty(address))
                address = vehicle.name + "/depth";
        }

        public async override void SendMessage()
        {
            await _streamWriter.WriteAsync( new DepthStreamingRequest
            {
                Address = address,
                Depth = depth
            });
            hasData = false;
        }

        public void Refresh()
        {
            var o = new { V1 = depth, V3 = "gfdsg"};
            depth = -transform.position.y;
            Log(new { depth });
            hasData = true;
        }
    }
}
