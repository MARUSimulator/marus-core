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
using Std;
using UnityEngine;
using Labust.Utils;

namespace Labust.Sensors.Primitive
{
    public class DvlSensor : SensorBase
    {
        public bool debug = true;
        [NonSerialized] public Vector3 groundVelocity = new Vector3();
        [NonSerialized] public double[] velocityCovariance = new double[9];
        [NonSerialized] public float altitude;
        [NonSerialized] public double altitudeCovariance;

        [Header("Sensor parameters measurements")]
        [Header("Core measurements")]
        [ReadOnly] public Vector3 GroundVelocity;
        [ReadOnly] public float Altitude;

        [Header("Debug Beams")]
        [ReadOnly] public float[] beamRanges;
        [ReadOnly][SerializeField] RangeSensor[] beams;

        private Vector3 lastPosition;

        void Start()
        {
            beams = GetComponentsInChildren<RangeSensor>();
            beamRanges = new float[beams.Length];
            lastPosition = transform.position;
        }

        protected override void SampleSensor()
        {
            var position = transform.position;
            groundVelocity = transform.worldToLocalMatrix * ((position - lastPosition) / Time.fixedDeltaTime);

            lastPosition = position;

            altitude = Single.MaxValue;
            for (int i = 0; i < beams.Length; ++i)
            {
                beams[i].SampleSensor();

                if (beams[i].range < altitude)
                    altitude = beams[i].range;
                if (debug)
                {
                    beamRanges[i] = beams[i].range;
                }
            }
            if (debug)
            {
                Altitude = altitude;
                GroundVelocity = groundVelocity.Round(2);
            }
            hasData = true;
        }
    }
}
