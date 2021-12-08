using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Labust.Sensors.Primitive
{
    public class Nanomodem : MonoBehaviour
    {
        public float maxRange = 500;

        [HideInInspector]
        public AcousticMedium medium;

        public void Start()
        {
            AcousticMedium.Instance.RegisterNanomodem(this);
        }


        public void ReceiveMsg(int request, Nanomodem sender)
        {

        }
        
        public void TranmitMsg(int request)
        {

        }

    }
}