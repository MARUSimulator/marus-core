using UnityEngine;

namespace Simulator.Sensors
{
    public class DepthSensor : MonoBehaviour, ISensor
    {
        public float depth;
        
        private Transform sensor;

        void Start()
        {
            sensor = GetComponent<Transform>();
        }

        public void SampleSensor()
        {
            depth = sensor.position.y;
        }
    }
}
