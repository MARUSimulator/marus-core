using System;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity;
using Labust.Sensors.Primitive.GenericMedium;

namespace Labust.Sensors.Primitive.Acoustic
{
	public class Nanomodem : MediumDeviceBase<AcousticMessage>
	{
		/// <summary>
		/// Unique identifier of nanomodem
		/// 0-255
		/// </summary>
		public int Id;

		/// <summary>
		/// Nanomodem's supply voltage in Volts (V) 3-6.5V
		/// </summary>
		public double SupplyVoltage;
		
		/// <summary>
		/// Medium object used for communication between nanomodems
		/// </summary>
		public AcousticMedium medium;

		private NanomodemRosController rosController;

		public void Awake()
		{
			medium = (AcousticMedium) AcousticMedium.Instance;
			medium.Register(this);
			rosController = gameObject.AddComponent(typeof(NanomodemRosController)) as NanomodemRosController;
			SupplyVoltage = GetRandomVoltage();
		}

		public void Update()
		{
		}

		public override void Receive(AcousticMessage msg)
		{
			rosController.ExecuteNanomodemResponse(msg);
		}

		/// </summary>
		/// Returns range to nanomodem by id in meters.
		/// </summary>
		public float Range(int id)
		{
			//if id is not found, it will return 0 distance
			Nanomodem target = this;
			List<MediumDeviceBase<AcousticMessage>> nanomodems = medium.RegisteredDevices;
			foreach (Nanomodem nanomodem in nanomodems)
			{
				if (nanomodem.Id == id){
					target = nanomodem;
					break;
				}
			}

			return medium.DistanceFromTo(this, target);
		}

		/// </summary>
		/// Returns range to nanomodem by id recalculated for usage with nanomodem messages.
		/// R = yyyyy * c * 3.125e -5
		/// Returns yyyyy calculated from R (range in meters) and C (speed of sound in medium)
		/// </summary>
		public int GetRangeTransformed(int id)
		{
			float range = Range(id);
			return (int) Mathf.Round((float) (range / medium.C * 3.125 * 0.00001));
		}

		/// <summary>
		/// Returns random double in range 3 - 6.5 V
		/// This is used for setting initial supply voltage
		/// </summary>
		private double GetRandomVoltage()
		{
			// Supply voltage is in range 3-6.5V
			return new System.Random().NextDouble() * (6.5 - 3) + 3;
		}

		/// <summary>
		/// Returns voltage converted for usage in nanomodem messages
		/// v = yyyyy * 15/65536
		/// Returns yyyyy calculated from v (SupplyVoltage)
		/// </summary>
		public int GetConvertedVoltage()
		{
			// Convert to integer
			return (int) Mathf.Round((float) (SupplyVoltage * 65536 / 15));
		}
	}
}
