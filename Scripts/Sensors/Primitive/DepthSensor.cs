namespace Labust.Sensors.Primitive
{
    /// <summary>
    /// Depth sensor implementation
    /// </summary>
    public class DepthSensor : SensorBase
    {
        double depth;
        public double covariance;

        protected override void SampleSensor()
        {
            depth = -transform.position.y;
            Log(new { depth });
            hasData = true;
        }
    }

}
