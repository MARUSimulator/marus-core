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

using Grpc.Core;
using Marus.Core;
using Marus.Quest;
using UnityEngine;
using static Service.Commander;
using Labust;

namespace Marus.Networking
{

    /// <summary>
    /// This is primitive ServiceCaller and not to be used. This is just a POC
    ///
    /// Hardcoded to call specific service called PrimitivePointer
    /// RosServices will be called just like gRPC services and the methods will be
    /// autogenerated from .srv files
    /// </summary>
    public class ServiceCaller : MonoBehaviour
    {

        private ServiceCaller _instance;
        CommanderClient _client;

        public string RadiusTopic;
        public string GuidanceTopic;
        public bool GuidanceEnable;
        public Vector3 GuidanceTarget;
        public double Radius;
        public double VerticalOffset;
        public QuestControl QuestControl;



        void Start()
        {
            RosConnection.Instance.OnConnected += OnConnected;
            if (QuestControl != null)
                QuestControl.OnWaypointChange += OnWaypointChange;
        }

        private void OnWaypointChange(QuestWaypoint obj)
        {
            GuidanceTarget = obj.transform.position;
            GuidanceEnable = true;
            CallPrimitivePointerService();
        }

        public void OnConnected(Channel channel)
        {
            _client = new CommanderClient(channel);
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                CallPrimitivePointerService();
            }
        }


        private void GetServiceList()
        {
            // _serverStreamer = new ServerStreamer<TfFrameList>();
            // transform.parent = RosConnection.Instance.transform;
            // RosConnection.Instance.OnConnected += OnConnected;
            // InitializeTf();
        }

        public void CallService()
        {
            // _serverStreamer = new ServerStreamer<TfFrameList>();
            // transform.parent = RosConnection.Instance.transform;
            // RosConnection.Instance.OnConnected += OnConnected;
            // InitializeTf();
        }

        public void CallPrimitivePointerService()
        {
            Debug.Log("Calling service: PrimitivePointer");
            var request = new PointerPrimitiveService();
            request.RadiusTopic = RadiusTopic;
            request.Radius = Radius;
            request.GuidanceTopic = GuidanceTopic;
            request.GuidanceEnable = GuidanceEnable;
            var toEnu = GuidanceTarget.Unity2Map();
            request.GuidanceTarget = new Geometry.Vector3();
            request.GuidanceTarget.X = toEnu.y;
            request.GuidanceTarget.Y = toEnu.x;
            request.GuidanceTarget.Z = -toEnu.z;
            request.GuidanceTopic = GuidanceTopic;
            request.VerticalOffset = VerticalOffset;

            // TODO: fill request
            // request
            if (_client != null)
            {
                var response = _client.PrimitivePointer(request);
                Debug.Log($"PrimitivePointer: succes={response.Success}");
            }
        }
    }
}
