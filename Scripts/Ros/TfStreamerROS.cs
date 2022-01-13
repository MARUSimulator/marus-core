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

using Labust.Networking;
using UnityEngine;
using Labust.Core;
using Std;
using System;
using Grpc.Core;
using static Tf.Tf;

namespace Labust.ROS
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
                if (!RosConnection.Instance.IsConnected)
                    return null;
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
            //var r = RosConnection.Instance;
            address = "/tf";
            if (ParentFrameId == "")
            {
                ParentFrameId = "map";
            }

            streamHandle = streamingClient?.PublishFrame(cancellationToken:RosConnection.Instance.cancellationToken);

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
