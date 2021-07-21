using System;
using Sensorstreaming;
using UnityEngine;

namespace Labust.Sensors.Primitive
{
    [RequireComponent(typeof(SonarPrimitive))]
    public class SonarPrimitiveROS : SensorStreamer<SonarStreamingRequest>
    {
        protected override void SendMessage()
        {
        }
    }
}