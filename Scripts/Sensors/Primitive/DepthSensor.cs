using System;

namespace Labust.Sensors.Primitive
{
    /// <summary>
    /// Depth sensor implementation
    /// </summary>
    public class DepthSensor : SensorBase
    {
        public bool debug = true;
        [NonSerialized] public double depth;
        [NonSerialized] public double covariance;

        [ReadOnly] public double Depth;

        protected override void SampleSensor()
        {
            depth = -transform.position.y;
            if (debug)
            {
                Depth = depth;
            }
            Log(new { depth });
            hasData = true;
        }
    }
}
