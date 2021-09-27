using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using MissionWaypointNS;
using System;

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


    public event Action<MissionWaypoint> OnWaypointChange;

    void Update()
    {
        for(int i = 0; i<= waypoints.Count; i++)
        {
            // Display final message after all the waypoints have been visited..
            if (i == waypoints.Count)
            {
                if (TextBoxAndMessageExist(i))
                {
                    textElement.text = messages[i]; 
                }
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
                if (TextBoxAndMessageExist(i))
                {
                    textElement.text = messages[i]; 
                }
                waypoints[i].EnableWaypoint(displayWaypoints);
                if(OnWaypointChange != null)
                {
                    OnWaypointChange.Invoke(waypoints[i]);
                }
            }
        break;
        }
    }

    private bool TextBoxAndMessageExist(int i)
    {
        return textElement != null && !string.IsNullOrEmpty(messages[i]);
    }
}
