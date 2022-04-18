using System;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity;
using Marus.Utils;
using Marus.Logger;
using System.Collections;

namespace Marus.Communications.Rf
{
    [RequireComponent(typeof(LoraDevice))]
    public class LoraDummyTransmiter : MonoBehaviour
    {

        LoraDevice _transmiter;
        int msgCount;

        public float PublishEverySeconds = 1;

        public LoraDevice Target;

        public void Start()
        {
            _transmiter = GetComponent<LoraDevice>();
            if (Target == null)
            {
                enabled = false;
                return;
            }
            StartCoroutine(PublishEvery());
        }

        public IEnumerator PublishEvery()
        {
            while (true)
            {
                if (Target == null)
                {
                    yield return new WaitForSeconds(PublishEverySeconds);
                }
                msgCount++;
                PublishMsg();
                if (PublishEverySeconds >= 0)
                {
                    yield return new WaitForSeconds(PublishEverySeconds);
                }
            }
        }

        private void PublishMsg()
        {
            var msg = new LoraMessage
            {
                Message = $"THIS IS MESSAGE {msgCount}",
                Protocol = _transmiter.Protocol,
                TransmiterParams = _transmiter.GetTransmiterParams()
            };
            RfMediumHelper.Transmit(msg, Target);
        }
    }
}
