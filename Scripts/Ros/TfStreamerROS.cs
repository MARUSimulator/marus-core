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

using Marus.Networking;
using UnityEngine;
using Marus.Core;
using Std;
using Grpc.Core;
using static Tf.Tf;
using Marus.Utils;
using Marus.CustomInspector;

namespace Marus.ROS
{
    /// <summary>
    /// Publish TF ROS messages
    /// </summary>
    public class TfStreamerROS : MonoBehaviour
    {
        public float UpdateFrequency = 1;
        /// <summary>
        /// Transform of the parent object.
        /// Orientation and translation are calculated in relationship to this object.
        /// </summary>
        public Transform ParentTransform;


        [SerializeField]
        string parentFrameId;
        /// <summary>
        /// Frame ID of the parent object
        /// </summary>
        public string ParentFrameId
        {
            get
            {
                var veh = vehicle;
                if (string.IsNullOrEmpty(parentFrameId))
                {
                    parentFrameId = $"map";
                }
                return TfHandler.ParseTfName(parentFrameId, veh);
            }
        }


        [SerializeField]
        string frameId;

        /// <summary>
        /// Frame ID
        /// </summary>
        public string FrameId
        {
            get
            {
                var veh = vehicle;
                if (string.IsNullOrEmpty(frameId))
                {
                    frameId = $"veh/{name}";
                }
                return TfHandler.ParseTfName(frameId, veh);
            }
        }

        public bool AddOffset = false;

        [ConditionalHideInInspector("AddOffset", false)]
        public Vector3 TranslationOffset;

        [ConditionalHideInInspector("AddOffset", false)]
        public Vector3 RotationOffset;

        string address;

        protected Transform _vehicle;
        public Transform vehicle
        {
            get
            {
                _vehicle = Helpers.GetVehicle(transform);
                if (_vehicle == null)
                {
                    Debug.Log($@"Cannot get vehicle from sensor {transform.name}.
                        Using sensor as the vehicle transform");
                    return transform;
                }
                return _vehicle;
            }
        }

        Quaternion _rotation;
        Vector3 _translation;

        /// <summary>
        /// A client instance used for streaming tf messages
        /// </summary>
        /// <value></value>
        protected TfClient streamingClient
        {
            get
            {
                return RosConnection.Instance.GetClient<TfClient>();
            }
        }

        public AsyncClientStreamingCall<Tf.TfFrame, Empty> streamHandle;


        /// <summary>
        /// Used to write tf messages
        /// </summary>
        /// <value></value>
        protected IClientStreamWriter<Tf.TfFrame> _streamWriter
        {
            get
            {
                if (streamHandle != null)
                    return streamHandle.RequestStream;
                return null;
            }
        }



        public void Start()
        {
            address = "/tf";
            streamHandle = streamingClient?.PublishFrame(cancellationToken:RosConnection.Instance.CancellationToken);
        }

        double cumulativeTime = 0;
        void Update()
        {
            cumulativeTime += Time.deltaTime;
            if (cumulativeTime > (1 / UpdateFrequency) && RosConnection.Instance.IsConnected)
            {
                cumulativeTime = 0;
                UpdateTransform();
                SendMessage();
            }
        }

        void UpdateTransform()
        {
            if (ParentTransform != null)
            {
                _translation = ParentTransform.InverseTransformPoint(transform.position);
                _rotation = (Quaternion.Inverse(ParentTransform.transform.rotation) * transform.rotation);
                if (AddOffset)
                {
                    _translation += TranslationOffset;
                    _rotation *= Quaternion.Euler(RotationOffset);
                }
                // if parent is assigned, assume it is local position (body frame) and transform to (forward-left-up) FLU
                _translation = _translation.Unity2Body();
                _rotation = _rotation.Unity2Body();
            }
            else
            {
                // if no parent is assigned, assume it is global position and transform to ENU frame
                _rotation =  transform.rotation.Unity2Map();
                _translation = transform.position.Unity2Map();

            }
        }

        protected async void SendMessage()
        {
            var tfOut = new Tf.TfFrame
            {
                Header = new Header
                {
                    FrameId = FrameId,
                    Timestamp = TimeHandler.Instance.TimeDouble
                },
                FrameId = ParentFrameId,
                ChildFrameId = FrameId,
                Translation = _translation.AsMsg(),
                Rotation = _rotation.AsMsg(),
                Address = address
            };
            await _streamWriter.WriteAsync(tfOut);
        }
    }
}
