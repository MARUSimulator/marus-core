using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Labust.Sensors;
using Labust.Sensors.Primitive;
using Labust.Networking;
using UnitTests;

public class GnssTest
{

    GnssSensor _gnss;

    [OneTimeSetUp]
    public void SetUp()
    {
        _gnss = TestUtils.CreateAndInitializeObject<GnssSensor>("Gnss", PrimitiveType.Cube);
        _gnss.transform.position = new Vector3(200, 5, 100);
    }

    [Test]
    public void TestGnssSample()
    {
        var delta = 0.000001;
        TestUtils.CallFixedUpdate(SensorSampler.Instance);
        var point = _gnss.point;
        Assert.AreEqual(45.000899, point.latitude, delta, "Latitude is wrong");
        Assert.AreEqual(15.0025366, point.longitude, delta, "Longitude is wrong");
        Assert.AreEqual(5.003915, point.altitude, delta, "Altitude is wrong");
    }
}
