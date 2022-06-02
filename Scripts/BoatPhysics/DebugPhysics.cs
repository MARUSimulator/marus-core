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

//To be able to change the different physics parameters real time
namespace Marus
{
    public class DebugPhysics : MonoBehaviour 
    {
        public static DebugPhysics current;

        //Force 2 - Pressure Drag Force
        [Header("Force 2 - Pressure Drag Force")]
        public float velocityReference;

        [Header("Pressure Drag")]
        public float C_PD1 = 10f;
        public float C_PD2 = 10f;
        public float f_P = 0.5f;

        [Header("Suction Drag")]
        public float C_SD1 = 10f;
        public float C_SD2 = 10f;
        public float f_S = 0.5f;

        //Force 3 - Slamming Force
        [Header("Force 3 - Slamming Force")]
        //Power used to ramp up slamming force
        public float p = 2f;
        public float acc_max;
        public float slammingCheat;

        void Start() 
        {
            current = this;
        }
    }
}