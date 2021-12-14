using NUnit.Framework;
using UnityEngine;
using Labust.Sensors;
using Labust.Sensors.Primitive;
using TestUtils;

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
    }

    [Test]
    public void TestImuSample()
    {
        _rigidBody.velocity = new Vector3(0.2f, 0, 0);
        _rigidBody.rotation = new Quaternion(0.6f, 0, 0, 0.8f).normalized;
        _rigidBody.angularVelocity = new Vector3(0.3f, 0, 0);
        Utils.CallFixedUpdate(SensorSampler.Instance);
        _rigidBody.velocity = new Vector3(0.25f, 0, 0);
        _rigidBody.angularVelocity = new Vector3(0.35f, 0, 0);
        Utils.CallFixedUpdate(SensorSampler.Instance);
        Assert.AreEqual(2.5f, _imu.linearAcceleration.x, "Linear acceleration in z should be 2.5f");
        Assert.AreEqual(_imu.orientation.x, 0.6f, "Orientation in x should be 0.6f");
        Assert.AreEqual(_imu.angularVelocity.x, 0.35f, "Angular velocity in x should be 0.35f");
    }
}
