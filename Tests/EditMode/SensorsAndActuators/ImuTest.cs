// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
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
using Marus.Sensors.Primitive;
using TestUtils;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;

public class ImuTest
{

    ImuSensor _imu;
    Rigidbody _rigidBody;

    [OneTimeSetUp]
    public void SetUp()
    {
        _imu = Utils.CreateAndInitializeObject<ImuSensor>("Imu", PrimitiveType.Cube);
        _imu.SampleFrequency = 100;
        _imu.transform.position = new Vector3(1, 0, 0);
        _rigidBody = _imu.gameObject.GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;

        _imu.AccelerometerNoise.NoiseTypeFullName = "Marus.NoiseDistributions.Noise+NoNoise";
        _imu.AccelerometerNoise.ParameterKeys = new List<string>();
        _imu.AccelerometerNoise.ParameterValues = new List<string>();

        _imu.GyroNoise.NoiseTypeFullName = "Marus.NoiseDistributions.Noise+NoNoise";
        _imu.GyroNoise.ParameterKeys = new List<string>();
        _imu.GyroNoise.ParameterValues = new List<string>();

        _imu.OrientationNoise.NoiseTypeFullName = "Marus.NoiseDistributions.Noise+NoNoise";
        _imu.OrientationNoise.ParameterKeys = new List<string>();
        _imu.OrientationNoise.ParameterValues = new List<string>();
    }

    [UnityTest]
    public IEnumerator TestImuSample()
    {
        float vel_first = 1.0f, vel_later = 1.5f;
        
        _rigidBody.velocity = new Vector3(vel_first, 0, 0);
        _rigidBody.rotation = new Quaternion(0.6f, 0, 0, 0.8f).normalized;
        _rigidBody.angularVelocity = new Vector3(0.3f, 0, 0);
        Utils.CallNonpublicMethod(_imu, "SampleSensor");

        // skip one frame (deltaTime)
        yield return null;

        _rigidBody.velocity = new Vector3(vel_later, 0, 0);
        _rigidBody.angularVelocity = new Vector3(0.35f, 0, 0);
        Utils.CallNonpublicMethod(_imu, "SampleSensor");

        float expectedLAx = (vel_later - vel_first) / Time.deltaTime;
        
        Assert.AreEqual(_imu.linearAcceleration.x, expectedLAx, $"Linear acceleration in x should be {expectedLAx}");
        Assert.AreEqual(_imu.orientation.x, 0.6f, "Orientation in x should be 0.6f");
        Assert.AreEqual(_imu.angularVelocity.x, 0.35f, "Angular velocity in x should be 0.35f");
    }
}
