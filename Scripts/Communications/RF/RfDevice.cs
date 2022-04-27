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
using Marus.Networking;
using Marus.Logger;
using System;

namespace Marus.Communications.Rf
{

    public abstract class RfDevice<T> : RfDevice where T : RfMessage
    {

        public abstract void OnReceive(T message);
        protected abstract void Send(T message, 
                Action<T> onAcknowledgeCallback=null,
                Action<T> onTimeoutCallback=null);

        public override void Send(RfMessage msg, 
                Action<RfMessage> onAcknowledgeCallback=null,
                Action<RfMessage> onTimeoutCallback=null)
        {
            Log($"RfDevice-{DeviceId}", new { Name = name, Message = msg, Event = "Sent"});
            if (msg is T asT)
                Send(asT, onAcknowledgeCallback);
        }

        public override void OnReceive(RfMessage msg)
        {
            Log($"RfDevice-{DeviceId}", new { Name = name, Message = msg, Event = "Received"});
            if (msg is T asT)
                OnReceive(msg as T);
        }

    }

    public abstract class RfDevice : MonoBehaviour, RfTransmiter, RfReceiver
    {
        /// <summary>
        /// Transmitting range, in meters (m).
        /// </summary>
        public float Range;

        public abstract int DeviceId { get; }
        public abstract string Protocol { get; }

        protected GameObjectLogger Logger;
        protected void Log<W>(string topic, W data)
        {
            if (Logger == null)
            {
                Logger = DataLogger.Instance.GetLogger<W>(topic);
            }
            (Logger as GameObjectLogger<W>).Log(data);
        }

        public Vector3 Location => transform.position;


        public abstract RfTransmiterParams GetTransmiterParams();

        /// <summary>
        /// Returns RfDevice object based on id.
        /// </summary>
        public T GetDeviceById<T>(uint id) where T : RfDevice
        {
            foreach (var dev in RfMediumHelper.GetAllRfDevices())
            {
                if (dev is T asT
                    && dev.DeviceId == id)
                {
                    return asT;
                }
            }
            return null;
        }

        public abstract void OnReceive(RfMessage message);
        public abstract void Send(RfMessage message, 
            Action<RfMessage> onAcknowledgeCallback=null,
            Action<RfMessage> onTimeoutCallback=null);
    }

    public interface RfTranciever : RfTransmiter, RfReceiver
    {
    }

    public interface RfReceiver
    {
        Vector3 Location { get; }
        int DeviceId { get; }
        string Protocol { get; }
        /// <summary>
        /// Processing received messages.
        /// </summary>
        void OnReceive(RfMessage message);
    }

    /// <summary>
    /// Interface to define behaviour and properties of device in a medium
    /// </summary>
    public interface RfTransmiter
    {
        Vector3 Location { get; }
        int DeviceId { get; }
        string Protocol { get; }
        void Send(RfMessage message, 
                Action<RfMessage> onAcknowledgeCallback=null,
                Action<RfMessage> onTimeoutCallback=null);
    }
}
