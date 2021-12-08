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

public class PoseTest
{

    PoseSensor _poseSensor;
    Rigidbody _rigidBody;
    GameObject _parent;

    [OneTimeSetUp]
    public void SetUp()
    {
        _poseSensor = Utils.CreateAndInitializeObject<PoseSensor>("Pose", PrimitiveType.Cube);
        _poseSensor.SampleFrequency = 100;
        _rigidBody = _poseSensor.gameObject.AddComponent<Rigidbody>();
        _rigidBody.useGravity = false;

        Utils.CallStart<PoseSensor>(_poseSensor);
    }

    [Test]
    public void TestPoseSample()
    {
        Utils.CallFixedUpdate(SensorSampler.Instance);
        _rigidBody.position = new Vector3(2, 3, 5);
        _rigidBody.velocity = new Vector3(0, 0, 0.5f);
        _rigidBody.angularVelocity = new Vector3(0.4f, 0, 0);
        _rigidBody.rotation = new Quaternion(0.6f, 0, 0, 0.8f).normalized;
        Utils.CallFixedUpdate(SensorSampler.Instance);
        Assert.AreEqual(0.4f, _poseSensor.angularVelocity.x);
        Assert.AreEqual(0.5f, _poseSensor.linearVelocity.z);
        Assert.AreEqual(new Vector3(2, 3, 5), _poseSensor.position);
        Assert.AreEqual(0.6f, _poseSensor.orientation.x);
    }
}
