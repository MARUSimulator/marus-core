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
using TestUtils;

public class DepthTest
{

    DepthSensor _depthSensor;

    [OneTimeSetUp]
    public void SetUp()
    {
        _depthSensor = Utils.CreateAndInitializeObject<DepthSensor>("Depth", PrimitiveType.Cube);
        _depthSensor.SampleFrequency = 100;
        _depthSensor.transform.position = Vector3.zero;
    }

    [Test]
    public void TestDepthSample()
    {
        Utils.CallNonpublicMethod(_depthSensor, "SampleSensor");
        Assert.AreEqual(0, _depthSensor.depth);

        _depthSensor.transform.position = new Vector3(0, -3, 0);
        Utils.CallNonpublicMethod(_depthSensor, "SampleSensor");
        Assert.AreEqual(3, _depthSensor.depth);
    }
}
