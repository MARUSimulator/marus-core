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

public class LidarTest
{
    RaycastLidar _RaycastLidar;
    GameObject _target;

    [OneTimeSetUp]
    public void SetUp()
    {
        _RaycastLidar = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<RaycastLidar>();
        _RaycastLidar.SampleFrequency = 100;
        _RaycastLidar.WidthRes = 1024;
        _RaycastLidar.HeightRes = 128;
        _RaycastLidar.transform.position = new Vector3(0, 0.5f, 0);
        _RaycastLidar.ParticleMaterial = Resources.Load("Material/PointMaterial", typeof(Material)) as Material;
        _target = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _target.transform.position = new Vector3(0, 0, 10);
    }

    [UnityTest]
    public IEnumerator TestLidarSample()
    {
        // skip a few frames
        for (int i = 0; i < 8; i++)
        {
            yield return null;
        }
        var points = new List<Vector3>();
        points.AddRange(_RaycastLidar.pointsCopy);
        List<Vector3> filtered = points.FindAll(e => e != Vector3.zero);
        Assert.Greater(filtered.Count, 0);
        for (int i = 0; i < filtered.Count - 1; i++)
        {
            Assert.AreEqual(_target.GetComponent<Collider>().ClosestPoint(filtered[i]), filtered[i]);
        }
        UnityEngine.Object.Destroy(_RaycastLidar);
        UnityEngine.Object.Destroy(_target);
    }
}
