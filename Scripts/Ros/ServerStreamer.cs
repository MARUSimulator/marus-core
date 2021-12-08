using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using Grpc.Core;

namespace Labust.Networking
{
    public class ServerStreamer<T> where T : IMessage
    {

        public enum MessageHandleMode
        {
            DropAndTakeLast = 1,
            Sequential,
        }

        public MessageHandleMode mode = MessageHandleMode.DropAndTakeLast;
        public bool IsStreaming { get; private set; }

        /// <summary>
        /// Set this in the Awake() method of the sensor script.
        /// Instantiate appropriate client service
        /// </summary>
        protected AsyncServerStreamingCall<T> _streamHandle;

        /// <summary>
        /// Internal buffer for storing response messages.
        /// </summary>
        ConcurrentQueue<T> _responseBuffer = new ConcurrentQueue<T>();


        Thread _handleStreamThread;

        /// <summary>
        /// Start streaming given stream
        /// 
        /// On every message, call given callback method
        /// </summary>
        /// <param name="streamHandle"></param>
        /// <param name="onResponseMsg"></param>
        public void StartStream(AsyncServerStreamingCall<T> streamHandle)
        {
            _streamHandle = streamHandle;
            _handleStreamThread = new Thread(_HandleResponse);
            _handleStreamThread.Start();
            IsStreaming = true;
        }

        /// <summary>
        /// Stop stream that is running
        /// </summary>
        public void StopStream()
        {
            IsStreaming = false;
            _streamHandle.Dispose();
        }

        /// <summary>
        /// Worker thread method to handle the response from stream
        /// </summary>
        /// <returns></returns>
        async void _HandleResponse()
        {
            while (!RosConnection.Instance.IsConnected)
            {
                Thread.Sleep(1000);
            }
            // invoke rpc call
            var stream = _streamHandle.ResponseStream;

            while (await stream.MoveNext(RosConnection.Instance.cancellationToken))
            {
                var current = stream.Current;
                _responseBuffer.Enqueue(current);
            }
        }

        /// <summary>
        /// Handle newly received messages from stream
        /// Call given callback method with received message 
        /// </summary>
        /// <param name="onResponseMsg"></param>
        public void HandleNewMessages(Action<T> onResponseMsg)
        {
            if (_responseBuffer.Count > 0)
            {
                if (mode == MessageHandleMode.Sequential)
                {
                    if (_responseBuffer.TryDequeue(out var result))
                    {
                        onResponseMsg(result);
                    }
                }
                else if (mode == MessageHandleMode.DropAndTakeLast)
                {
                    var last = _responseBuffer.LastOrDefault();
                    if (last != null)
                    {
                        onResponseMsg(last);
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