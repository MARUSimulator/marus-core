using UnityEngine;
using Unity;
using System;
using Labust.Sensors.Primitive.GenericMedium;

namespace Labust.Sensors.AIS 
{
	/// <summary>
	///  This class implements AIS capabilities.
	/// </summary>
	public class AISDevice : MediumDevice<AISMessage>
	{	
		/// <summary>
		/// AIS class type: A or B
		/// More info <see cref="!:https://www.navcen.uscg.gov/?pageName=typesAIS">here.</see>
		/// </summary>
		public AISClassType ClassType;

		/// <summary>
		/// Maritime Mobile Service Identity
		/// Unique 9 digit number assigned to radio or AIS unit.
		/// </summary>
		public string MMSI = "";

		/// <summary>
		/// Name of the vessel used in some message types, maximum 20 ASCII characters.
		/// Default as set is standard for undefined.
		/// </summary>
		public string Name = "@@@@@@@@@@@@@@@@@@@@";

		/// <summary>
		/// Set transimission on or off, receiving will be enabled regardless.
		/// </summary>
		public Boolean ActiveTransmission = true;

		private float period = 0;
		private float delta = 0;
		private AISMessage message;
		private Vector3 lastPosition;
		private MediumBase<AISMessage> AISMedium;
		private Rigidbody rb;
		

		public void Awake()
		{
			if (string.IsNullOrEmpty(MMSI))
			{
				MMSI = MMSIGenerator.GenerateMMSI();
			}
			rb = GetComponent<Rigidbody>();
			// Register object to AIS manager
			AISMedium = AISManager.Instance;
			AISMedium.Register(this);
		}

		

		public void FixedUpdate()
		{   
			if (ActiveTransmission)
			{

				period = TimeIntervals.getInterval(ClassType, GetSOG());
				if (delta > period)
				{	
					SetRange();
					message = PositionReport();
					MediumMessage<AISMessage> radioMessage = new MediumMessage<AISMessage>(this, message);
					AISMedium.Broadcast(radioMessage);
					delta = 0;
				}
				delta += Time.deltaTime;
				lastPosition = transform.position;
			}
			
		}

		public override void Receive<AISMessage>(MediumMessage<AISMessage> msg)
		{
			Debug.Log(msg);
		}
		
		private PositionReportClassA PositionReport()
		{
			// TODO lat-long
			PositionReportClassA msg = new PositionReportClassA(int.Parse(this.MMSI));
			msg.TrueHeading = GetTrueHeading();
			msg.COG = GetCOG();
			msg.SOG = GetSOG();
			msg.TimeStamp = (uint) System.DateTime.UtcNow.Second;
			return msg;
		}

		private uint GetTrueHeading()
		{
			float myHeading = transform.eulerAngles.y;
			float northHeading = Input.compass.magneticHeading;

			float dif = myHeading - northHeading;
			if (dif < 0) dif += 360f;
			return (uint) Mathf.Round(dif);
		}

		private uint GetCOG()
		{
			Vector3 d = transform.position - lastPosition;
			Vector3 direction = new Vector3(d.x, 0, d.z);
			Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
			float r = rotation.eulerAngles.y;
			if (r < 0)
			{
				r += 360f;
			}
			return (uint) Mathf.Round(r*10);
		}

		private float GetSOG()
		{
			// TODO see if we can remove rigidbody dependency 
			float conversion = 1.94384f; // mps to kn conversion constant
			float velocity = rb.velocity.magnitude * conversion;
			return Mathf.Round(velocity * 10);
		}

		private (float, float) GetLatLong()
		{	
			// TODO when GNSS sensor is ready;
			return (0f, 0f);
		}

		private void SetRange()
		{
			if (ClassType == AISClassType.ClassA)
			{	
				// 75km range for 12.5W transponder
				this.Range = 75f * 1000;
			}
			else
			{
				// 15km range for 2W transponder
				this.Range = 15f * 1000;
			}
		}
	}
}
