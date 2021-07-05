using System;
using UnityEngine;
using Unity;
namespace Labust.Sensors.AIS
{

    /// <summary>
    /// Get reporting time intervals based on AIS Class, speed and course status.
    /// </summary>
    public class TimeIntervals
    {
        public static float getInterval(AISClassType type, float speed, Boolean changingCourse = false)
        {
            if (type == AISClassType.ClassA)
            {
                if (speed < 140f)
                {
                    if (changingCourse) return 3.333f;
                    return 10f;
                }
                if (speed < 230f)
                {
                    if (changingCourse) return 2f;
                    return 6f;
                }
                return 2f;
            }
            if (type == AISClassType.ClassB)
            {
                if (speed < 20f) return 180f;
                if (speed < 140f) return 30f;
                if (speed < 230f) return 15f;
                if (speed > 230f) return 5f;
            }
            return 10f;
        }
    }
}
