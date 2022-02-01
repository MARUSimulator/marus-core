using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using Grpc.Core;
using Labust.Networking;
using Remotecontrol;
using UnityEngine;

namespace Labust.Actuators
{
    public class ControllerBase<T> : MonoBehaviour where T : class, IMessage
    {

        public enum MessageHandleMode
        {
            DropAndTakeLast = 1,
            Sequential,
            Latch
        }
        public MessageHandleMode mode = MessageHandleMode.DropAndTakeLast;
        protected RemoteControl.RemoteControlClient remoteControlClient
        {
            get
            {
                if (RosConnection.Instance == null)
                    throw new System.Exception("Ros connection not set!");
                return RosConnection.Instance.GetClient<RemoteControl.RemoteControlClient>();
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
        protected AsyncServerStreamingCall<T> streamHandle;

        /// <summary>
        /// Internal buffer for storing response messages.
        /// </summary>
        ConcurrentQueue<T> _responseBuffer = new ConcurrentQueue<T>();


        T _lastMsg;
        Thread _handleStreamThread;
        Action<T> _onResponseMsg;
        protected void HandleResponse(Action<T> onResponseMsg)
        {
            _handleStreamThread = new Thread(_HandleResponse);
            _handleStreamThread.Start();
            _onResponseMsg = onResponseMsg;
            _lastMsg = null;
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
        void FixedUpdate()
        {
            if (_responseBuffer.Count > 0)
            {
                if (mode == MessageHandleMode.Sequential)
                {
                    if (_responseBuffer.TryDequeue(out var result))
                    {
                        _lastMsg = result;
                        _onResponseMsg(result);
                    }
                }
                else if (mode == MessageHandleMode.DropAndTakeLast)
                {
                    var last = _responseBuffer.LastOrDefault();
                    if (last != null)
                    {
                        _lastMsg = last;
                        _onResponseMsg(last);
                        // clear queue, leave last element
                        while (_responseBuffer.Count > 1 && _responseBuffer.TryDequeue(out var item))
                        {
                            // do nothing
                        }
                    }
                }
            }
            else if (_lastMsg != null && mode == MessageHandleMode.Latch)
            {
                _onResponseMsg(_lastMsg);
            }
        }

    }
}