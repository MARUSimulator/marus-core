

using System;
using Unity.Robotics.ROSTCPConnector;
using UnityEditor;
using UnityEngine;
using System.IO;

public class RosConfig
{

    static RosConfig()
    {
        _instance = new RosConfig(_defaultConfigPath);
    }

    
    private static readonly string _defaultConfigPath = 
        Path.Combine(Application.dataPath, "config.yaml");

    private static RosConfig _instance;
    private static string _rosConfigPath;
    static bool _isSet;

    public static RosConfig Instance 
    {
        get 
        {
            if (!_isSet)
                Debug.Log($"RosConfig instance not set!");
            return _instance;
        }
    }

    private RosConfig(string path)
    {
        _isSet = true;
    }

    public static RosConfig LoadRosConfig(string path)
    {
        // TODO
        return _instance;
    }

    public string[] GetTopicList() 
    {
        return new string[] {"aassas", "slike"};
    }

}