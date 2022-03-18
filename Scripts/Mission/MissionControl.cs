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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Marus.Mission 
{
    public class MissionControl : MonoBehaviour
    {
        /// <summary>
        /// Class for mission control.
        /// Enable waypoint object one-by-one as the game progresses.
        /// Show text for every waypoint.
        /// </summary>

        public GameObject agent;
        public List<MissionWaypoint> waypointObjects;

        // Messeges are publicly set and displayed in Text object.
        public Text textElement;
        public List<string> messages;

        // Waypoints can be hidden.
        public bool displayWaypoints = true;

        [System.NonSerialized]
        public bool MissionComplete = false;

        public event Action<MissionWaypoint> OnWaypointChange;

        // Count how many events have been triggered.
        // Counts until eventCount == waypointsObjects.Count
        int eventCount;
        void Start() 
        {
            eventCount = -1;
            //Set mission parameter for every waypoint
            foreach (var wp in waypointObjects)
            {
                wp.mission = this;
            }     
        }

        void Update()
        {
            if (MissionComplete)
            {
                return;
            }

            for (int i = 0; i<= waypointObjects.Count; i++)
            {
                // Display final message after all the waypoints have been visited.
                if (i == waypointObjects.Count && TextBoxAndMessageExist(i))
                {
                    textElement.text = messages[i];
                    MissionComplete = true;
                    return;
                }
                MissionWaypoint waypoint = waypointObjects[i].GetComponent<MissionWaypoint>();
                // Skip every waypoint that has already been visited.
                // Initially, "visited" property for all the waypoints is set to false.
                if(waypoint.Visited == true){
                    continue;
                }
                // For the first waypoint that hasn't been visited display proper message
                // and show that waypoint.
                else if(waypoint.Visited == false)
                {
                    if (TextBoxAndMessageExist(i))
                    {
                        textElement.text = messages[i]; 
                    }
                    if (eventCount != i)
                    {
                        waypoint.EnableWaypoint(displayWaypoints);
                        OnWaypointChange?.Invoke(waypoint);
                        eventCount = i;
                    }
                    break;
                }
            
            }
        }


        private bool TextBoxAndMessageExist(int i)
        {
            return textElement != null && !string.IsNullOrEmpty(messages[i]);
        }
    }

}
