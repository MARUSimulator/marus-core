using System.Collections;
using System.Collections.Generic;
using Labust.Networking;
using Labust.Sensors.Primitive;
using Sensorstreaming;
using UnityEngine;
using System;
using Unity;

namespace Labust.Sensors.AIS
{
    public class AISSensor : SensorBase<AISStreamingRequest>
    {
        
		public uint TrueHeading;
		public uint SOG;
		public uint COG;

		private Rigidbody rb;
		private GNSSSensor geoSensor;
		private AISDevice aisDevice;
		private Vector3 lastPosition;
		

        void Awake()
        {
            streamHandle = streamingClient.StreamAisSensor(cancellationToken:RosConnection.Instance.cancellationToken);
            AddSensorCallback(SensorCallbackOrder.Last, PositionReport);
            if (string.IsNullOrEmpty(address))
                address = transform.name + "/ais";

			rb = GetComponent<Rigidbody>();
			geoSensor = GetComponent<GNSSSensor>();
			aisDevice = GetComponent<AISDevice>();
        }

		public void FixedUpdate()
		{
			lastPosition = transform.position;
			SensorUpdateHz = 1 / TimeIntervals.getInterval(aisDevice.ClassType, SOG);
			PositionReport();

		}

        public async override void SendMessage()
        {
			var msg = new AISStreamingRequest
            {
				Address = address,
				AisPositionReport = new Common.AISPositionReport 
				{
					Type = (uint) AISMessageType.PositionReportClassA,
					Mmsi = (uint) Int32.Parse(aisDevice.MMSI),
					Heading = (float) TrueHeading,
					Timestamp = (uint) System.DateTime.UtcNow.Second,
					Geopoint = new Common.GeoPoint {
						Latitude = this.geoSensor.point.latitude,
						Longitude = this.geoSensor.point.longitude,
						Altitude = 0
					},
					SpeedOverGround = SOG,
					CourseOverGround = COG
				}
            };
            await _streamWriter.WriteAsync(msg);
            hasData = false;
			
        }
		public void PositionReport()
		{
			SetCOG();
			SetSOG();
			SetTrueHeading();
			hasData = true;
			
		}

		private void SetTrueHeading()
		{
			float myHeading = transform.eulerAngles.y;
			float northHeading = Input.compass.magneticHeading;

			float dif = myHeading - northHeading;
			if (dif < 0) dif += 360f;
			TrueHeading =  (uint) Mathf.Round(dif);
		}

		private void SetCOG()
		{
			Vector3 d = transform.position - lastPosition;
			Vector3 direction = new Vector3(d.x, 0, d.z);
			if (direction != Vector3.zero) 
			{
				Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
				float r = rotation.eulerAngles.y;
				if (r < 0)
				{
					r += 360f;
				}
				COG = (uint) Mathf.Round(r*10);
			}
		}

		private void SetSOG()
		{
			// TODO see if we can remove rigidbody dependency 
			float conversion = 1.94384f; // m/s to kn conversion constant
			float velocity = rb.velocity.magnitude * conversion;
			SOG = (uint) Mathf.Round(velocity * 10);
		}
    }
}