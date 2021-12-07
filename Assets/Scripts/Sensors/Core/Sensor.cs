using UnityEngine;
using UnityEngine.Rendering;
using Grpc.Core;
using Sensorstreaming;

namespace Gemini.EMRS.Core
{
    /// <summary>
    /// Will be removed when Radar and IR are done
    /// </summary>
    public abstract class Sensor : MonoBehaviour
    {
        [Space]
        [Header("Streaming Parameters")]
        public int SensorUpdateHz = 1;

        //public bool RunRecording = false;

        // Change this to the IP that the server are running on for you
        public static string serverIP = "192.168.80.128";

        public static int serverPort = 30052;
        protected static Channel _streamingChannel = new Channel(serverIP + ":" + serverPort, ChannelCredentials.Insecure);
        protected SensorStreaming.SensorStreamingClient _streamingClient = new SensorStreaming.SensorStreamingClient(_streamingChannel);

        public enum SensorCallbackOrder
        {
            First,
            Last
        };

        public struct SensorCallback
        {
            public System.Action<ScriptableRenderContext, Camera[]> _callback;
            public SensorCallbackOrder _executionOrder;
            public SensorCallback(System.Action<ScriptableRenderContext, Camera[]> callback, SensorCallbackOrder executionOrder)
            {
                _callback = callback;
                _executionOrder = executionOrder;
            }
        };


        protected bool gate = false;
        protected double nextOSPtime = 0;
        protected double OSPtime = 0;
        private uint count = 0;
        private double nextActionTime = 0.0f;
        void Update()
        {
            // Repeats experiment
            if (nextOSPtime < OSPtime)
            {
                OSPtime = nextOSPtime;
                nextActionTime = nextOSPtime;
            }

            if (nextOSPtime >= OSPtime)
            {
                if (count % (10 / SensorUpdateHz) == 0)
                {
                    if (gate)
                    {
                        //SendMessage();
                        nextActionTime = nextOSPtime + 1/(double)SensorUpdateHz;
                    }
                }
                count++;
                OSPtime = nextOSPtime;
            }
        }

        private SensorCallback[] sensorCallbackList;

        public void SetupSensorCallbacks(params SensorCallback[] sensorCallbacks)
        {
            sensorCallbackList = sensorCallbacks;
        }

        private void AddSensorCallbacks(params SensorCallback[] sensorCallbacks)
        {
            foreach (SensorCallback sensorCallback in sensorCallbacks)
            {
                if(sensorCallback._executionOrder == SensorCallbackOrder.First)
                {
                    RenderPipelineManager.beginFrameRendering += sensorCallback._callback;
                }
                else if(sensorCallback._executionOrder == SensorCallbackOrder.Last)
                {
                    RenderPipelineManager.endFrameRendering += sensorCallback._callback;
                }
            }
        }

        private void RemoveSensorCallbacks(params SensorCallback[] sensorCallbacks)
        {
            foreach (SensorCallback sensorCallback in sensorCallbacks)
            {
                if (sensorCallback._executionOrder == SensorCallbackOrder.First)
                {
                    RenderPipelineManager.beginFrameRendering -= sensorCallback._callback;
                }
                else if (sensorCallback._executionOrder == SensorCallbackOrder.Last)
                {
                    RenderPipelineManager.endFrameRendering -= sensorCallback._callback;
                }
            }
        }

        public void OnEnable()
        {
            AddSensorCallbacks(sensorCallbackList);
        }

        public void OnDisable()
        {
            RemoveSensorCallbacks(sensorCallbackList);
        }

        public abstract bool SendMessage();


        // External Helper functions


        public static Sensor[] GetActiveSensors()
        {
            var _sensors = GameObject.FindObjectsOfType<Sensor>();
            return _sensors;
        }

        public static void ResetSensorTime(Sensor[] sensors)
        {
            foreach (Sensor sensor in sensors)
            {
                sensor.nextOSPtime = 0;
                sensor.OSPtime = sensor.nextOSPtime;
            }
        }

        public static bool SensorTimeUpdated(Sensor[] sensors)
        {
            bool allSensorsUpdated = true;
            foreach (Sensor sensor in sensors)
            {
                allSensorsUpdated = allSensorsUpdated && (sensor.OSPtime == sensor.nextOSPtime);
            }
            return allSensorsUpdated;
        }

        public static void UpdateSensorTime(double newTime, Sensor[] sensors)
        {
            foreach (Sensor sensor in sensors)
            {
                sensor.nextOSPtime = newTime;
            }
        }
    }
}