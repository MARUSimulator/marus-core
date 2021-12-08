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
using TestUtils;

public class DvlTest
{

    DvlSensor _dvlSensor;
    Rigidbody _rigidbody;
    RangeSensor _beam;
    GameObject _seaFloor;

    [OneTimeSetUp]
    public void SetUp()
    {
        //Utils.CreateEmptyScene();
        _dvlSensor = Utils.CreateAndInitializeObject<DvlSensor>("Depth", PrimitiveType.Cube);
        _dvlSensor.SampleFrequency = 100;
        _dvlSensor.transform.position = Vector3.zero;

        _beam = Utils.CreateAndInitializeObject<RangeSensor>("beam", PrimitiveType.Cube);
        _beam.transform.SetParent(_dvlSensor.transform);
        _beam.transform.eulerAngles = new Vector3(180, 0, 0);

        _seaFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _seaFloor.transform.position = new Vector3(0, -20, 2);
        Utils.CallStart<DvlSensor>(_dvlSensor);
    }

    [Test]
    public void TestDvlSample()
    {
        Utils.CallFixedUpdate(SensorSampler.Instance);
        Utils.CallPhysicsUpdate();
        Assert.AreEqual(0f, _dvlSensor.groundVelocity.z);
        _dvlSensor.transform.position = new Vector3(0, 0, 2);
        Utils.CallFixedUpdate(SensorSampler.Instance);
        Assert.AreEqual(100, _dvlSensor.groundVelocity.z);
        Assert.AreEqual(19.5f, _dvlSensor.altitude);
    }
}
