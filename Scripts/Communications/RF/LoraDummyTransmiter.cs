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
                    yield return new WaitForSeconds(1);
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
            _transmiter.Send(new LoraMessage
            {
                Message = $"THIS IS MESSAGE {msgCount}",
                Protocol = _transmiter.Protocol,
                TransmiterParams = _transmiter.GetTransmiterParams(),
                ReceiverId = Target.DeviceId,
                TransmitionType = TransmitionType.Unicast
            });
        }
    }
}
