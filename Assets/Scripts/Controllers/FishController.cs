using UnityEngine;
using System;
using System.Collections.Generic;

namespace Labust.Controllers
{
    /// <summary>
    /// This script rotates object around target object.
    /// </summary>
    public class FishController : MonoBehaviour
    {
        public List<GameObject> Targets;
		public float Speed = 5f;

        private VesselVelocityController vesselVelocityController;

        private System.Random random;

        void Start() {
            random = new System.Random();
            vesselVelocityController = gameObject.AddComponent(typeof(VesselVelocityController)) as VesselVelocityController;
            var target = Targets[random.Next(0, Targets.Count)];
            vesselVelocityController.Target = target.transform;
            vesselVelocityController.Use3DTarget = true;
            vesselVelocityController.StoppingDistance = 5f;
            vesselVelocityController.Speed = UnityEngine.Random.Range(0.1f, 0.5f);
        }

		void Update()
		{
            if (vesselVelocityController.stop)
            {
                var target = Targets[random.Next(0, Targets.Count)];
                vesselVelocityController.Target = target.transform;
            }
		}
    }
}
