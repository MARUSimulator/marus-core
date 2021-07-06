using System;

namespace Labust.Sensors.AIS 
{
	/// <summary>
	/// This class implements simple MMSIGenerator using Random library.
	/// </summary>
	public class MMSIGenerator
	{
		private Random r;

		public MMSIGenerator() {
			r = new Random();
		}

		/// <summary>
		/// Generates a 9 digit random integer
		/// </summary>
		public string generateMMSI() {
			int mmsi = r.Next(100000000, 999999999);
			return mmsi.ToString();
		}
	}
}
