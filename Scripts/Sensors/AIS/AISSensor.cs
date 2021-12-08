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
    [RequireComponent(typeof(GnssSensor))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AisDevice))]
    public class AisSensor : SensorBase
    {
        public uint TrueHeading;
        public uint SOG;
        public uint COG;

        public GnssSensor geoSensor;
        private Rigidbody rb;
        private AisDevice aisDevice;
        private Vector3 lastPosition;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            geoSensor = GetComponent<GnssSensor>();
            aisDevice = GetComponent<AisDevice>();
        }

        protected override void SampleSensor()
        {
            lastPosition = transform.position;
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
