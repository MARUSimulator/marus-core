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

using System;
using System.Collections;
using System.Collections.Generic;
using Labust.Networking;
using UnityEngine;

namespace Labust.Controllers
{
    /// <summary>
    /// Vessel controller that directly controls velocity and orientation to move and rotate towards the Target position
    /// </summary>
    public class VesselVelocityController : MonoBehaviour
    {
        public Transform Target;
        public Boolean stop;
        public float Speed = 0.5f;
        public float RotationSpeed = 1.0f;
        public float StoppingDistance = 1f;
        public float RestartDistance = 5f;
        public bool Use3DTarget = false;

        public void Awake()
        {
            stop = false;
        }

        public void FixedUpdate()
        { 
            if (Target == null)
            {
                return;
            }
            RotateTowards();
            MoveTowards();
        }

        public void MoveTowards()
        {
            float dist = Vector3.Distance(new Vector3(Target.position.x, 0, Target.position.z), new Vector3(transform.position.x, 0, transform.position.z));

            //Stop the vessel when close enough to target
            if (dist < StoppingDistance)
            {
                stop = true;
            }

            //start the vessel when target changed
            if (stop && dist > RestartDistance)
            {
                stop = false;
            }

            if (!stop)
            {
                //use distance to slow down when approaching target position
                transform.position += transform.forward * Time.deltaTime * Mathf.Sqrt(dist) * Speed;
            }
        }

        public void RotateTowards()
        {
            Vector3 relativePos = Target.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(relativePos, Vector3.up);
            Vector3 targetAngles = targetRotation.eulerAngles;
            Quaternion target;
            if (!Use3DTarget)
            {
                target = Quaternion.Euler(0, targetAngles.y, 0);
            }
            else
            {
                target = Quaternion.Euler(targetAngles.x, targetAngles.y, 0);
            }

            float error = Vector3.Angle(relativePos, transform.forward);

            if (!stop)
            {
                //use error in interpolation ratio to minimize rotation when approaching optimal course
                transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, target, Time.deltaTime * RotationSpeed);
            }
        }
    }
}
