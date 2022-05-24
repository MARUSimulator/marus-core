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
using UnityEngine;
using Marus.Quest;
using System.Linq;
using TestUtils;
using NUnit.Framework;

public class QuestTest
{
    /// <summary>
        /// Tests for quest waypoint and quest control.
        /// Functionalities tested are part of questWaypoint.cs and questContoll.cs scripts.
    /// </summary>
    GameObject player = new GameObject();
    GameObject questObject = new GameObject();
    GameObject[] waypointsObject = {new GameObject("waypoint1"), new GameObject("waypoint2")};
    
    [OneTimeSetUp]
    public void SetUp()
    {
        QuestControl quest = questObject.AddComponent<QuestControl>();
        player.AddComponent<MeshCollider>();

        quest.messages = new List<string>{waypointsObject[0].name, waypointsObject[1].name, "finished"};
        
        GameObject text = new GameObject();
        UnityEngine.UI.Text textComponent = text.AddComponent<UnityEngine.UI.Text>();
        quest.textElement = textComponent;

        quest.agent = player;

        foreach(var wp in waypointsObject){
            wp.AddComponent<MeshRenderer>();
            QuestWaypoint waypoint = wp.AddComponent<QuestWaypoint>();

            //This is used in onTriggerEnter function for checking if quest player has entered waypoint.
            waypoint.quest = quest;
        }

        quest.waypointObjects = waypointsObject.Select(x => x.GetComponent<QuestWaypoint>()).ToList();
    }

    [Test]
    public void TestDisableWaypoint()
    {
        waypointsObject[0].SetActive(true);
        waypointsObject[0].GetComponent<QuestWaypoint>().DisableWaypoint();
        Assert.AreEqual(waypointsObject[0].activeSelf, false, "Can't disable the waypoint");
    }

    [Test]
    public void TestEnableWaypoint()
    {
        //Set initial waypoint values.
        waypointsObject[0].SetActive(false);
        waypointsObject[0].GetComponent<MeshRenderer>().enabled = false;
        
        //Enabling waypoint with displayWaypoint set to true.
        waypointsObject[0].GetComponent<QuestWaypoint>().EnableWaypoint(true);
        Assert.AreEqual(waypointsObject[0].activeSelf, true, "Can't enable the waypoint");
        var waypointVisible = waypointsObject[0].GetComponent<MeshRenderer>().enabled;
        Assert.AreEqual(waypointVisible, true, "Waypoint isn't visible after enabling, even though displayWaypoint parameter was true");

        //Set initial waypoint values.
        waypointsObject[0].SetActive(false);
        waypointsObject[0].GetComponent<MeshRenderer>().enabled = true;

        //Enabling waypoint with displayWaypoint set to false.
        waypointsObject[0].GetComponent<QuestWaypoint>().EnableWaypoint(false);
        Assert.AreEqual(waypointsObject[0].activeSelf, true, "Can't enable the waypoint");
        waypointVisible = waypointsObject[0].GetComponent<MeshRenderer>().enabled;
        Assert.AreEqual(waypointVisible, false, "Waypoint is visible after enabling, even though displayWaypoint parameter was false");
        
    }

    [Test]
    public void TestDisableOnEntering()
    {
        //Insure waypoint is initially enabled.
        waypointsObject[0].SetActive(true);

        //Simulate player entering the waypoint.
        Utils.CallOnTriggerEnter(waypointsObject[0].GetComponent<QuestWaypoint>(), player.GetComponent<MeshCollider>());

        Assert.AreEqual(waypointsObject[0].activeSelf, false, "Waypoint wasn't disabled after the player has entered it's area");
    }

    [Test]
    public void TestQuestControl()
    {
        QuestControl quest = questObject.GetComponent<QuestControl>();
        Utils.CallStart(quest);
        //Set inital waypoint values.
        foreach(var wp in waypointsObject){
            wp.SetActive(false);
            
            Utils.SetNonpublicField(
                wp.GetComponent<QuestWaypoint>(), "_visited", false);
        }

        Utils.CallUpdate(quest);

        //Check if initally fist message is dispalyed and if only first waypoint is enabled.
        Assert.AreEqual(quest.textElement.text, waypointsObject[0].name, "Initialy text element is not set correctly.");
        Assert.AreEqual(waypointsObject[0].activeSelf, true, "First waypoint isn't inititally enabled.");
        Assert.AreEqual(waypointsObject[1].activeSelf, false, "Only first waypoint should be enabled at the beginning of the quest.");

        // Trigger first waypoint.
        Utils.CallOnTriggerEnter(waypointsObject[0].GetComponent<QuestWaypoint>(), player.GetComponent<MeshCollider>());
        Utils.CallUpdate(quest);
        Assert.AreEqual(quest.textElement.text, waypointsObject[1].name, "Text element hasn't been changed after first waypoint is reached.");
        Assert.AreEqual(waypointsObject[0].activeSelf, false, "First waypoint should be disabled after visited.");
        Assert.AreEqual(waypointsObject[1].activeSelf, true, "Only second waypoint should be enabled after first waypoint is reached."); 

        // Trigger second waypoint.
        Utils.CallOnTriggerEnter(waypointsObject[1].GetComponent<QuestWaypoint>(), player.GetComponent<MeshCollider>());
        Utils.CallUpdate(quest);
        Assert.AreEqual(quest.textElement.text, "finished", "Final message doesn't display in text element.");
        Assert.AreEqual(waypointsObject[0].activeSelf, false, "First waypoint should be disabled after quest has finished.");
        Assert.AreEqual(waypointsObject[1].activeSelf, false, "Second waypoint should be disabled after quest has finished."); 
    }
}
