using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets active agent that listens to the keyboard input
/// Press C to switch active agent
/// </summary>
public class AgentManager : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> agents;

    [NonSerialized]
    public GameObject activeAgent;

    int _index;
    void Start()
    {
        _index = 0;
        if (agents != null)
        activeAgent = agents[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (agents == null)
            return;
        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeAgent();
        }
    }

    void ChangeAgent()
    {
        _index = (_index + 1) % agents.Count;
        activeAgent = agents[_index];
        Debug.Log($"Selected agent: {agents[_index].name}");
    }
}
