using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Labust.Mission {
    public class MissionControl : MonoBehaviour
    {
        /// <summary>
        /// Class for mission control.
        /// Enable waypoint object one-by-one as the game progresses.
        /// Show text for every waypoint.
        /// </summary>

        public GameObject agent;
        public List<MissionWaypoint> waypointObjects = new List<MissionWaypoint>();

        // Messeges are publicly set and displayed in Text object.
        public Text textElement;
        public List<string> messages;

        // Waypoints can be hidden.
        public bool displayWaypoints = true;

        [System.NonSerialized]
        public bool MissionComplete = false;

        public event Action<MissionWaypoint> OnWaypointChange;

        int _eventCount;

        void Start() 
        {
            _eventCount = -1;
            //Set mission parameter for every waypoint
            foreach (MissionWaypoint wp in waypointObjects){
                wp.mission = this;
            }     
        }

        void Update()
        {
            if (MissionComplete )
            {
                return;
            }

            for(int i = 0; i<= waypointObjects.Count; i++)
            {
                // Display final message after all the waypoints have been visited.
                if (i == waypointObjects.Count && TextBoxAndMessageExist(i))
                {
                    textElement.text = messages[i];
                    MissionComplete = true;
                    return;
                }
                var waypoint = waypointObjects[i].GetComponent<MissionWaypoint>();
                // Skip every waypoint that has already been visited.
                // Initially, "visited" property for all the waypoints is set to false.
                if(waypoint.visited == true){
                    continue;
                }
                // For the first waypoint that hasn't been visited display proper message
                // and show that waypoint.
                else if(waypoint.visited == false)
                {
                    if (TextBoxAndMessageExist(i))
                    {
                        textElement.text = messages[i]; 
                    }

                    if(_eventCount != i)
                    {
                        waypoint.EnableWaypoint(displayWaypoints);
                        OnWaypointChange?.Invoke(waypoint);
                        _eventCount = i;
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
