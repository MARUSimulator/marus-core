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
using Marus.Sensors;
using Marus.Sensors.Primitive;
using Marus.Sensors.AIS;
using TestUtils;

public class AISTest
{

    AisSensor _ais;
    AisSensor _ais2;

    AisDevice _aisDevice;
    AisDevice _aisDevice2;
    GnssSensor _gnss;
    AisDevice _device;
    Rigidbody _rigidbody;

    [OneTimeSetUp]
    public void SetUp()
    {
        _ais = Utils.CreateAndInitializeObject<AisSensor>("AIS1", PrimitiveType.Cube);
        _ais.SampleFrequency = 100;
        _aisDevice = _ais.gameObject.AddComponent<AisDevice>();
        _aisDevice.Range = 200;
        _aisDevice.MMSI = "1111111111";
        AisManager.Instance.Register(_aisDevice);
        _ais.transform.position = Vector3.zero;
        _rigidbody = _ais.GetComponent<Rigidbody>();
        _rigidbody.velocity = Vector3.zero;

        _ais2 = Utils.CreateAndInitializeObject<AisSensor>("AIS2", PrimitiveType.Cube);
        _aisDevice2 = _ais2.gameObject.AddComponent<AisDevice>();
        _aisDevice2.MMSI = "22222222";
        AisManager.Instance.Register(_aisDevice2);
        _ais2.SampleFrequency = 100;
        _ais2.transform.position = new Vector3(100, 0, 0);
    }

    [Test]
    public void TestAisSample()
    {
        Utils.CallNonpublicMethod(_ais, "SampleSensor");
        Assert.AreEqual(0, _ais.SOG);
        Assert.AreEqual(0, _ais.COG);
        Assert.AreEqual(0, _ais.TrueHeading);

        _ais.transform.eulerAngles = new Vector3(0, 90, 0);
        _ais.transform.position = new Vector3(-1, 0, 0);
        _rigidbody.velocity = new Vector3(-3f, 0, 0);
        Utils.CallNonpublicMethod(_ais, "SampleSensor");
        Assert.AreEqual(58, _ais.SOG);
        Assert.AreEqual(2700, _ais.COG);
        Assert.AreEqual(90, _ais.TrueHeading);
    }

    [Test]
    public void TestAisMessage()
    {
        var received = false;
        var msg = new PositionReportClassA()
        {
            MMSI = _aisDevice.MMSI,
            sender = _aisDevice
        };
        _aisDevice2.OnReceiveEvent += (msg) => { received = true; };

        AisManager.Instance.Broadcast(msg);
        Assert.AreEqual(true, received);

        received = false;
        _aisDevice.Range = 50;
        AisManager.Instance.Broadcast(msg);
        Assert.AreEqual(false, received);
    }
}
