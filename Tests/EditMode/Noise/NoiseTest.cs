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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Marus.NoiseDistributions;
using NUnit.Framework;
using TestUtils;
using UnityEngine;

public class NoiseTest
{

    class TestNoise : INoise
    {
        public float A;
        public int B;
        public string C;
        public string ignore;
        public float Sample()
        {
            return A + B + C.Length;
        }
    }

    [OneTimeSetUp]
    public void SetUp()
    {
        // force load noise types
        var types = Noise.NoiseTypes;

        // add test noise to NoiseType unsafely
        var f = typeof(Noise).GetField("_noiseTypes",
            BindingFlags.Static | BindingFlags.NonPublic);
        var l = (List<Type>)f.GetValue(null);
        l.Add(typeof(TestNoise));

    }

    [Test]
    public void TestNoiseTypeDetection()
    {
        var types = Noise.NoiseTypes;
        Assert.GreaterOrEqual(types.Count, 2, "There should be some noise types detected");
        Assert.AreEqual("NoNoise", types[0].Name, "First noise should be of NoNoise type");
        Debug.Log(string.Join(" ", types.Select(x => x.FullName)));
        var testNoise = types.FirstOrDefault(x => x.FullName == typeof(TestNoise).FullName);
        Assert.NotNull(testNoise, "TestNoise type should be in the NoiseTypes list");
    }

    [Test]
    public void TestNoiseSample()
    {
        var noiseParams = new NoiseParameters
        {
            NoiseTypeFullName = typeof(TestNoise).FullName,
            ParameterKeys = new List<string> {"A", "B", "C"},
            ParameterValues = new List<string> {"0.01", "5", "str"}
        };
        var s = Noise.Sample(in noiseParams);
        Assert.AreEqual(8.01, s, .000001f, "Value should be 8.01");
    }


}
