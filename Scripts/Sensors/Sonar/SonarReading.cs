using UnityEngine;

namespace Labust.Sensors
{
    public struct SonarReading
    {
        public bool Valid;
        public float Intensity;
        public float Distance;
        //public NativeArray<Vector3> Debug;
    }
}