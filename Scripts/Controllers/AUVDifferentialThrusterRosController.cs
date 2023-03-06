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

using UnityEngine;

using Remotecontrol;
using System.Threading;
using Marus.Networking;
using System.Linq;
using static Remotecontrol.RemoteControl;
using Marus.Utils;

namespace Marus.Actuators
{

    /// <summary>
    /// Subscribes to ROS server to receive control commands
    /// WIP
    /// </summary>
    public class AUVDifferentialThrusterRosController : MonoBehaviour
    {

        public float linSpeed = 2f;
        public float rotSpeed = 700f;
        public string vehId = "veh";
        public DifferentialThrusterController thrusterController;

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
                cancellationToken: RosConnection.Instance.CancellationToken)
            );
        }

        void Update()
        {
            _streamer.HandleNewMessages();
        }

        void UpdateMovement(ForceResponse result)
        {
            thrusterController.ApplyInput(result.Pwm.Data.ToArray());
        }
    }

}