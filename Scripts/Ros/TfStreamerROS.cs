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

namespace Marus.ROS
{
    /// <summary>
    /// Publish TF ROS messages
    /// </summary>
    public class TfStreamerROS : MonoBehaviour
    {
        public float UpdateFrequency = 20;
        /// <summary>
        /// Transform of the parent object.
        /// Orientation and translation are calculated in relationship to this object.
        /// </summary>
        public Transform ParentTransform;

        /// <summary>
        /// Frame ID of the parent object
        /// </summary>
        public string ParentFrameId;

        /// <summary>
        /// Frame ID
        /// </summary>
        public string FrameId;
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

        #if UNITY_EDITOR
        protected void Reset()
        {
            FrameId = $"{vehicle.name}/{gameObject.name}";
        }
        #endif

        Quaternion _rotation;
        Vector3 _translation;
        Quaternion _lastRotation;
        Vector3 _lastTranslation;

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
            if (ParentFrameId == "")
            {
                ParentFrameId = "map";
            }

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
            }
            else
            {
                _translation = transform.position;
                _rotation = transform.rotation;
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
                Translation = _translation.Unity2Map().AsMsg(),
                Rotation = _rotation.Unity2Map().AsMsg(),
                Address = address
            };
            await _streamWriter.WriteAsync(tfOut);
        }
    }
}
