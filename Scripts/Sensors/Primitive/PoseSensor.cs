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
using UnityEngine;
using Labust.Utils;
using Std;

namespace Labust.Sensors.Primitive
{
    /// <summary>
    /// Pose sensor implementation
    /// </summary>
    public class PoseSensor : SensorBase
    {
        public bool debug = true;
        [NonSerialized] public Vector3 position;
        [NonSerialized] public Quaternion orientation;
        [NonSerialized] public float verticalOffset;
        [NonSerialized] public Vector3 linearVelocity;
        [NonSerialized] public Vector3 angularVelocity;

        [Header("Pose")]
        [ReadOnly] public Vector3 Position;
        [ReadOnly] public Quaternion Orientation;
        [ReadOnly] public float VerticalOffset;
        [Header("Twist")]
        [ReadOnly] public Vector3 LinearVelocity;
        [ReadOnly] public Vector3 AngularVelocity;

        public Rigidbody measuredObject;
      
        void Start()
        {
            measuredObject = Helpers.GetParentRigidBody(transform);
        }

        protected override void SampleSensor()
        {
            position = measuredObject.position;
            orientation = measuredObject.rotation;
            linearVelocity = measuredObject.transform.InverseTransformVector(measuredObject.velocity);
            angularVelocity = measuredObject.angularVelocity;
            if (debug)
            {
                Position = position.Round(2);
                Orientation = orientation.Round(2);
                LinearVelocity = linearVelocity.Round(2);
                AngularVelocity = angularVelocity.Round(2);
            }
            Log(new { position, orientation });
            hasData = true;
        }
    }
}
