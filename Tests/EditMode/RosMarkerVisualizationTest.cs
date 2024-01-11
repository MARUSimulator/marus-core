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

using NUnit.Framework;
using TestUtils;
using Marus.Networking;
using System.Collections.Generic;
using Visualization;
using Marus.Visualization;
using Marus.Visualization.Primitives;

public class RosMarkerVisualizationTest
{
    [Test]
    public void TestAddDeleteMarker()
    {
        var m = CreateMockPathMarker();
        VisualizationgRPC.Instance.UpdateMarker(m);
        var f = (Dictionary<string, List<VisualElement>>) Utils.GetNonpublicField<Visualizer>(Visualizer.Instance, "_visualElements");
        Assert.AreEqual(true, f.ContainsKey("ros_markers"));
        Assert.AreEqual(1, f["ros_markers"].Count, "Number of added visual elements should be 1");

        // delete previously added marker
        Marker deleteMarker = new Marker();
        deleteMarker.Id = 123123;
        deleteMarker.Action = Marker.Types.Action.Delete;
        VisualizationgRPC.Instance.UpdateMarker(deleteMarker);
        Utils.CallUpdate(VisualizationgRPC.Instance);
        f = (Dictionary<string, List<VisualElement>>) Utils.GetNonpublicField<Visualizer>(Visualizer.Instance, "_visualElements");
        Assert.AreEqual(true, f.ContainsKey("ros_markers"), "ros_markers tag should exist in Visualizer");
        Assert.AreEqual(0, f["ros_markers"].Count, "Number of added visual elements should be 0");
    }

    [Test]
    public void TestMarkerArrayAddDeleteAll()
    {
        var m1 = CreateMockSingleMarker(1);
        var m2 = CreateMockSingleMarker(2);
        var m3 = CreateMockSingleMarker(3);
        var m4 = CreateMockSingleMarker(4);
        var markerList = new List<Marker>() { m1, m2, m3, m4 };

        var markerArray = new MarkerArray();
        markerArray.Markers.AddRange(markerList);
        VisualizationgRPC.Instance.UpdateMarkerArray(markerArray);
        var f = (Dictionary<string, List<VisualElement>>) Utils.GetNonpublicField<Visualizer>(Visualizer.Instance, "_visualElements");
        Assert.AreEqual(true, f.ContainsKey("ros_markers"));
        Assert.AreEqual(4, f["ros_markers"].Count, "Number of added visual elements should be 4");

        Marker deleteMarker = new Marker();
        deleteMarker.Id = 1;
        deleteMarker.Action = Marker.Types.Action.Delete;
        VisualizationgRPC.Instance.UpdateMarker(deleteMarker);
        f = (Dictionary<string, List<VisualElement>>) Utils.GetNonpublicField<Visualizer>(Visualizer.Instance, "_visualElements");
        Assert.AreEqual(3, f["ros_markers"].Count, "Number of added visual elements should be 3");

        deleteMarker = new Marker();
        deleteMarker.Action = Marker.Types.Action.Deleteall;
        VisualizationgRPC.Instance.UpdateMarker(deleteMarker);
        f = (Dictionary<string, List<VisualElement>>) Utils.GetNonpublicField<Visualizer>(Visualizer.Instance, "_visualElements");
        Assert.AreEqual(false, f.ContainsKey("ros_markers"), "ros_markers tag should not exist in Visualizer");
    }

    Marker CreateMockPathMarker()
    {
        Marker m = new Marker();
        m.Id = 123123;
        m.Type = Marker.Types.Type.LineStrip;
        m.Action = Marker.Types.Action.Add;
        m.Color = new Std.ColorRGBA();
        m.Color.A = 1;
        m.Color.R = 1;
        m.Lifetime = 8	;
        m.Scale = new Geometry.Vector3();
        m.Scale.X = 0.1f;
        m.Scale.Y = 0.01f;
        var p1 = new Geometry.Point();
        p1.X = 0;

        var p2 = new Geometry.Point();
        p2.X = 5;
        p2.Z = 3;

        var p3 = new Geometry.Point();
        p3.X = 10;
        var pointList = new List<Geometry.Point>()
        {
            p1, p2, p3
        };
        m.Points.AddRange(pointList);
        return m;
    }

    Marker CreateMockSingleMarker(int id)
    {
        Marker m = new Marker();
        m.Id = id;
        m.Type = Marker.Types.Type.Cube;
        m.Action = Marker.Types.Action.Add;
        m.Color = new Std.ColorRGBA();
        m.Color.A = 0.5f;
        m.Color.R = 1;
        m.Lifetime = 52;
        m.Pose = new Geometry.Pose();
        m.Pose.Position = new Geometry.Point();
        m.Pose.Position.Y = 5;
        return m;
    }
}
