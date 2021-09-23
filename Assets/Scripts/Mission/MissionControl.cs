using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using MissionWaypointNS;


public class MissionControl : MonoBehaviour
{
    /// <summary>
    /// Class for mission control.
    /// Enable waypoint object one-by-one as the game progresses.
    /// Show text for every waypoint.
    /// </summary>

    public List<MissionWaypoint> waypoints;

    // Messeges are publicly set and displayed in Text object.
    public Text textElement;
    public List<string> messages;

    // Waypoint can be hidden.
    public bool displayWaypoints = true;
    
    void Update()
    {
        for(int i = 0; i<= waypoints.Count; i++)
        {
            // Display final message after all the waypoints have been visited..
            if (i == waypoints.Count)
            {
                textElement.text = messages[i]; 
            }
            // Skip every waypoint that has already been visited.
            // Initially, "visited" property for all the waypoints is set to false.
            else if(waypoints[i].visited == true){
                continue;
            }
            // For the first waypoint that hasn't been visited display proper message
            // and show that waypoint.
            else if(waypoints[i].visited == false)
            {
                textElement.text = messages[i];
                waypoints[i].EnableWaypoint(displayWaypoints);
            }
        break;
        }
    }


}
