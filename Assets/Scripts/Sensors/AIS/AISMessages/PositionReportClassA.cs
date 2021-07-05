using UnityEngine;

namespace Labust.Sensors.AIS
{
    /// <summary>
    /// This class implements Class A type AIS position report message.
    /// This covers message types 1, 2 and 3.
    /// For more reference see <see cref="!:https://www.navcen.uscg.gov/?pageName=AISMessagesA">here.</see>
    /// </summary>
    public class PositionReportClassA : AISMessage
    {
        /// <summary>
		/// Speed over ground in 1/10 knot steps
		/// </summary>
        public float SOG { get; set; }

        /// <summary>
		/// Course over ground
		/// </summary>
        public uint COG { get; set; }

        public PositionAccuracy PositionAccuracy { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public uint TrueHeading { get; set; }
        public uint TimeStamp { get; set; }
        public Raim Raim { get; set; }
        public ManeuverIndicator ManeuverIndicator { get; set; }

        public PositionReportClassA()
        {
            this.MessageType = AISMessageType.PositionReportClassA;
            this.PositionAccuracy = PositionAccuracy.Low;
            this.Raim = Raim.NotInUse;
            this.ManeuverIndicator = ManeuverIndicator.NotAvailable;
        }

        public PositionReportClassA(int MMSI)
        {
            this.MMSI = MMSI;
            this.PositionAccuracy = PositionAccuracy.Low;
            this.Raim = Raim.NotInUse;
            this.ManeuverIndicator = ManeuverIndicator.NotAvailable;
        }

        public override string ToString()
        {
            //TODO debug purposes
            return string.Format("MMSI: {0}, Type: {1}, TrueHeading: {2}, COG: {3}, SOG: {4}, Timestamp: {5}", 
                this.MMSI, this.MessageType, this.TrueHeading, this.COG, this.SOG, this.TimeStamp);
        }
    }
}
