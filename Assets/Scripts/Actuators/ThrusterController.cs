using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labust.Actuators
{
    public class ThrusterController : MonoBehaviour
    {

        public List<PwmThruster> thrusters = new List<PwmThruster>();

        public void ApplyPwm(float[] array)
        {
            for (int i = 0; i < array.Length; i++)
                thrusters[i].ApplyPwm(array[i]);
        }


    }
}

