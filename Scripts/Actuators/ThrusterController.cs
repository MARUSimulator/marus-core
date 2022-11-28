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

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace Marus.Actuators
{
    public class ThrusterController : MonoBehaviour
    {

        public List<Thruster> thrusters = new List<Thruster>();

        public void ApplyPwm(float[] array)
        {
            for (int i = 0; i < thrusters.Count; i++)
            {
                if (i < array.Length)
                {
                    thrusters[i].ApplyPwm(array[i]);
                }
                else
                {
                    break;
                }
            }
        }

        public void ApplyPwm(Vector3 tau, Matrix<double> inverseAllocationMatrix)
        {
            var vec = CreateVector.Dense<double>(3);
            vec[0] = tau.x;
            vec[1] = tau.y;
            vec[2] = tau.z;
            var forces = inverseAllocationMatrix.Multiply(vec);

            for (int i = 0; i < thrusters.Count; i++)
            {
                if (i < forces.Count)
                {
                    var pwm = thrusters[i].GetPwmForForce((float)forces[i]);
                    thrusters[i].ApplyPwm(pwm);
                }
                else
                {
                    break;
                }
            }

        }
    }
}

