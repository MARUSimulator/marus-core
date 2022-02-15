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

using static Visualization.Visualization;
using Visualization;
using UnityEngine;
using Labust.Visualization.Primitives;
using Labust.Core;
using Labust.Visualization;
using System.Collections.Generic;
using Labust.Utils;
using Grpc.Core;

namespace Labust.Networking
{
    public class VisualizationROS : Singleton<VisualizationROS>
    {
        VisualizationClient client;
        ServerStreamer<Marker> streamer1;
        ServerStreamer<MarkerArray> streamer2;
        string default_namespace = "ros_markers";
        HashSet<string> namespaces;

        Dictionary<Marker.Types.Type, PrimitiveType> typeMap = new Dictionary<Marker.Types.Type, PrimitiveType>()
        {
            { Marker.Types.Type.Sphere, PrimitiveType.Sphere },
            { Marker.Types.Type.Cube, PrimitiveType.Cube },
            { Marker.Types.Type.Cylinder, PrimitiveType.Cylinder }
        };

        protected override void Initialize()
        {
            streamer1 = new ServerStreamer<Marker>(UpdateMarker);
            streamer2 = new ServerStreamer<MarkerArray>(UpdateMarkerArray);

            RosConnection.Instance.OnConnected += OnConnected;
            namespaces = new HashSet<string>();
        }

        public void OnConnected(Channel channel)
        {
            string address = "/unity/marker";
            string address2 = "/unity/markerArray";
            var client = RosConnection.Instance.GetClient<VisualizationClient>();

            streamer1.StartStream(client.SetMarker(
                new MarkerRequest {Address = address},
                cancellationToken: RosConnection.Instance.cancellationToken));
            streamer2.StartStream(client.SetMarkerArray(
                new MarkerRequest {Address = address2},
                cancellationToken: RosConnection.Instance.cancellationToken));
        }

        void Update()
        {
            streamer1.HandleNewMessages();
            streamer2.HandleNewMessages();
        }

        public void UpdateMarker(Marker response)
        {
            if (response.Action == Marker.Types.Action.Add)
            {
                CreateMarker(response);
            }
            else if (response.Action == Marker.Types.Action.Delete)
            {
                var key = (response.Ns != "") ? response.Ns : default_namespace;
                var id = $"{key}_{response.Id.ToString()}";
                Visualizer.Instance.RemoveById(id);
            }

            else if (response.Action == Marker.Types.Action.Deleteall)
            {
                foreach (var key in namespaces)
                {
                    Visualizer.Instance.FlushKey(key);
                }
            }
            else if (response.Action == Marker.Types.Action.Modify)
            {
                var key = (response.Ns != "") ? response.Ns : default_namespace;
                var id = $"{key}_{response.Id.ToString()}";
                Visualizer.Instance.RemoveById(id);
                CreateMarker(response);
            }
        }

        private void CreateMarker(Marker response)
        {
            var key = (response.Ns != "") ? response.Ns : default_namespace;
            if (typeMap.ContainsKey(response.Type))
            {
                var p = new Point(response.Pose.Position.AsUnity().Map2Unity());
                p.Lifetime = response.Lifetime;
                p.Id = $"{key}_{response.Id.ToString()}";
                p.PointType = typeMap[response.Type];
                p.SetColor(response.Color.AsUnity());
                Visualizer.Instance.AddPoint(p, key);
            }
            else if (response.Type == Marker.Types.Type.LineStrip)
            {
                var path = new Path();
                path.SetLineColor(response.Color.AsUnity());
                path.Id = $"{key}_{response.Id.ToString()}";
                path.SetLineThickness((float) response.Scale.X);
                path.SetPointSize((float) response.Scale.Y);
                path.Lifetime = response.Lifetime;
                foreach (var point in response.Points)
                {
                    var uPoint = point.AsUnity();
                    path.AddPointToPath(uPoint);
                }
                Visualizer.Instance.AddPath(path, key);
            }
            else if (response.Type == Marker.Types.Type.Arrow)
            {
                var arrow = new Arrow();
                arrow.Id = $"{key}_{response.Id.ToString()}";
                arrow.SetColor(response.Color.AsUnity());
                arrow.SetHeadColor(response.Color.AsUnity());
                if (response.Points.Count == 2)
                {
                    arrow.StartPoint = response.Points[0].AsUnity();
                    arrow.EndPoint = response.Points[1].AsUnity();
                    arrow.SetRadius((float) response.Scale.X);
                    arrow.SetHeadRadius((float) response.Scale.Y);
                    arrow.SetHeadLength((float) response.Scale.Z);
                    Visualizer.Instance.AddArrow(arrow, key);
                }
            }
            namespaces.Add(key);
        }

        public void UpdateMarkerArray(MarkerArray response)
        {
            foreach (var marker in response.Markers)
            {
                UpdateMarker(marker);
            }
        }
    }
}
