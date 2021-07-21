using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using Grpc.Core;
using Labust.Networking;
using AcousticTransmission;
using UnityEngine;

namespace Labust.Actuators
{
    public class AcousticRosControllerBase : MonoBehaviour
    {

        public enum MessageHandleMode
        {
            DropAndTakeLast = 1,
            Sequential,
        }
        public MessageHandleMode mode = MessageHandleMode.DropAndTakeLast;
        protected AcousticTransmission.Acoustics.AcousticsClient acousticsClient
        {
            get
            {
                if (RosConnection.Instance == null)
                    throw new System.Exception("Ros connection not set!");
                return RosConnection.Instance.GetClient<AcousticTransmission.Acoustics.AcousticsClient>();
            }
        }

        protected Rigidbody body;
        /// <summary>
        /// Vehichle that sensor is attached to
        /// Gets rigid body component of a first ancestor
        /// </summary>
        /// <value></value>
        public Transform vehicle
        {
            get
            {
                if (body == null)
                {
                    var component = GetComponent<Rigidbody>();
                    if (component != null)
                        body = component;
                    else
                        body = Utils.Helpers.GetParentRigidBody(transform);
                }
                return body.transform;
            }
        }

        /// <summary>
        /// Set this in the Awake() method of the sensor script.
        /// Instantiate appropriate client service
        /// </summary>
        protected AsyncServerStreamingCall<AcousticResponse> streamHandle;

        /// <summary>
        /// Internal buffer for storing response messages.
        /// </summary>
        ConcurrentQueue<AcousticResponse> _responseBuffer = new ConcurrentQueue<AcousticResponse>();


        Thread _handleStreamThread;
        Action<AcousticResponse> _onResponseMsg;
        protected void HandleResponse(Action<AcousticResponse> onResponseMsg)
        {
            _handleStreamThread = new Thread(_HandleResponse);
            _handleStreamThread.Start();
            _onResponseMsg = onResponseMsg;
        }

        async void _HandleResponse()
        {
            while (!RosConnection.Instance.IsConnected)
            {
                Thread.Sleep(1000);
            }
            // invoke rpc call
            var stream = streamHandle.ResponseStream;

            while (await stream.MoveNext(RosConnection.Instance.cancellationToken))
            {
                var current = stream.Current;
                _responseBuffer.Enqueue(current);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_responseBuffer.Count > 0)
            {
                if (mode == MessageHandleMode.Sequential)
                {
                    if (_responseBuffer.TryDequeue(out var result))
                    {
                        _onResponseMsg(result);
                    }
                }
                else if (mode == MessageHandleMode.DropAndTakeLast)
                {
                    var last = _responseBuffer.LastOrDefault();
                    if (last != null)
                    {
                        _onResponseMsg(last);
                        // clear queue, leave last element
                        while (_responseBuffer.Count > 1 && _responseBuffer.TryDequeue(out var item))
                        {
                            // do nothing
                        }
                    }
                }
            }
        }

    }
}