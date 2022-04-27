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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Marus.Sensors;
using UnityEngine;


namespace Marus.Communications.Rf
{
    public class LoraRanging : SensorBase
    {
        public LoraDevice Master;
        public List<LoraDevice> Targets;
        List<LoraDevice> _advancedNodes;
        bool advancedRanging;

        public float PacketDropRate = 0;

        [ReadOnly] public List<RangeReading> Ranges;

        // Start is called before the first frame update
        new void Awake()
        {
            base.Awake();
            if (Master == null)
            {
                Debug.Log("Ranging error. The  master for ranging is not set.");
                this.enabled = false;
                return;
            }
            if (Targets == null)
            {
                Debug.Log("Ranging error. No ranging target is set.");
                this.enabled = false;
                return;
            }
            if (PacketDropRate < 0 || PacketDropRate > 1)
            {
                Debug.Log("Ranging error. Packet drop rate must be between 0 and 1");
                this.enabled = false;
                return;
            }
            Ranges = new List<RangeReading>();
            _advancedNodes = GetComponentsInChildren<LoraDevice>(false).ToList();
            _advancedNodes.RemoveAll(x => x.GetInstanceID() == Master.GetInstanceID());
            advancedRanging = _advancedNodes.Count > 0;
        }


        protected override void SampleSensor()
        {
            Ranges.Clear();
            // master - target ranging
            for (var i = 0; i < Targets.Count; i++)
            {
                var reading = GetRangeReading(Master, Targets[i]);
                Ranges.Add(reading);
            }
            // for each message that got from master to target, do
            // advanced ranging
            if (advancedRanging)
            {
                int count = Ranges.Count;
                for (var i = 0; i < count; i++)
                {
                    var reading = Ranges[i];
                    foreach (var adv in _advancedNodes)
                    {
                        var target = Targets.First(x => x.DeviceId == reading.TargetId);
                        var advReading = GetRangeReading(adv, target);
                        advReading.SourceId = adv.DeviceId;
                        advReading.TargetId = target.DeviceId;
                        Ranges.Add(advReading);
                    }
                }
            }
            
        }

        private RangeReading GetRangeReading(RfDevice source, RfDevice target)
        {
            var reading = new RangeReading
            {
                SourceId = source.DeviceId,
                TargetId = target.DeviceId,
            };

            var d = Random.Range(0f, 1f);
            if (d < PacketDropRate)
            {
                reading.IsDropped = true;
            }
            else
            {
                var r = RfMediumHelper.Range(source, target);
                reading.Range = r;
                reading.IsDropped = false;
            }
            return reading;
        }
    }
}

