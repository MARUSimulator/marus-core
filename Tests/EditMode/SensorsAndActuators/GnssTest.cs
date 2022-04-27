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
using Marus.Sensors;
using Marus.Sensors.Primitive;
using TestUtils;

public class GnssTest
{

    GnssSensor _gnss;

    [OneTimeSetUp]
    public void SetUp()
    {
        _gnss = Utils.CreateAndInitializeObject<GnssSensor>("Gnss", PrimitiveType.Cube);
        _gnss.SampleFrequency = 100;
        _gnss.transform.position = new Vector3(200, 5, 100);
    }

    [Test]
    public void TestGnssSample()
    {
        Utils.CallNonpublicMethod(_gnss, "SampleSensor");
        var delta = 0.000001;
        Utils.CallNonpublicMethod(_gnss, "SampleSensor");
        var point = _gnss.point;
        Assert.AreEqual(45.000899, point.latitude, delta, "Latitude is wrong");
        Assert.AreEqual(15.0025366, point.longitude, delta, "Longitude is wrong");
        Assert.AreEqual(5.003915, point.altitude, delta, "Altitude is wrong");
    }
}
