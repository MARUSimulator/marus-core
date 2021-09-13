using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

using Google.Protobuf;

using Gemini.EMRS.Core;
using Sensorstreaming;
using Labust.Sensors.Core;
using System.Linq;
using Labust.Visualization;

namespace Labust.Sensors {

    [RequireComponent(typeof(SphericalProjectionFilter))]
    [RequireComponent(typeof(PointCloudManager))]

    /// <summary>
    /// STIL DOES NOT WORK ON UNITY 2020. Works on Unity 2019
    /// </summary>
    public class LidarScript : Sensor
    {
        public ComputeShader lidarShader;
        [Range(3, 5)] public int NumCameras = 4;

        [Space]
        [Header("Lidar Parameters")]
        public int WidthRes = 2048;

        public DepthCameras.BufferPrecision DepthBufferPrecision = DepthCameras.BufferPrecision.bit24;

        public int LidarHorisontalRes = 1024;
        public int NrOfLasers = 16;
        [Range(0.0f, 1f)] public float rayDropProbability = 0.1f;
        [Range(0.01f, 2f)] public float MinDistance = 0.1F;
        [Range(5f, 1000f)] public float MaxDistance = 100F;
        [Range(5.0f, 90f)] public float LidarVerticalAngle = 30f;


        [Space]
        [Header("Sensor Output")]
        [SerializeField] uint NumberOfLidarPoints = 0;

        [HideInInspector] public Camera[] lidarCameras;
        ComputeBuffer pixelCoordinatesBuffer;

        PointCloudManager pointCloud;
        SphericalProjectionFilter projectionFilter;

        int _widthResPerCamera;
        int _lidarHorisonralResPerCamera;
        int HeightRes = 32;
        int kernelHandle;
        public ComputeBufferDataExtractor<Vector3> particeData;
        public ComputeBufferDataExtractor<byte> lidarDataByte;
        public ComputeBufferDataExtractor<uint> randomStateVector;
        public ComputeBufferDataExtractor<Vector4> debug;

        void Start()
        {

            // Setting User information

            _widthResPerCamera = WidthRes / NumCameras;
            _lidarHorisonralResPerCamera = LidarHorisontalRes /  NumCameras;

            // vertical angle at the left or right side of camera frustum
            float lidarVerticalAngle = LidarVerticalAngle;
            HeightRes = (int)(_widthResPerCamera * Mathf.Tan(lidarVerticalAngle * Mathf.Deg2Rad / 2) / Mathf.Sin(Mathf.PI / NumCameras));
            LidarVerticalAngle = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(lidarVerticalAngle * Mathf.Deg2Rad / 2) / Mathf.Cos(Mathf.PI / NumCameras));

            NumberOfLidarPoints = (uint)NrOfLasers * (uint)_lidarHorisonralResPerCamera * (uint)NumCameras;

            // Settup Game objects

            pointCloud = GetComponent<PointCloudManager>();
            pointCloud.SetupPointCloud((int)NumberOfLidarPoints);

            CameraFrustum frustum;
            (lidarCameras, frustum) = DepthCameras.SpawnDepthCameras(transform, NumCameras, _widthResPerCamera, MaxDistance, MinDistance, LidarVerticalAngle);
            DepthCameras.SetCameraBuffers(lidarCameras, frustum, lidarShader, "CSMain");

            projectionFilter = GetComponent<SphericalProjectionFilter>();
            projectionFilter.SetupSphericalProjectionFilter(_lidarHorisonralResPerCamera, NrOfLasers, frustum);
            pixelCoordinatesBuffer = projectionFilter.filterCoordinates.buffer;

            // Setting up Compute Buffers
            kernelHandle = lidarShader.FindKernel("CSMain");

            lidarShader.SetBuffer(kernelHandle, "sphericalPixelCoordinates", pixelCoordinatesBuffer);
            lidarShader.SetInt("N_theta", _lidarHorisonralResPerCamera);
            lidarShader.SetInt("N_phi", NrOfLasers);
            lidarShader.SetFloat("rayDropProbability", rayDropProbability);

            // Used to generate random float 0-1 numbers on GPU
            randomStateVector = new ComputeBufferDataExtractor<uint>(NumCameras * NrOfLasers * _lidarHorisonralResPerCamera, sizeof(float), "_state_xorshift");
            randomStateVector.SetBuffer(lidarShader, "RNG_Initialize");
            randomStateVector.SynchUpdate(lidarShader, "RNG_Initialize");

            randomStateVector.SetBuffer(lidarShader, "CSMain");

            particeData = new ComputeBufferDataExtractor<Vector3>(NumCameras * NrOfLasers * _lidarHorisonralResPerCamera, sizeof(float) * 3, "lines");
            particeData.SetBuffer(lidarShader, "CSMain");

            lidarDataByte = new ComputeBufferDataExtractor<byte>(NumCameras * NrOfLasers * _lidarHorisonralResPerCamera, sizeof(float) * 6, "LidarData");
            lidarDataByte.SetBuffer(lidarShader, "CSMain");

            debug = new ComputeBufferDataExtractor<Vector4>(NumCameras * NrOfLasers * _lidarHorisonralResPerCamera, 4*sizeof(float), "debugBuffer");
            debug.SetBuffer(lidarShader, "CSMain");

        }

        private void Awake()
        {
            SetupSensorCallbacks(
                new SensorCallback(LidarUpdate, SensorCallbackOrder.First),
                new SensorCallback(PointCloudRendering, SensorCallbackOrder.Last),
                new SensorCallback(RecieveLidarData, SensorCallbackOrder.Last)
            );
        }

        public bool SynchronousUpdate = false;

        void LidarUpdate(ScriptableRenderContext context, Camera[] cameras)
        {
            lidarShader.SetFloat("rayDropProbability", rayDropProbability);
            debug.SynchUpdate(lidarShader, "CSMain");

            var max = debug.data.Max(x => x.x);
            var a = 5;
            // lidarShader.Dispatch(kernelHandle, (int)Mathf.Ceil(NumberOfLidarPoints / 1024.0f), 1, 1);
        }

        void PointCloudRendering(ScriptableRenderContext context, Camera[] cameras)
        {
            if (SynchronousUpdate)
            {
                particeData.SynchUpdate(lidarShader,"CSMain");
                if (pointCloud != null) { pointCloud.UpdatePointCloud(particeData.data); }
                gate = true;
            }
            else
            {
                AsyncGPUReadback.Request(particeData.buffer, PointCloudCompleted);
            }
        }

        void PointCloudCompleted(AsyncGPUReadbackRequest request)
        {
            particeData.AsynchUpdate(request);
            if (pointCloud != null) { pointCloud.UpdatePointCloud(particeData.data); }
            gate = true;
        }

        void RecieveLidarData(ScriptableRenderContext context, Camera[] cameras)
        {
            if (SynchronousUpdate)
            {
                lidarDataByte.SynchUpdate(lidarShader, "CSMain");
                message = new LidarMessage(_lidarHorisonralResPerCamera * NrOfLasers * NumCameras, OSPtime, lidarDataByte.data);
                gate = true;
            }
            else
            {
                AsyncGPUReadback.Request(lidarDataByte.buffer, PointCloudDataCompleted);
            }
        }

        private LidarMessage message;

        void PointCloudDataCompleted(AsyncGPUReadbackRequest request)
        {
            lidarDataByte.AsynchUpdate(request);
            message = new LidarMessage(_lidarHorisonralResPerCamera * NrOfLasers * NumCameras, OSPtime, lidarDataByte.data);
            gate = true;


            /*
            var stringarray = System.BitConverter.ToSingle(lidarDataByte.array, lidarDataByte.array.Length - 12);
            Debug.Log("x in bytes: " + stringarray.ToString());
            Debug.Log("CPU is little endian: " + System.BitConverter.IsLittleEndian.ToString());
            */
        }

        // Memo: hadde lønnt seg å lage en konstruktor for dette i den autogenererte koden
        public override bool SendMessage()
        {
            //Debug.Log("Lidar message time: " + message.timeInSeconds.ToString());
            
            LidarStreamingRequest lidarStreamingRequest = new LidarStreamingRequest();
            
            lidarStreamingRequest.TimeInSeconds = message.timeInSeconds;
            lidarStreamingRequest.Height = message.height;
            lidarStreamingRequest.Width = message.width;

            Core.PointField[] fields = message.fields;
            Sensorstreaming.PointField[] pointFields = new Sensorstreaming.PointField[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                Sensorstreaming.PointField tempPointField = new Sensorstreaming.PointField();
                tempPointField.Name = fields[i]._name;
                tempPointField.Offset = fields[i]._offset;
                tempPointField.Datatype = fields[i]._datatype;
                tempPointField.Count = fields[i]._count;

                lidarStreamingRequest.Fields.Add(tempPointField);
            }

            lidarStreamingRequest.IsBigEndian = message.is_bigendian;
            lidarStreamingRequest.PointStep = message.point_step;
            lidarStreamingRequest.RowStep = message.row_step;
            lidarStreamingRequest.Data = ByteString.CopyFrom(message.data);
            lidarStreamingRequest.IsDense = message.is_dense;

            lidarStreamingRequest.IsDense = message.is_dense;

            // bool success = _streamingClient.StreamLidarSensor(lidarStreamingRequest).Success;

            // if (success)
            //     return true;
            return false;
        }
        void OnDestroy()
        {
            particeData.Delete();
            lidarDataByte.Delete();
            randomStateVector.Delete();
            debug.Delete();
        }

    }


}
