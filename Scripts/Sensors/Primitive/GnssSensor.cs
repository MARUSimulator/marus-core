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

using Marus.Core;
using Marus.Networking;
using Marus.NoiseDistributions;
using Std;
using UnityEngine;

namespace Marus.Sensors.Primitive
{
    public class GnssSensor : SensorBase
    {
        
        public NoiseParameters measurementNoise;

        [Header("Precision")]
        public bool isRTK = true;
        public float maximumOperatingDepth = 0.5f;

        [Header("Position info")]
        
        [ReadOnly]
        public GeoPoint point;
        [ReadOnly]
        public GeoPoint origin;

        protected override void SampleSensor()
        {
            Vector3 pos = transform.position;
            pos.x += Noise.Sample(measurementNoise);
            pos.y += Noise.Sample(measurementNoise);
            pos.z += Noise.Sample(measurementNoise);

            var world = TfHandler.Instance.OriginGeoFrame;
            point = world.Unity2Geo(pos);
            origin = world.origin;

            Log(new { point.latitude, point.longitude, point.altitude });
            hasData = true;
        }
    }
}