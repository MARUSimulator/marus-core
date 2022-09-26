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

//using UnityEngine;
//using System.Collections.Generic;

//namespace Marus.Controllers
//{
//    /// <summary>
//    /// This script rotates object around target object.
//    ///
//    /// DO NOT USE
//    /// </summary>
//    public class FishController : MonoBehaviour
//    {
//        public List<GameObject> Targets;
//		public float Speed = 5f;

//        private VesselVelocityController vesselVelocityController;

//        private System.Random random;

//        void Start() {
//            random = new System.Random();
//            vesselVelocityController = gameObject.AddComponent(typeof(VesselVelocityController)) as VesselVelocityController;
//            var target = Targets[random.Next(0, Targets.Count)];
//            vesselVelocityController.Target = target.transform;
//            vesselVelocityController.Use3DTarget = true;
//            vesselVelocityController.StoppingDistance = 5f;
//            vesselVelocityController.Speed = UnityEngine.Random.Range(0.1f, 0.5f);
//        }

//		void Update()
//		{
//            if (vesselVelocityController.stop)
//            {
//                var target = Targets[random.Next(0, Targets.Count)];
//                vesselVelocityController.Target = target.transform;
//            }
//		}
//    }
//}
