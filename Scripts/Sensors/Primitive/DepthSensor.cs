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

namespace Marus.Sensors.Primitive
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
