using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using Labust.Core;
using Labust.Networking;
using Marine;
using Sensorstreaming;
using Std;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

namespace Labust.Sensors.Primitive
{
    public class DvlSensor : SensorBase<DvlStreamingRequest>
    {
        [Header("Sensor parameters measurements")]
        [Header("Core measurements")]
        public Vector3 groundVelocity = new Vector3();
        public double[] velocityCovariance = new double[9];
        //public Vector3 velocity;
        //public Rigidbody body;
        
        public float altitude;
        public double altitudeCovariance;

        [Header("Debug Beams")]
        public float[] beamRanges;
        RangeSensor[] beams;

        private Transform sensor;
        private Vector3 lastPosition;

        void Start()
        {
            sensor = GetComponent<Transform>();
            beams = GetComponentsInChildren<RangeSensor>();
            beamRanges = new float[beams.Length];
            lastPosition = sensor.position;
            //AddSensorCallback(SensorCallbackOrder.Last, Refresh);
            streamHandle = streamingClient?.StreamDvlSensor(cancellationToken:RosConnection.Instance.cancellationToken);
        }

        protected override void SampleSensor()
        {
            var position = sensor.position;
            groundVelocity = sensor.worldToLocalMatrix * ((position - lastPosition) / Time.fixedDeltaTime);

            lastPosition = position;

            altitude = Single.MaxValue;
            for (int i = 0; i < beams.Length; ++i)
            {
                beams[i].SampleSensor();

                if (beams[i].range < altitude)
                    altitude = beams[i].range;

                beamRanges[i] = beams[i].range;
            }
            hasData = true;
        }

        protected async override void SendMessage()
        {
            var dvlOut = new TwistWithCovarianceStamped
            {
                Header = new Header()
                {
                    FrameId = frameId,
                    Timestamp = TimeHandler.Instance.TimeDouble
                },
                Twist = new TwistWithCovariance 
                {
                    Twist = new Twist
                    {
                        Linear = groundVelocity.Unity2Body().AsMsg()
                    }
                }
            };
            dvlOut.Twist.Covariance.AddRange(velocityCovariance);
            
            var request = new DvlStreamingRequest
            {
                Address = address,
                Data = dvlOut
            };
            
            Log(new { altitude, groundVelocity });
            await _streamWriter.WriteAsync(request);
            hasData = false;
        }
    }
}
