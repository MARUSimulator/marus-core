using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Labust.Sensors.Primitive.Acoustic;
using UnitTests;
using Acoustictransmission;
using Labust;
using Labust.Sensors.Acoustics;

public class NanomodemTest
{

    Nanomodem _nanomodem1;
	Nanomodem _nanomodem2;

    [OneTimeSetUp]
    public void SetUp()
    {
        _nanomodem1 = TestUtils.CreateAndInitializeObject<Nanomodem>("Nanomodem1", PrimitiveType.Cube);
		TestUtils.SetNonpublicField(_nanomodem1, "Id", 1);
		_nanomodem1.Range = 1000;
		_nanomodem1.transform.position = new Vector3(0, -2, 10);
		var rb = _nanomodem1.gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;

		_nanomodem2 = TestUtils.CreateAndInitializeObject<Nanomodem>("Nanomodem2", PrimitiveType.Cube);
		TestUtils.SetNonpublicField(_nanomodem1, "Id", 2);
        _nanomodem2.transform.position = new Vector3(0, -2, 40);
		_nanomodem2.Range = 1000;
		var rb2 = _nanomodem2.gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
	}

    [Test]
    public void TestNanomodemRange()
    {
		var delta = _nanomodem1.RangingIncrement;

		NanomodemRequest req = new NanomodemRequest()
		{
			ReqType = NanomodemRequest.Types.Type.Pingid,
			Id = 2
		};

		NanomodemROS rosController = _nanomodem1.gameObject.GetComponent<NanomodemROS>();
		TestUtils.CallAwake<NanomodemROS>(rosController);

		// List<AcousticPayload> payloadList = rosController.ParseAndExecuteCommand(req);
		// Assert.AreEqual(3, payloadList.Count, "Number of response payloads is wrong");

		// var msg1 = payloadList[0].Payload.Msg;
		// Assert.AreEqual("$P002", msg1, "Acknowledge message is wrong");

		// var range = payloadList[1].Range.RangeM;
		// Assert.AreEqual(30, range, delta, "Measured range is wrong");
    }
}
