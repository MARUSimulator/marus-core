using System;

namespace Labust.Sensors.AIS
{
    /// <summary>
    /// This class implements simple MMSIGenerator using Random library.
    /// </summary>
    public static class MMSIGenerator
    {
        /// <summary>
        /// Generates a 9 digit random integer
        /// </summary>
        public static string GenerateMMSI() {
            Random r = new Random();
            int mmsi = r.Next(100000000, 999999999);
            return mmsi.ToString();
        }
    }
}
