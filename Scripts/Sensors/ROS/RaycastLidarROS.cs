using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Labust.Networking;
using Labust.Sensors;
using Labust.Sensors.Core;
using Labust.Visualization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Sensorstreaming;
using System.Threading;
using Labust.Core;

namespace Labust.Sensors
{

    /// <summary>
    /// Lidar that cast N rays evenly distributed in configured field of view.
    /// Implemented using IJobParallelFor on CPU
    /// Can drop performance
    /// </summary>
    [RequireComponent(typeof(RaycastLidar))] 
    public class RaycastLidarROS : SensorStreamer<PointCloudStreamingRequest>
    {
        
        RaycastLidar sensor;
        void Start()
        {
            sensor = GetComponent<RaycastLidar>();
            if (string.IsNullOrEmpty(address))
                address = transform.name + "/lidar";
            StreamSensor(streamingClient?.StreamLidarSensor(cancellationToken: RosConnection.Instance.cancellationToken));
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