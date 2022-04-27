using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Marus.Core;
using Marus.Networking;
using Marus.Sensors;
using Rfcommunication;
using UnityEngine;
using static Rfcommunication.LoraTransmission;

namespace Marus.Communications.Rf
{
    [RequireComponent(typeof(LoraRanging))]
    public class LoraRangingRos : SensorStreamer<LoraTransmissionClient, RangeingMsg>
    {
        LoraRanging Ranging;

        void Awake()
        {
            Ranging = GetComponent<LoraRanging>();
            if (string.IsNullOrEmpty(address))
            {
                address = $"loradevice_{Ranging.Master.DeviceId}_ranging";
            }
            StreamSensor(Ranging,
                streamingClient.StreamRangeingMsgs
            );
        }

        async protected override void SendMessage()
        {
            var msg = new RangeingMsg
            {
                Address = address,
                Header = new Std.Header { Timestamp = TimeHandler.Instance.TimeDouble},
                MasterId = (uint)Ranging.Master.DeviceId
            };
            msg.Ranges.AddRange(Ranging.Ranges
                .Where(x => !x.IsDropped)
                .Select(x =>
                new Range
                {
                    SourceId = (uint)x.SourceId,
                    TargetId = (uint)x.TargetId,
                    Range_ = x.Range,
                }));
            await _streamWriter?.WriteAsync(msg);
        }
    }
}

