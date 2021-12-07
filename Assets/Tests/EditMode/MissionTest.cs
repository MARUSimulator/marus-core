using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Labust.Mission;
using System.Linq;
using UnitTests;

public class MissionTest
{
    /// <summary>
        /// Tests for mission waypoint and mission control.
        /// Functionalities tested are part of missionWaypoint.cs and missionControl.cs scripts.
    /// </summary>
    GameObject agent = new GameObject();
    GameObject missionObject = new GameObject();
    GameObject[] waypointsObject = {new GameObject("waypoint1"), new GameObject("waypoint2")};
    
    [OneTimeSetUp]
    public void SetUp()
    {
        MissionControl mission = missionObject.AddComponent<MissionControl>();
        agent.AddComponent<MeshCollider>();


        foreach (GameObject waypoint in waypointsObject){
            MissionWaypoint missionWaypointTemp = waypoint.AddComponent<MissionWaypoint>();
            missionWaypointTemp.mission = mission;
            mission.waypointObjects.Add(missionWaypointTemp);
        }

        mission.messages = new List<string>{waypointsObject[0].name, waypointsObject[1].name, "finished"};
        
        GameObject text = new GameObject();
        UnityEngine.UI.Text textComponent = text.AddComponent<UnityEngine.UI.Text>();
        mission.textElement = textComponent;

        mission.agent = agent;

        foreach(GameObject wp in waypointsObject){
            wp.AddComponent<MeshRenderer>();
            MissionWaypoint waypoint = wp.AddComponent<MissionWaypoint>();

            //This is used in onTriggerEnter function for checking if mission agent has entered waypoint.
            waypoint.mission = mission;
        }
    }

    [Test]
    public void TestDisableWaypoint()
    {
        waypointsObject[0].SetActive(true);
        waypointsObject[0].GetComponent<MissionWaypoint>().DisableWaypoint();
        Assert.AreEqual(waypointsObject[0].activeSelf, false, "Can't disable the waypoint");
    }

    [Test]
    public void TestEnableWaypoint()
    {
        //Set initial waypoint values.
        waypointsObject[0].SetActive(false);
        waypointsObject[0].GetComponent<MeshRenderer>().enabled = false;
        
        //Enabling waypoint with displayWaypoint set to true.
        waypointsObject[0].GetComponent<MissionWaypoint>().EnableWaypoint(true);
        Assert.AreEqual(waypointsObject[0].activeSelf, true, "Can't enable the waypoint");
        var waypointVisible = waypointsObject[0].GetComponent<MeshRenderer>().enabled;
        Assert.AreEqual(waypointVisible, true, "Waypoint isn't visible after enabling, even though displayWaypoint parameter was true");

        //Set initial waypoint values.
        waypointsObject[0].SetActive(false);
        waypointsObject[0].GetComponent<MeshRenderer>().enabled = true;

        //Enabling waypoint with displayWaypoint set to false.
        waypointsObject[0].GetComponent<MissionWaypoint>().EnableWaypoint(false);
        Assert.AreEqual(waypointsObject[0].activeSelf, true, "Can't enable the waypoint");
        waypointVisible = waypointsObject[0].GetComponent<MeshRenderer>().enabled;
        Assert.AreEqual(waypointVisible, false, "Waypoint is visible after enabling, even though displayWaypoint parameter was false");
        
    }

    [Test]
    public void TestDisableOnEntering()
    {
        //Insure waypoint is initially enabled.
        waypointsObject[0].SetActive(true);

        //Simulate agent entering the waypoint.
        TestUtils.CallOnTriggerEnter(waypointsObject[0].GetComponent<MissionWaypoint>(), agent.GetComponent<MeshCollider>());

        Assert.AreEqual(waypointsObject[0].activeSelf, false, "Waypoint wasn't disabled after the agent has entered it's area");
    }

    [Test]
    public void TestMissionControl()
    {
        MissionControl mission = missionObject.GetComponent<MissionControl>();
        //Set inital waypoint values.
        foreach(GameObject wp in waypointsObject){
            wp.SetActive(false);
            TestUtils.SetNonpublicField(wp.GetComponent<MissionWaypoint>(), "_visited", false);
        }
        TestUtils.CallStart(mission);
        TestUtils.CallUpdate(mission);

        //Check if initally fist message is dispalyed and if only first waypoint is enabled.
        Assert.AreEqual(mission.textElement.text, waypointsObject[0].name, "Initialy text element is not set correctly.");
        Assert.AreEqual(waypointsObject[0].activeSelf, true, "First waypoint isn't inititally enabled.");
        Assert.AreEqual(waypointsObject[1].activeSelf, false, "Only first waypoint should be enabled at the beginning of the mission.");

        // Trigger first waypoint.
        TestUtils.CallOnTriggerEnter(waypointsObject[0].GetComponent<MissionWaypoint>(), agent.GetComponent<MeshCollider>());
        TestUtils.CallUpdate(mission);
        Assert.AreEqual(mission.textElement.text, waypointsObject[1].name, "Text element hasn't been changed after first waypoint is reached.");
        Assert.AreEqual(waypointsObject[0].activeSelf, false, "First waypoint should be disabled after visited.");
        Assert.AreEqual(waypointsObject[1].activeSelf, true, "Only second waypoint should be enabled after first waypoint is reached."); 

        // Trigger second waypoint.
        TestUtils.CallOnTriggerEnter(waypointsObject[1].GetComponent<MissionWaypoint>(), agent.GetComponent<MeshCollider>());
        TestUtils.CallUpdate(mission);
        Assert.AreEqual(mission.textElement.text, "finished", "Final message doesn't display in text element.");
        Assert.AreEqual(waypointsObject[0].activeSelf, false, "First waypoint should be disabled after mission has finished.");
        Assert.AreEqual(waypointsObject[1].activeSelf, false, "Second waypoint should be disabled after mission has finished."); 
    }
}
