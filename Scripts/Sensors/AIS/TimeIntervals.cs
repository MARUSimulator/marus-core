// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Marus.Sensors.AIS
{

    /// <summary>
    /// Get reporting time intervals based on AIS Class, speed and course status.
    /// </summary>
    public static class TimeIntervals
    {
        public static float GetInterval(AISClassType type, float speed, Boolean changingCourse = false)
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
