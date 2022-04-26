using UnityEngine;
using Marus.Logger;
using System;

namespace Marus.Communications.Acoustics
{

    public abstract class AcousticDevice<T> : AcousticDevice where T : AcousticMessage
    {
        public abstract void OnReceive(T message);
        public abstract void Send(T message, 
                Action<T> onAcknowledgeCallback=null,
                Action<T> onTimeoutCallback=null);

        public override void Send(AcousticMessage msg, 
                Action<AcousticMessage> onAcknowledgeCallback=null,
                Action<AcousticMessage> onTimeoutCallback=null)
        {
            Log($"AcousticDevice-{DeviceId}", new { Name = name, Message = msg, Event = "Sent"});
            if (msg is T asT)
                Send(asT, onAcknowledgeCallback);
        }

        public override void OnReceive(AcousticMessage msg)
        {
            Log($"AcousticDevice-{DeviceId}", new { Name = name, Message = msg, Event = "Received"});
            if (msg is T asT)
                OnReceive(msg as T);
        }
    }

    public abstract class AcousticDevice : MonoBehaviour, AcousticTransmiter, AcousticReceiver
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


        // REST OF THE PARAMS

        /// <summary>
        /// Returns AcousticDevice object based on id.
        /// </summary>
        public T GetDeviceById<T>(uint id) where T : AcousticDevice
        {
            foreach (var dev in AcousticMediumHelper.GetAllAcousticDevices())
            {
                if (dev is T asT
                    && dev.DeviceId == id)
                {
                    return asT;
                }
            }
            return null;
        }

        public abstract void OnReceive(AcousticMessage message);
        public abstract void Send(AcousticMessage message, 
            Action<AcousticMessage> onAcknowledgeCallback=null,
            Action<AcousticMessage> onTimeoutCallback=null);
    }

    public interface AcousticTranciever : AcousticTransmiter, AcousticReceiver
    {
    }

    public interface AcousticReceiver
    {
        Vector3 Location { get; }
        int DeviceId { get; }
        string Protocol { get; }
        /// <summary>
        /// Processing received messages.
        /// </summary>
        void OnReceive(AcousticMessage message);
    }

    /// <summary>
    /// Interface to define behaviour and properties of device in a medium. E.g. nanomodem, ais transponder etc.
    /// </summary>
    public interface AcousticTransmiter
    {
        Vector3 Location { get; }
        int DeviceId { get; }
        string Protocol { get; }
        void Send(AcousticMessage message, 
                Action<AcousticMessage> onAcknowledgeCallback=null,
                Action<AcousticMessage> onTimeoutCallback=null);
    }
}
