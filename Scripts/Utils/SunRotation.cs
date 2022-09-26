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
    using System.Collections;
     
    public class SunRotation : MonoBehaviour
    {
     
        [HideInInspector]
        public GameObject sun;
        [HideInInspector]
        public Light sunLight;
     
        [Range(0, 24)]
        public float timeOfDay = 12;
     
        public float secondsPerMinute = 60;
        [HideInInspector]
        public float secondsPerHour;
        [HideInInspector]
        public float secondsPerDay;
     
        public float timeMultiplier = 1;
     
        void Start()
        {
            sun = gameObject;
            sunLight = gameObject.GetComponent<Light>();
     
            //secondsPerHour = secondsPerMinute * 60;
            //secondsPerDay = secondsPerHour * 24;
        }
     
        // Update is called once per frame
        void Update()
        {
            SunUpdate();
        }
     
        public void SunUpdate()
        {
            //30,-30,0 = sunrise
            //90,-30,0 = High noon
            //180,-30,0 = sunset
            //-90,-30,0 = Midnight
     
            sun.transform.rotation = Quaternion.Euler((timeOfDay - 6.3f / 24) * 360, -30, 0);
            //sun.transform.eulerAngles = new Vector3(-3 + Time.time * timeMultiplier, 2.2f);
        }
    }
