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

// Modified code from https://www.habrador.com/tutorials/unity-boat-tutorial/5-resistance-forces/

using UnityEngine;

//Data that belongs to one triangle in the original boat mesh
//and is needed to calculate the slamming force
namespace Marus
{
    public class SlammingForceData 
    {
        //The area of the original triangles - calculate once in the beginning because always the same
        public float originalArea;
        //How much area of a triangle in the whole boat is submerged
        public float submergedArea;
        //Same as above but previous time step
        public float previousSubmergedArea;
        //Need to save the center of the triangle to calculate the velocity
        public Vector3 triangleCenter;
        //Velocity
        public Vector3 velocity;
        //Same as above but previous time step
        public Vector3 previousVelocity;
    }
}