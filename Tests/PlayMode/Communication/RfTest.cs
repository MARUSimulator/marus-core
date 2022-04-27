// Copyright 2021 Laboratory for Underwater Systems and Technologies (LABUST)
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

using NUnit.Framework;
using Marus.Communications.Rf;
using TestUtils;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

public class RfTest
{
    LoraDevice _lora1;
    LoraDevice _lora2;
    LoraDevice _lora3;
    LoraDevice _lora4;
    LoraRanging _loraRanging;

    [OneTimeSetUp]
    public void SetUp()
    {
        _lora1 = Utils.CreateAndInitializeObject<LoraDevice>("lora1", PrimitiveType.Cube);
        _lora1.Range = 600;
        _lora1.ChangeId(1);


        _lora2 = Utils.CreateAndInitializeObject<LoraDevice>("lora2", PrimitiveType.Cube);
        _lora2.transform.position = new Vector3(1, 0 , 0);
        _lora2.Range = 600;
        _lora2.ChangeId(2);


        _lora3 = Utils.CreateAndInitializeObject<LoraDevice>("lora3", PrimitiveType.Cube);
        _lora2.transform.position = new Vector3(-1, 0 , 0);
        _lora3.Range = 600;
        _lora3.ChangeId(3);

        _lora4 = Utils.CreateAndInitializeObject<LoraDevice>("lora4", PrimitiveType.Cube);
        _lora2.transform.position = new Vector3(0, 1 , 0);
        _lora4.Range = 600;
        _lora4.ChangeId(4);


        var initWith = new Dictionary<string, object>
        {
            { "PacketDropRate", 0},
            { "Master", _lora1 },
            { "Targets", new List<LoraDevice> { _lora4 }}
        };
        _loraRanging = Utils.CreateObject<LoraRanging>("loraranging", PrimitiveType.Cube, initWith);

        _lora1.transform.parent = _loraRanging.transform;
        // advanced ranging loras
        _lora2.transform.parent = _loraRanging.transform;
        _lora3.transform.parent = _loraRanging.transform;
        Utils.InitializeScript(_loraRanging);
    }

    [UnityTest]
    public IEnumerator TestLoraTransmit()
    {
        // PASS arr BY REFERENCE TO THE METHOD
        bool msgReceived = false;
        LoraMessage mess = null;
        int deviceId = -1;
        Action<LoraDevice, LoraMessage> onReceive = (device, msg) => 
        {
            msgReceived = true;
            deviceId = device.DeviceId;
            mess = msg;
        };
        _lora2.OnReceiveEvent += onReceive;
        _lora1.Send(
            new LoraMessage()
                {
                    Message = "TESTMSG",
                    ReceiverId = 2,
                    TransmitionType = TransmitionType.Unicast
                }
        );
        // STEP
        yield return null;
        Assert.AreEqual(true, msgReceived, "Message should be received");
        Assert.AreEqual(_lora2.DeviceId, deviceId, "Lora with ID=2 should receive message");
        Assert.AreEqual("TESTMSG", mess?.Message, "Message should be 'TESTMSG'");
        _lora2.OnReceiveEvent -= onReceive;
    }


    [UnityTest]
    public IEnumerator TestLoraBroadcast()
    {
        int[] arr = new int[3];
        Action<LoraDevice, LoraMessage> onReceive = (device, msg) => 
        {
            arr[device.DeviceId-2] = 1;
        };
        _lora2.OnReceiveEvent += onReceive;
        _lora3.OnReceiveEvent += onReceive;
        _lora4.OnReceiveEvent += onReceive;
        _lora1.Send(
            new LoraMessage()
                {
                    Message = "TESTMSG",
                    TransmitionType = TransmitionType.Broadcast
                }
        );
        // STEP
        yield return null;
        Assert.AreEqual(1, arr[0], $"Lora device with id {2} should have received the message");
        Assert.AreEqual(1, arr[1], $"Lora device with id {3} should have received the message");
        Assert.AreEqual(1, arr[2], $"Lora device with id {4} should have received the message");

    }

    [UnityTest]
    public IEnumerator TestLoraRanging()
    {
        Utils.CallNonpublicMethod(_loraRanging, "SampleSensor");
        yield return null;
        Assert.AreEqual(3, _loraRanging.Ranges.Count, "There should be 3 lora ranging messages");

        var masterRange = _loraRanging.Ranges[0];
        Assert.AreEqual(_lora1.DeviceId, masterRange.SourceId, $"First range should be from first lora");
        Assert.AreEqual(_lora4.DeviceId, masterRange.TargetId, $"First range should be to fourth lora");

    }
}
