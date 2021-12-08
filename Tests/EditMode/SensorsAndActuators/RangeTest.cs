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
using Labust.Sensors.Primitive;
using TestUtils;

public class RangeTest
{

    RangeSensor _rangeSensor;
    GameObject _target;

    [OneTimeSetUp]
    public void SetUp()
    {
        Utils.CreateEmptyScene();

        _rangeSensor = Utils.CreateAndInitializeObject<RangeSensor>("Range", PrimitiveType.Cube);
        _rangeSensor.maxRange = 120;
        _rangeSensor.transform.position = Vector3.zero;

        _target = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _target.transform.position = Vector3.up * 20;
    }

    [Test]
    public void TestRangeSample()
    {
        Utils.CallPhysicsUpdate();
        _rangeSensor.SampleSensor();
        Assert.AreEqual(19.5f, _rangeSensor.range);
    }
}
