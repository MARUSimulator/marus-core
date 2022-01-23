using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Remotecontrol;
using Grpc.Core;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Concurrent;
using Labust.Networking;
using System.Linq;
using static Remotecontrol.RemoteControl;
using Labust.Utils;

namespace Labust.Actuators
{

    /// <summary>
    /// Subscribes to ROS server to receive control commands
    /// WIP
    /// </summary>
    public class AUVRosController : MonoBehaviour
    {

        public float linSpeed = 2f;
        public float rotSpeed = 700f;
        public string vehId = "veh";
        public ThrusterController thrusterController;

        Transform _targetTransform;
        Thread _handleStreamThread;
        ServerStreamer<ForceResponse> _streamer;

        // Start is called before the first frame update
        void Awake()
        {
            _targetTransform = transform;
            _streamer = new ServerStreamer<ForceResponse>(UpdateMovement);
            var client = RosConnection.Instance.GetClient<RemoteControlClient>();
            var address = Helpers.GetVehicle(transform)?.name ?? name;
            _streamer.StartStream(client.ApplyForce(
                new ForceRequest { Address = $"{address}/pwm_out" },
                cancellationToken: RosConnection.Instance.cancellationToken)
            );
        }

        void Update()
        {
            _streamer.HandleNewMessages();
        }

        void UpdateMovement(ForceResponse result)
        {
            thrusterController.ApplyPwm(result.Pwm.Data.ToArray());
        }
    }

}