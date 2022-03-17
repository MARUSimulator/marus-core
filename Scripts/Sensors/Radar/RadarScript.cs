// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;
using Gemini.EMRS.Core;
using Sensorstreaming;
using Grpc.Core;
using Google.Protobuf;
using Labust.Sensors.Core;

namespace Labust.Sensors
{
    public class RadarScript : Gemini.EMRS.Core.Sensor
    {

        public ComputeShader computeShader;
        [Range(3, 16)] public int NrOfCameras = 8;
        public string RadarLayer = "Radar";

        [Space]
        [Header("Radar Parameters")]
        public Texture2D RadiationPattern;
        public float AntennaGainDbi = 1;
        [SerializeField] private int HeightRes = 32;
        [SerializeField] private int WidthRes = 2048;
        [SerializeField] private int radarSweepResolution = 512;
        [SerializeField] private int radarSpokeResolution = 1024;
        public float PowerW = 20;
        public float RadarFrequencyGhz = 5.4f;
        [Range(5f, 5000f)] public float MaxDistance = 100F;
        [Range(0.01f, 2f)] public float MinDistance = 0.1F;
        [Range(5.0f, 90f)] public float VerticalAngle = 30f;


        [Space(10)]
        [Header("Debugging Options")]
        public RenderTexture RadarPlotExternalImage;
        public RenderTexture RadarSpokeExternalImage;
        [Range(1f, 30000)] public float Sensitivity = 500f;
        [Range(0, 360)] public float SpokeAngle = 0;

        [HideInInspector] public Camera[] radarCameras;
        private RenderTexture RadarPlotImage;
        private RenderTexture RadarSpokeImage;
        private NativeArray<int> RadarSpokesInt;
        private NativeArray<float> RadarSpokesFloat;

        void Start()
        {
            if (RadiationPattern == null)
            {
                RadiationPattern = new Texture2D(WidthRes, HeightRes);
            }
            else
            {
                HeightRes = RadiationPattern.height;
                WidthRes = RadiationPattern.width;
            }

            WidthRes /= NrOfCameras;

            radarCameras = SpawnCameras("DepthCam", NrOfCameras, WidthRes, HeightRes, VerticalAngle, RenderTextureFormat.Depth);

            StartCoroutine(UpdateRadar());
        }

        public struct RadarMessage
        {

            public float _range_increment;     // range increment [m]
            public float _range_start;         // start range of the spoke [m]

            public uint _num_samples;       // number of samples in the spoke
            public uint _num_spokes;

            public uint _min_intensity;   // minimum intensity in intensity range
            public uint _max_intensity;  // maximum intensity in intensity range

            //public RadarMetaData[] _metaData;
            public double[] _timeInSeconds;               // timestamp of scan and coordinate frame id
            public float[] _azimuth;             // azimuth spoke angle [rad]
            public byte[] _radarSpokes;
            public RadarMessage(byte[] radarSpokes, double[] timeInSeconds, float[] azimuth, uint minDistance, uint maxDistance, uint spokeSamples)
            {
                _range_start = minDistance;         // start range of the spoke [m]
                _range_increment = (float)(maxDistance-minDistance)/spokeSamples;     // range increment [m]

                _num_samples = spokeSamples;       // number of samples in the spoke
                _num_spokes = (uint)Mathf.Ceil((float)radarSpokes.Length / spokeSamples);

                _min_intensity = 0;   // minimum intensity in intensity range
                _max_intensity = 15;  // maximum intensity in intensity range

                _radarSpokes = radarSpokes;
                _timeInSeconds = timeInSeconds;               // timestamp of scan and coordinate frame id
                _azimuth = azimuth;             // azimuth spoke angle [rad]
            }
        }

        RadarMessage message;

        public override bool SendMessage()
        {
            int spokeIndex = (int)(radarSweepResolution * (SpokeAngle / 360));
            string Tag = "Radar at angle " + SpokeAngle + ", with " + message._radarSpokes.Length.ToString() +" elements";
            int radarByteIndex = spokeIndex * radarSpokeResolution;
            Helper.PrintPartialByteArrayAs<byte>(message._radarSpokes, radarByteIndex, 512, Tag);
            //Debug.Log("Radar intensity at spoke angle: " + SpokeAngle + ", index 512: " + message._radarSpokes[spokeIndex * radarSpokeResolution + 512].ToString() + " at the time: " + message._metaData[spokeIndex].timeInSeconds.ToString());

            
            // gRPC
            RadarStreamingRequest radarStreamingRequest = new RadarStreamingRequest();

            radarStreamingRequest.RangeStart = message._range_start;
            radarStreamingRequest.RangeIncrement = message._range_increment;
            radarStreamingRequest.NumSpokes = message._num_spokes;
            radarStreamingRequest.NumSamples = message._num_samples;
            radarStreamingRequest.MinIntensity = message._min_intensity;
            radarStreamingRequest.MaxIntensity = message._max_intensity;

            radarStreamingRequest.TimeInSeconds.AddRange(message._timeInSeconds);
            radarStreamingRequest.Azimuth.AddRange(message._azimuth);

            radarStreamingRequest.RadarSpokes = ByteString.CopyFrom(message._radarSpokes);

            // bool success = _streamingClient.StreamRadarSensor(radarStreamingRequest).Success;

            // return success;
            return true;

            /*
            int radarByteIndex = spokeIndex * radarSpokeResolution + 512;
            Helper.PrintPartialByteArrayAs<byte>(message._radarSpokes, radarByteIndex, 64, Tag);
            Debug.Log("Radar time: " + message._timeInSeconds[spokeIndex].ToString() + "with array length " + message._timeInSeconds.Length.ToString());
            Debug.Log("Radar azimuth: " + message._azimuth[spokeIndex].ToString() + "with array length " + message._azimuth.Length.ToString());
            return true;
            */
        }

        private IEnumerator UpdateRadar()
        {
            // Set Global Buffers
            computeShader.SetMatrix("inv_CameraMatrix", radarCameras[0].projectionMatrix.inverse);
            computeShader.SetInt("ImageWidthRes", WidthRes);
            computeShader.SetInt("ImageHeightRes", HeightRes);
            computeShader.SetInt("NrOfImages", NrOfCameras);
            computeShader.SetInt("radarResolution", radarSpokeResolution);
            computeShader.SetInt("radarSweepResolution", radarSweepResolution);
            computeShader.SetFloat("MaxDist", MaxDistance);
            computeShader.SetFloat("MinDist", MinDistance);
            computeShader.SetFloat("AntennaGainDbi", AntennaGainDbi);
            computeShader.SetFloat("PowerW", PowerW);
            computeShader.SetFloat("RadarFrequencyGhz", RadarFrequencyGhz);
            computeShader.SetFloat("SpokeAngle", SpokeAngle);

            // Radar Spokes buffers 
            int RadarSpokesHandle = computeShader.FindKernel("RadarSpokes");
            computeShader.SetTexture(RadarSpokesHandle, "RadarCharacteristics", RadiationPattern);
            int dataSize = sizeof(int);
            ComputeBuffer RadarSpokesIntBuffer = new ComputeBuffer(radarSpokeResolution * radarSweepResolution, dataSize);
            RadarSpokesInt = new NativeArray<int>(radarSpokeResolution * radarSweepResolution, Allocator.Temp, NativeArrayOptions.ClearMemory);
            RadarSpokesIntBuffer.SetData(RadarSpokesInt);
            computeShader.SetBuffer(RadarSpokesHandle, "RadarSpokesInt", RadarSpokesIntBuffer);
            for (int i = 0; i < radarCameras.Length; i++)
            {
                Quaternion angle = Quaternion.Euler(0, i * 360.0f / NrOfCameras, 0);
                Matrix4x4 m = Matrix4x4.Rotate(angle);

                computeShader.SetTexture(RadarSpokesHandle, "depthImage" + i.ToString(), radarCameras[i].targetTexture);
                computeShader.SetMatrix("CameraRotationMatrix" + i.ToString(), m);
            }

            // Radar range Convolution buffers 
            int RadarRangeCovolutionHandle = computeShader.FindKernel("RadarRangeCovolution");
            computeShader.SetBuffer(RadarRangeCovolutionHandle, "RadarSpokesInt", RadarSpokesIntBuffer);
            dataSize = sizeof(float);
            ComputeBuffer RadarSpokesFloatBuffer = new ComputeBuffer(radarSpokeResolution * radarSweepResolution, dataSize);
            RadarSpokesFloat = new NativeArray<float>(radarSpokeResolution * radarSweepResolution, Allocator.Temp, NativeArrayOptions.ClearMemory);
            RadarSpokesFloatBuffer.SetData(RadarSpokesFloat);
            computeShader.SetBuffer(RadarRangeCovolutionHandle, "RadarSpokesFloat", RadarSpokesFloatBuffer);

            // Radar spoke max buffers 
            int RadarSpokeMax = computeShader.FindKernel("FindRadarSpokeMax");
            ComputeBuffer MaxSpokesBuffer = new ComputeBuffer(radarSweepResolution, sizeof(float));
            NativeArray<float> MaxSpokes = new NativeArray<float>(radarSweepResolution, Allocator.Temp, NativeArrayOptions.ClearMemory);
            MaxSpokesBuffer.SetData(MaxSpokes);
            computeShader.SetBuffer(RadarSpokeMax, "MaxSpokes", MaxSpokesBuffer);
            computeShader.SetBuffer(RadarSpokeMax, "RadarSpokesFloat", RadarSpokesFloatBuffer);

            // Radar plot image buffers 
            int RadarPlotImageHandle = computeShader.FindKernel("CreateRadarPlotImage");
            if (RadarPlotExternalImage != null)
            {
                dataSize = 24;
                RadarPlotImage = new RenderTexture(RadarPlotExternalImage.width, RadarPlotExternalImage.height, dataSize);
                RadarPlotImage.enableRandomWrite = true;
                RadarPlotImage.Create();
                computeShader.SetTexture(RadarPlotImageHandle, "plotImage", RadarPlotImage);
                computeShader.SetBuffer(RadarPlotImageHandle, "RadarSpokesFloat", RadarSpokesFloatBuffer);
                computeShader.SetBuffer(RadarPlotImageHandle, "MaxSpokes", MaxSpokesBuffer);
            }

            // Radar spoke image buffers 
            int RadarSpokeImageHandle = computeShader.FindKernel("CreateRadarSpokeImage");
            if (RadarSpokeExternalImage != null)
            {
                dataSize = 24;
                RadarSpokeImage = new RenderTexture(RadarSpokeExternalImage.width, RadarSpokeExternalImage.height, dataSize);
                RadarSpokeImage.enableRandomWrite = true;
                RadarSpokeImage.Create();
                computeShader.SetTexture(RadarSpokeImageHandle, "spokeImage", RadarSpokeImage);
                computeShader.SetBuffer(RadarSpokeImageHandle, "RadarSpokesFloat", RadarSpokesFloatBuffer);
                computeShader.SetBuffer(RadarSpokeImageHandle, "MaxSpokes", MaxSpokesBuffer);
            }

            // Radar Data
            string radarDataKernel = "RadarDataKernel";
            string timeInSecondsBuffer = "timeInSeconds";
            string azimuthBuffer = "azimuth";
            string radarDataBuffer = "spokeData";

            ComputeBufferDataExtractor<byte> radarData = new ComputeBufferDataExtractor<byte>(radarSweepResolution* radarSpokeResolution/4, sizeof(uint), radarDataBuffer);
            radarData.SetBuffer(computeShader, radarDataKernel);

            ComputeBufferDataExtractor<float> azimuth = new ComputeBufferDataExtractor<float>(radarSweepResolution, sizeof(float), azimuthBuffer); ;
            azimuth.SetBuffer(computeShader, radarDataKernel);

            ComputeBufferDataExtractor<double> timeInSeconds = new ComputeBufferDataExtractor<double>(radarSweepResolution, sizeof(double), timeInSecondsBuffer); ;
            timeInSeconds.SetBuffer(computeShader, radarDataKernel);

            int radarDataKernelHandle = computeShader.FindKernel(radarDataKernel);
            computeShader.SetBuffer(radarDataKernelHandle, "RadarSpokesFloat", RadarSpokesFloatBuffer);
            computeShader.SetBuffer(radarDataKernelHandle, "MaxSpokes", MaxSpokesBuffer);

            ComputeBuffer timeArray = new ComputeBuffer(2,sizeof(double));
            double[] timeArr = new double[2] { OSPtime, nextOSPtime };
            timeArray.SetData(timeArr);
            computeShader.SetBuffer(radarDataKernelHandle,"timeArray",timeArray);

            // Radar clear buffers 
            int ClearKernelHandle = computeShader.FindKernel("ClearRadar");
            computeShader.SetBuffer(ClearKernelHandle, "RadarSpokesInt", RadarSpokesIntBuffer);

            // Fetch data from GPU
            float tempSens = -1;
            float tempSpokeAngle = -1;

            while (true)
            {
                // Check change in Inspector Variables
                if (Sensitivity != tempSens)
                {
                    computeShader.SetFloat("Sensitivity", Sensitivity);
                    tempSens = Sensitivity;
                }
                if (SpokeAngle != tempSpokeAngle)
                {
                    computeShader.SetFloat("SpokeAngle", SpokeAngle);
                    tempSpokeAngle = SpokeAngle;
                }

                timeArr[0] = OSPtime;
                timeArr[1] = nextOSPtime;
                timeArray.SetData(timeArr);
                computeShader.SetBuffer(radarDataKernelHandle, "timeArray", timeArray);

                // Create and Radar Data
                computeShader.Dispatch(RadarSpokesHandle, (int)Mathf.Ceil(NrOfCameras * WidthRes / 32), (int)Mathf.Ceil(HeightRes / 32), 1);
                computeShader.Dispatch(RadarRangeCovolutionHandle, (int)Mathf.Ceil(radarSweepResolution / 512), 1, 1);
                computeShader.Dispatch(RadarSpokeMax, (int)Mathf.Ceil(radarSweepResolution / 512), 1, 1);
                computeShader.Dispatch(radarDataKernelHandle, (int)Mathf.Ceil(radarSweepResolution / 512), 1, 1);

                // Recieve and send radar data
                var request2 = AsyncGPUReadback.Request(radarData.buffer);
                yield return new WaitUntil(() => request2.done);
                radarData.AsynchUpdate(request2);

                var request3 = AsyncGPUReadback.Request(azimuth.buffer);
                yield return new WaitUntil(() => request3.done);
                azimuth.AsynchUpdate(request3);

                var request4 = AsyncGPUReadback.Request(timeInSeconds.buffer);
                yield return new WaitUntil(() => request4.done);
                timeInSeconds.AsynchUpdate(request4);

                message = new RadarMessage(radarData.data, timeInSeconds.data, azimuth.data, (uint)MinDistance, (uint)MaxDistance, (uint)radarSpokeResolution);
                gate = true;

                // Debugging Images
                if (RadarPlotExternalImage != null)
                {
                    computeShader.Dispatch(RadarPlotImageHandle, (int)Mathf.Ceil(RadarPlotImage.width / 32), (int)Mathf.Ceil(RadarPlotImage.height / 32), 1);
                }

                if (RadarSpokeExternalImage != null)
                {
                    computeShader.Dispatch(RadarSpokeImageHandle, (int)Mathf.Ceil(RadarSpokeImage.width / 32), (int)Mathf.Ceil(RadarSpokeImage.height / 32), 1);
                }

                // Clear GPU memory
                computeShader.Dispatch(ClearKernelHandle, (int)Mathf.Ceil((radarSpokeResolution * radarSweepResolution) / 1024), 1, 1);
            }
        }

        private Camera[] SpawnCameras(string name, int numbers, int Width, int Height, float verticalAngle, RenderTextureFormat format)
        {

            Camera[] Cameras = new Camera[numbers];
            for (int i = 0; i < numbers; i++)
            {
                GameObject CameraObject = new GameObject();
                CameraObject.name = name + i;
                CameraObject.transform.SetParent(transform);
                CameraObject.transform.localRotation = Quaternion.Euler(0, 180 + (1 / 2 + i) * 360.0f / numbers, 0);
                CameraObject.transform.localPosition = new Vector3(0, 0, 0);

                CameraObject.AddComponent<Camera>();
                Camera cam = CameraObject.GetComponent<Camera>();

                if (cam.targetTexture == null)
                {
                    cam.targetTexture = new RenderTexture(Width, Height, 32, format);
                }

                cam.usePhysicalProperties = false;
                cam.aspect = (360.0f / numbers) / verticalAngle;
                cam.fieldOfView = Camera.HorizontalToVerticalFieldOfView(360.0f / numbers, cam.aspect);
                cam.farClipPlane = MaxDistance;
                cam.nearClipPlane = MinDistance;
                cam.enabled = false;
                cam.cullingMask = LayerMask.GetMask(RadarLayer);
                Cameras[i] = cam;
            }
            return Cameras;
        }

        void OnEnable()
        {
            RenderPipelineManager.endFrameRendering += EndCameraRendering;
        }

        void OnDisable()
        {
            RenderPipelineManager.endFrameRendering -= EndCameraRendering;
        }

        void EndCameraRendering(ScriptableRenderContext context, Camera[] cam)
        {
            if (RadarPlotExternalImage != null)
            {
                Graphics.Blit(RadarPlotImage, RadarPlotExternalImage);
            }
            if (RadarSpokeExternalImage != null)
            {
                Graphics.Blit(RadarSpokeImage, RadarSpokeExternalImage);
            }
        }
    }
}