using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Grpc.Core;
using Labust.Core;
using Labust.Networking;
using NUnit.Framework;
using Parameterserver;
using TestUtils;
using Tf;
using UnityEngine;

/// <summary>
/// This are integration tests that test the communication with the gRPC server
/// 
/// For this a mock gRPC server is instantiated with primitive funcionality
/// defined in the SetUp() method
///
/// For performance reasons, this tests are not independent. They all use the same 
/// instatiated singletons instantiated at the SetUp() 
/// 
/// </summary>
public class RosConnectionTest
{
    RosConnection instance;
    Server server;

    /// <summary>
    /// Set when RosConnection calls OnConnect callback
    /// Used to test
    /// </summary>
    bool testConnected;

    
    [OneTimeSetUp]
    public void SetUp()
    {

        var parameters = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 0.5 },
            { "lat", 43.0 },
            { "lon", 12.0 }
        };

        var frames = new TfFrameList();
        frames.Frames.AddRange(
            new List<TfFrame>
            {
                new TfFrame()
                {
                    FrameId = "frame",
                    ChildFrameId = "child",
                }
            }
        );

        // Instantiate mock server
        server = new Server
        {
            Services = 
            { 
                ParameterServer.BindService(new ParameterServerMock(parameters)),
                Tf.Tf.BindService(new TfMock(frames)),
                Ping.Ping.BindService(new PingMock())
            },
            Ports = { new ServerPort("localhost", 30052, ServerCredentials.Insecure) },

        };
        server.Start();

        instance = RosConnection.Instance;
        instance.OnConnected += OnRosConnected;

        // normally, this would be called from coroutine
        var enumerator = (IEnumerator)Utils.CallNonpublicMethod(instance, "WhileConnectionAwait");
        
        int i = 0;
        while (enumerator.MoveNext() && i < 1000) // half a second
        {
            Thread.Sleep(5);
            i++;
        }

    }

    private void OnRosConnected(Channel obj)
    {
        testConnected = true;
    }

    [Test]
    public void TestInstanceCreatedAndConnected()
    {
        var instance = RosConnection.Instance;
        Utils.CallUpdate(instance);
        Assert.NotNull(instance, "Instance should be set ");
        Assert.AreEqual(instance, RosConnection.Instance, "The Instance should be created again"); 


        Assert.NotNull(TfHandler.Instance);
        Assert.NotNull(TimeHandler.Instance);
        Assert.NotNull(ParamServerHandler.Instance);


        Assert.IsTrue(this.instance.IsConnected);
        Assert.IsTrue(testConnected);

    }

    [Test]
    public void TestAddAndGetClient()
    {
        Assert.Throws<Exception>(() =>
        {
            var cl = instance.GetClient<TestClient>();
        });

        instance.AddNewClient<TestClient>();

        Assert.NotNull(instance.GetClient<TestClient>());
    }

    [Test]
    public void TestTfHandlerInit()
    {
        
        var mapFrame = (Transform)Utils.GetNonpublicField(TfHandler.Instance, "_mapFrame");
        Assert.NotNull(mapFrame);
        Assert.AreEqual(Vector3.zero, mapFrame.position);
        Assert.AreEqual(Quaternion.identity, mapFrame.rotation);
    }

    [Test]
    public void TestTfHandlerSetGeoFrameDefault()
    {
        Thread.Sleep(1000);
        var geoFrame = (GeographicFrame)Utils.GetNonpublicField(TfHandler.Instance, "_originGeoFrame");
        var latBefore = geoFrame.origin.latitude;
        var lonBefore = geoFrame.origin.longitude;

        RosConnection.Instance.OriginFrameLatitude = "notExist";
        RosConnection.Instance.OriginFrameLongitude = "notExist";
        TfHandler.Instance.SetGeoOriginAndMap();

        geoFrame = (GeographicFrame)Utils.GetNonpublicField(TfHandler.Instance, "_originGeoFrame");
        Assert.AreEqual(latBefore, geoFrame.origin.latitude, 1e-6);
        Assert.AreEqual(lonBefore, geoFrame.origin.longitude, 1e-6);
    }

    [Test]
    public void TestTfHandlerSetGeoFrame()
    {
        RosConnection.Instance.OriginFrameLatitude = "lat";
        RosConnection.Instance.OriginFrameLongitude = "lon";
        TfHandler.Instance.SetGeoOriginAndMap();

        var geoFrame = (GeographicFrame)Utils.GetNonpublicField(TfHandler.Instance, "_originGeoFrame");
        Assert.AreEqual(43.0, geoFrame.origin.latitude, 1e-6);
        Assert.AreEqual(12.0, geoFrame.origin.longitude, 1e-6);
    }

    [Test]
    public void TestTimeHandler()
    {
        var currTime = TimeHandler.Instance.TimeDouble;
        Utils.CallNonpublicMethod(TimeHandler.Instance, "UpdateTime");
        Utils.CallNonpublicMethod(TimeHandler.Instance, "UpdateTime");
        Utils.CallNonpublicMethod(TimeHandler.Instance, "UpdateTime");
        Utils.CallNonpublicMethod(TimeHandler.Instance, "UpdateTime");
        
        Assert.AreEqual(currTime + 4 * Time.fixedDeltaTime, TimeHandler.Instance.TimeDouble, 1e-6);
    }


    [OneTimeTearDown]
    public async void TearDown()
    {
        if (server != null)
        {
            await server.KillAsync();
        }
    }



}