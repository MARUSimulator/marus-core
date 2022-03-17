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

using UnityEngine;

namespace Labust.Sensors.Primitive
{
    public class SonarPrimitive : SensorBase
    {
        public float maxRange = 120f;
        public float minBearing = -60f; // 60 degrees to the left
        public float maxBearing = 60f; // 60 degrees to the right
        public Transform trackedObject;

        float range;
        float bearing;

        new void Awake()
        {
            if (trackedObject == null)
            {
                Debug.Log($"trackedObject not set in SonarPrimitive script {name}");
            }
            base.Awake();
        }


        protected override void SampleSensor()
        {
            if (trackedObject == null) return;

            var sonar = transform;
            if (Vector3.Distance(sonar.position, trackedObject.position) > 0 &&
                Vector3.Distance(sonar.position, trackedObject.position) < maxRange)
            {
                range = Vector3.Distance(sonar.position, trackedObject.position);
                Debug.DrawRay(sonar.position, transform.TransformDirection(Vector3.up) * range,
                    Color.yellow);
            }
            else
            {
                range = 0;
            }
            Log(new { range, bearing });

            // float xDiff = trackedObject.position.x - sonar.position.x;
            // float zDiff = trackedObject.position.z - sonar.position.z;

            // float angle = (float)Math.Atan2(xDiff, zDiff) * (float)(180/Math.PI);

            // float sonarAngle = sonar.eulerAngles.y;

            // if (sonarAngle>180)
            // {   
            //     sonarAngle -= 360;

            // }

            // bearing = angle - sonarAngle;
            // Debug.Log(sonarAngle);
            // Debug.Log(angle);


            Vector3 targetHeading = new Vector3(trackedObject.position.x, 0, trackedObject.position.z) - 
                                    new Vector3(sonar.position.x, 0, sonar.position.z);
            bearing = -Vector3.SignedAngle(targetHeading, sonar.forward, sonar.up);

            if (bearing > maxBearing || bearing < minBearing || range == 0)
            {
                bearing = 0;
                range = 0;
            }

            hasData = true;
        }
    }
}
