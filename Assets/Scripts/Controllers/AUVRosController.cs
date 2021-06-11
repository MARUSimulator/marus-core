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

namespace Labust.Actuators
{

    /// <summary>
    /// Subscribes to ROS server to receive control commands
    /// WIP
    /// </summary>
    public class AUVRosController : ControllerBase<ForceResponse>
    {

        public float linSpeed = 2f;
        public float rotSpeed = 700f;
        public string vehId = "veh";
        public ThrusterController thrusterController;

        Transform _targetTransform;
        Thread _handleStreamThread;


        // Start is called before the first frame update
        void Awake()
        {
            _targetTransform = transform;
            streamHandle = remoteControlClient.ApplyForce(
                new ForceRequest { Address = vehicle.name + "/pwm_out" },
                cancellationToken: RosConnection.Instance.cancellationToken);

            HandleResponse(UpdateMovement);
        }

        void UpdateMovement(ForceResponse result)
        {
            thrusterController.ApplyPwm(result.Pwm.Out.ToArray());
        }
    }

}