using System;

namespace Labust.Sensors.Acoustics
{
    public class NanomodemMessage : AcousticMessage
    {
        /// <summary>
        /// String message used by nanomodems.
        /// eg. $P001, $Q? etc.
        /// </summary>
        public string Message { get; set; }
        public int ReceiverId { get; set; }


        public override string ToString() => Message;

    }

}
