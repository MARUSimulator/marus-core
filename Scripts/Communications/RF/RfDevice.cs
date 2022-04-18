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
            Log($"AcousticDevice-{DeviceId}", new { Name = name, Message = msg, Event = "Sent"});
            if (msg is T asT)
                Send(asT, onAcknowledgeCallback);
        }

        public override void OnReceive(RfMessage msg)
        {
            Log($"AcousticDevice-{DeviceId}", new { Name = name, Message = msg, Event = "Received"});
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
        /// Returns AcousticDevice object based on id.
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
    /// Interface to define behaviour and properties of device in a medium. E.g. nanomodem, ais transponder etc.
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
