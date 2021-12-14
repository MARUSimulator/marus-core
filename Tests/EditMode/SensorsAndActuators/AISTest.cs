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
using Labust.Sensors;
using Labust.Sensors.Primitive;
using Labust.Sensors.AIS;
using TestUtils;

public class AISTest
{

    AisSensor _ais;
    GnssSensor _gnss;
    AisDevice _device;
    Rigidbody _rigidbody;

    [OneTimeSetUp]
    public void SetUp()
    {
        _ais = Utils.CreateAndInitializeObject<AisSensor>("AIS", PrimitiveType.Cube);
        _ais.SampleFrequency = 100;
        _ais.transform.position = Vector3.zero;
        _rigidbody = _ais.GetComponent<Rigidbody>();
        _rigidbody.velocity = Vector3.zero;
    }

    [Test]
    public void TestAisSample()
    {
        Utils.CallFixedUpdate(SensorSampler.Instance);
        Assert.AreEqual(0, _ais.SOG);
        Assert.AreEqual(0, _ais.COG);
        Assert.AreEqual(0, _ais.TrueHeading);

        _ais.transform.eulerAngles = new Vector3(0, 90, 0);
        _ais.transform.position = new Vector3(-1, 0, 0);
        _rigidbody.velocity = new Vector3(-3f, 0, 0);
        Utils.CallFixedUpdate(SensorSampler.Instance);
        Assert.AreEqual(58, _ais.SOG);
        Assert.AreEqual(2700, _ais.COG);
        Assert.AreEqual(90, _ais.TrueHeading);
    }
}
