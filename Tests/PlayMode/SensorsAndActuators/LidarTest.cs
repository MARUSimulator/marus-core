// Copyright 2021 Laboratory for Underwater Systems and Technologies (LABUST)
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

using NUnit.Framework;
using UnityEngine;
using Marus.Sensors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using System.Linq;

public class LidarTest
{
    RaycastLidar _RaycastLidar;
    GameObject _target;

    [OneTimeSetUp]
    public void SetUp()
    {
        _RaycastLidar = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<RaycastLidar>();
        _RaycastLidar.SampleFrequency = 100;
        _RaycastLidar.transform.position = new Vector3(0, 0.5f, 0);
        var jsonText = File.ReadAllText(JsonConfigPath());
        _RaycastLidar.Configs = JsonConvert.DeserializeObject<List<LidarConfig>>(jsonText);
        _RaycastLidar.ConfigIndex = 3;
        _RaycastLidar.ParticleMaterial = Resources.Load("Material/PointMaterial", typeof(Material)) as Material;
        _target = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _target.transform.position = new Vector3(0, 0, 10);
    }

    string JsonConfigPath()
    {
        var jsonTextFile = Resources.Load<TextAsset>("Configs/lidars");
        return Path.Combine(Path.GetDirectoryName(Application.dataPath), AssetDatabase.GetAssetPath(jsonTextFile));
    }

    [UnityTest]
    public IEnumerator TestLidarSample()
    {
        // skip a few frames
        for (int i = 0; i < 20; i++)
        {
            yield return null;
        }
        var _filteredIndices = _RaycastLidar.Readings.Select((v, i) => new { v, i }).Where(x => x.v.IsValid).Select(x => x.i);
        var _filteredPoints = _filteredIndices.Select(m => _RaycastLidar.Points[m]).ToList();
        Assert.Greater(_filteredPoints.Count, 0);
        for (int i = 0; i < _filteredPoints.Count - 1; i++)
        {
            var transformedPoint = _RaycastLidar.transform.TransformPoint(_filteredPoints[i]);
            Assert.AreEqual(_target.GetComponent<Collider>().ClosestPoint(transformedPoint), transformedPoint);
        }
        UnityEngine.Object.Destroy(_RaycastLidar);
        UnityEngine.Object.Destroy(_target);
    }
}
