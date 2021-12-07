using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Labust.Mission
{
    public class MissionWaypoint : MonoBehaviour
    {
        /// <summary>
        /// Class that represents waypoint in the simulator.
        /// </summary>

        bool _visited;
        public bool visited => _visited;
        
        [System.NonSerialized]
        public MissionControl mission;


        void Start() 
        {
            /// <summary>
            /// Make sure objects isTrigger property is true because 
            /// onTriggerEnter function has to be used.
            /// </summary>

            GetComponent<Collider>().isTrigger = true;
            DisableWaypoint();
        }

        void OnTriggerEnter(Collider collider) 
        {
            /// <summary>
            /// If agent enters waypoint it is disabled.
            /// </summary>
            // Make sure other movable objects in the scene don't trigger a waypoint.

            if(collider.gameObject.GetInstanceID() == mission.agent.GetInstanceID()){
                _visited = true;
                DisableWaypoint();
            }
        }

        public void DisableWaypoint()
        {
            /// <summary>
            /// Disable waypoint object.
            /// </summary>

            gameObject.SetActive(false);
        }

        public void EnableWaypoint(bool dispayWaypoint)
        {
            /// <summary>
            /// Enable waypoint object.
            /// Visaulize waypoint if displayWaypoint argument is true.
            /// </summary>

            gameObject.SetActive(true);
            if (dispayWaypoint)
            {
                GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

}
