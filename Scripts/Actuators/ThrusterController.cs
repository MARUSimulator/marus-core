using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace Labust.Actuators
{
    public class ThrusterController : MonoBehaviour
    {

        public List<PwmThruster> thrusters = new List<PwmThruster>();

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

