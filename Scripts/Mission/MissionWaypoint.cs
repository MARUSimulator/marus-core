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

namespace Marus.Mission
{
    public class MissionWaypoint : MonoBehaviour
    {
        /// <summary>
        /// Class that represents waypoint in the simulator.
        /// </summary>

        bool _visited = false;
        public bool Visited => _visited;

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
            /// If player enters waypoint it is disabled.
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
