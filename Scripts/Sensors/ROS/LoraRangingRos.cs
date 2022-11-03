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
        LoraRanging sensor;

        new void Start()
        {
            sensor = GetComponent<LoraRanging>();
            StreamSensor(sensor,
                streamingClient.StreamRangeingMsgs
            );
            if (string.IsNullOrEmpty(address))
            {
                address = $"loradevice_{sensor.Master.DeviceId}_ranging";
            }
            base.Start();
        }

        protected override RangeingMsg ComposeMessage()
        {
            var msg = new RangeingMsg
            {
                Address = address,
                Header = new Std.Header { Timestamp = TimeHandler.Instance.TimeDouble},
                MasterId = (uint)sensor.Master.DeviceId
            };
            msg.Ranges.AddRange(sensor.Ranges
                .Where(x => !x.IsDropped)
                .Select(x =>
                new Range
                {
                    SourceId = (uint)x.SourceId,
                    TargetId = (uint)x.TargetId,
                    Range_ = x.Range,
                }));
            return msg;
            // await _streamWriter?.WriteAsync(msg);
        }
    }
}

