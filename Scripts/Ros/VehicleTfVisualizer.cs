// Copyright 2026 Laboratory for Underwater Systems and Technologies (LABUST)
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

using Marus.Networking;
using UnityEngine;
using Marus.Core;
using Grpc.Core;
using static Tf.Tf;
using Marus.Utils;
using Tf;

namespace Marus.ROS
{
    /// <summary>
    /// Subscribe to vehicle TF from ROS2 and update Unity GameObject.
    /// This is for visualization only - no Unity physics involved.
    /// The actual dynamics run in ROS2 (i.e. Fossen math. model).
    /// </summary>
    public class VehicleTfVisualizer : MonoBehaviour
    {
        [Header("ROS2 TF Settings")]
        [Tooltip("The TF frame name for the vehicle (e.g., 'base_link', 'namespace/base_link')")]
        public string VehicleFrameId = "base_link";
        
        [Tooltip("The parent frame (typically 'map' or 'odom')")]
        public string ParentFrameId = "map";

        [Header("Update Settings")]
        [Tooltip("How often to query TF updates (Hz). 0 = every frame")]
        public float UpdateFrequency = 30f;

        [Header("Visualization")]
        [Tooltip("The GameObject to move (leave empty to use this GameObject)")]
        public GameObject VehicleModel;

        [Header("Coordinate Frame Adjustment")]
        [Tooltip("Apply coordinate transformation from ROS (ENU) to Unity")]
        public bool ApplyCoordinateTransform = true;

        private TfClient _tfClient;
        private ServerStreamer<TfFrameList> _tfStreamer;
        private double _lastUpdateTime;
        private bool _isConnected;

        void Start()
        {
            if (VehicleModel == null)
            {
                VehicleModel = gameObject;
            }

            RosConnection.Instance.OnConnected += OnConnected;
        }

        public void OnConnected(ChannelBase channel)
        {
            _tfClient = RosConnection.Instance.GetClient<TfClient>();
            _tfStreamer = new ServerStreamer<TfFrameList>(OnTfUpdate);
            
            // Start streaming TF updates
            var tfStream = _tfClient.StreamAllFrames(
                new Std.Empty(), 
                cancellationToken: RosConnection.Instance.CancellationToken
            );
            
            _tfStreamer.StartStream(tfStream);
            _isConnected = true;
            
            Debug.Log($"VehicleTfVisualizer: Connected, listening for frame '{VehicleFrameId}'");
        }

        void Update()
        {
            if (!_isConnected) return;

            // Rate limiting
            if (UpdateFrequency > 0)
            {
                if (Time.timeAsDouble < _lastUpdateTime + (1.0 / UpdateFrequency))
                {
                    return;
                }
                _lastUpdateTime = Time.timeAsDouble;
            }

            // Process new TF messages
            _tfStreamer?.HandleNewMessages();
        }

        private void OnTfUpdate(TfFrameList frameList)
        {
            // Find our vehicle frame in the TF tree
            TfFrame vehicleFrame = null;
            
            foreach (var frame in frameList.Frames)
            {
                // Match by child frame ID (this is the frame name)
                if (frame.ChildFrameId == VehicleFrameId)
                {
                    vehicleFrame = frame;
                    break;
                }
            }

            if (vehicleFrame == null)
            {
                return; // Frame not found in this update
            }

            // Update the Unity GameObject
            UpdateVehiclePose(vehicleFrame);
        }

        private void UpdateVehiclePose(TfFrame frame)
        {
            Vector3 position;
            Quaternion rotation;

            if (ApplyCoordinateTransform)
            {
                // TODO: ENU or NED transform
                position = frame.Translation.AsUnity().Map2Unity();
                rotation = frame.Rotation.AsUnity().Map2Unity();
            }
            else
            {
                // Direct mapping (no coordinate transform)
                position = frame.Translation.AsUnity();
                rotation = frame.Rotation.AsUnity();
            }

            // Apply to GameObject
            VehicleModel.transform.position = position;
            VehicleModel.transform.rotation = rotation;
        }

        void OnDisable()
        {
            _tfStreamer?.StopStream();
        }

        void OnDestroy()
        {
            _tfStreamer?.StopStream();
        }
    }
}