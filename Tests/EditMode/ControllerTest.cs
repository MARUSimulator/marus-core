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

using System.Collections.Generic;
using NUnit.Framework;
using TestUtils;
using UnityEngine;

public class ControllerTest
{
    GameObject agent1;
    GameObject agent2;
    AgentManager agentManager;
    GameObject cameraObj;

    [OneTimeSetUp]
    public void SetUp()
    {
        agent1 = new GameObject();
        agent2 = new GameObject();

        cameraObj = new GameObject();
        var camera = cameraObj.AddComponent<Camera>();

        agentManager = Utils.CreateAndInitializeObject<AgentManager>("agentManager");
        agentManager.agents = new List<GameObject>(){agent1, agent2};
        Utils.CallStart<AgentManager>(agentManager);
    }

    [Test]
    public void SwitchActiveAgentTest()
    {
        Utils.CallNonpublicMethod<AgentManager>(agentManager, "ChangeAgent");
        Assert.AreEqual(agent2.GetInstanceID(), agentManager.activeAgent.GetInstanceID(), "Agent switch doesn't work");
    }

    [Test]
    public void TestAUVPrimitiveController()
    {
        var agent1Controller = agent1.AddComponent<AUVPrimitiveController>();
        Utils.CallAwake<AUVPrimitiveController>(agent1Controller);

        //Test translation.
        Utils.CallNonpublicMethod<AUVPrimitiveController>(agent1Controller, "UpdateMovement", new object[]{1, KeyCode.W, 1});
        Assert.AreEqual(1, agent1.transform.position.z, "AUV primitive controller translation doesn't work as expected");

        //Test rotation.
        agent1Controller.rotSpeed = 1000f;
        Utils.CallNonpublicMethod<AUVPrimitiveController>(agent1Controller, "UpdateMovement", new object[]{1, KeyCode.X, 1});

        Assert.IsTrue(Mathf.Abs(100 - agent1.transform.eulerAngles.y) < 0.01, "AUV primitive controller rotation doesn't work as expected");
    }

    [Test]
    public void TestCameraController()
    {
        var cameraController = cameraObj.AddComponent<CameraController>();
        Utils.CallAwake<CameraController>(cameraController);

        agentManager.agents = new List<GameObject>(){cameraObj};
        Utils.CallStart<AgentManager>(agentManager);

        //Test translation
        Utils.CallNonpublicMethod<CameraController>(cameraController, "UpdateMovement", new object[]{1, KeyCode.W, 1});
        Assert.AreEqual(1, cameraObj.transform.position.z,"Camera controller translation doesn't work as expected");

        //Test rotation
        cameraController.rotSpeed = 1000f;
        Utils.CallNonpublicMethod<CameraController>(cameraController, "UpdateMovement", new object[]{1, KeyCode.X, 1});

        Assert.IsTrue(Mathf.Abs(100 - cameraObj.transform.eulerAngles.y) < 0.01, "Camera controller rotation doesn't work as expected");

    }


}
