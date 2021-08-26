using System;
using System.Collections;
using System.Collections.Generic;
using Labust.Networking;
using Sensorstreaming;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;

namespace Labust.Sensors.Primitive
{
    public class DvlSensor : SensorBase<DvlStreamingRequest>
    {
        [Header("Core measurements")]
        public Vector3 groundVelocity = new Vector3();
        public float altitude;

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
            AddSensorCallback(SensorCallbackOrder.Last, Refresh);
            streamHandle = streamingClient.StreamDvlSensor(cancellationToken:RosConnection.Instance.cancellationToken);
        }

        void Refresh()
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

        public async override void SendMessage()
        {
            var request = new DvlStreamingRequest
            {
                Address = address,
                Altitude = altitude,
                GroundVelocity = groundVelocity.AsMsg(),
            };
            request.BeamRanges.AddRange(beamRanges);
            Log(new { altitude, groundVelocity });
            await _streamWriter.WriteAsync(request);
            hasData = false;
        }
    }
}
