using Labust.Networking;
using Labust.Sensors;
using Sensorstreaming;
using UnityEngine;

namespace Labust.Sensors
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
            if (string.IsNullOrEmpty(sensorId))
                sensorId = vehicle.name + "/depth";
        }

        public async override void SendMessage()
        {
            await streamWriter.WriteAsync( new DepthStreamingRequest
            {
                SensorId = sensorId,
                Depth = depth
            });
            hasData = false;
        }

        public void Refresh()
        {
            depth = -transform.position.y;
            hasData = true;
        }
    }
}
