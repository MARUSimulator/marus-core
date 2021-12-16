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
using UnityEngine;
using Labust.Sensors.Acoustics;
using TestUtils;
using UnityEngine.TestTools;
using System.Collections;
public class AcousticTest
{
  Nanomodem _nanomodem1;
  Nanomodem _nanomodem2;
  AcousticMedium _medium;

    [OneTimeSetUp]
    public void SetUp()
    {
        _medium = Utils.CreateAndInitializeObject<AcousticMedium>("medium");
        var boxVolume = _medium.gameObject.GetComponent<BoxVolume>();
        boxVolume.Type = BoxVolume.BoxType.World;

        _nanomodem1 = Utils.CreateAndInitializeObject<Nanomodem>("nanomodem1", PrimitiveType.Cube);
        _nanomodem1.Range = 600;
        _nanomodem1.ChangeId(1);

        _nanomodem2 = Utils.CreateAndInitializeObject<Nanomodem>("nanomodem2", PrimitiveType.Cube);
        _nanomodem2.transform.position = new Vector3(100, 0, 0);
        _nanomodem2.ChangeId(2);
    }

    [Test]
    public void TestNanomodemVoltage()
    {
        Assert.IsTrue(_nanomodem1.SupplyVoltage >= 3 && _nanomodem1.SupplyVoltage <= 6.5f);
    }

    [UnityTest]
    public IEnumerator TestSendMessage()
    {
        NanomodemMessage msg = new NanomodemMessage()
        {
            Message = "$P002",
            ReceiverId = 2,
            TransmitionType = TransmitionType.Unicast
        };
        var ack = false;
        _nanomodem2.OnReceiveEvent += (msg) => ack = true;

        _nanomodem1.Send(msg);
        for (int i = 0; i < 200; i++)
        {
            yield return null;
        }
        Assert.AreEqual(true, ack);

        // test when not in range
        ack = false;
        _nanomodem2.transform.position = new Vector3(700, 0, 0);
        _nanomodem1.Send(msg);
        for (int i = 0; i < 200; i++)
        {
            yield return null;
        }
        Assert.AreEqual(false, ack);
    }
}
