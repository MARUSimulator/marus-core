﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        activeAgent = agents[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (agents == null)
            return;
        if (Input.GetKeyDown(KeyCode.C))
        {
            _index = (_index + 1) % agents.Count;
            activeAgent = agents[_index];
            Debug.Log($"Selected agent: {agents[_index].name}");
        }

    }
}
