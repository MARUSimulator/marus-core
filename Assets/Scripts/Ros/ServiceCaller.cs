using Grpc.Core;
using Labust.Core;
using Labust.Utils;
using MissionWaypointNS;
using UnityEngine;
using static Service.Commander;

namespace Labust.Networking
{

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
        public MissionControl MissionControl;



        void Start()
        {
            RosConnection.Instance.OnConnected += OnConnected;
            if (MissionControl != null)
                MissionControl.OnWaypointChange += OnWaypointChange;
        }

        private void OnWaypointChange(MissionWaypoint obj)
        {
            GuidanceTarget = obj.transform.position;
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
            var response = _client.PrimitivePointer(request);
            Debug.Log($"PrimitivePointer: succes={response.Success}");
        }



    }

}