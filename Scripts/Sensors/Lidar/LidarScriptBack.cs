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
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;


//namespace Gemini.EMRS.Lidar {
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Material))]
    public class LidarScriptBack : MonoBehaviour
    {


        public ComputeShader computeShader;
        [Range(3, 4)] public int NrOfCameras = 4;
        public string LidarLayer = "Lidar";

        [Space]
        [Header("Lidar Parameters")]

        public int WidthRes = 2048;

        public enum DepthPrecision // your custom enumeration
        {
            bit16,
            bit24,
            bit32
        };
        public DepthPrecision DepthBufferPrecision = DepthPrecision.bit24;

        public int LidarHorisontalRes = 1024;
        public int NrOfLasers = 16;
        [Range(0.01f, 2f)] public float MinDistance = 0.1F;
        [Range(5f, 1000f)] public float MaxDistance = 100F;
        [Range(5.0f, 90f)] public float VerticalAngle = 30f;


        [Space]
        [Header("Sensor Output")]
        [SerializeField] private uint NumberOfDepthPixels = 0;
        [SerializeField] private uint NumberOfLidarPoints = 0;

        [Space(10)]
        [Header("Debugging Options")]
        public RenderTexture sphericalMask;
        public Material particleMaterial;
        public bool ShowPointCloud = true;

        [HideInInspector] public Camera[] lidarCameras;
        private int HeightRes = 32;
        private RenderTexture sphericalMaskImage;
        private Mesh particleMesh;
        private NativeArray<Vector3> particleCloud;
        ComputeBuffer pixelCoordinatesBuffer;

        void Start()
        {
            // Setting User information

            WidthRes /= NrOfCameras;
            float lidarVerticalAngle = VerticalAngle;
            HeightRes = (int)(WidthRes * Mathf.Tan(lidarVerticalAngle * Mathf.Deg2Rad / 2) / Mathf.Sin(Mathf.PI / NrOfCameras));
            VerticalAngle = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(lidarVerticalAngle * Mathf.Deg2Rad / 2) / Mathf.Cos(Mathf.PI / NrOfCameras));

            NumberOfDepthPixels = (uint)WidthRes * (uint)HeightRes * (uint)NrOfCameras;
            NumberOfLidarPoints = (uint)NrOfLasers * (uint)LidarHorisontalRes * (uint)NrOfCameras;

            // Settup Game objects

            lidarCameras = SpawnCameras("DepthCam", NrOfCameras, WidthRes, HeightRes, VerticalAngle, RenderTextureFormat.Depth);
            SetupLidarPointCloud();

            // Setting up Global Buffers

            computeShader.SetMatrix("inv_CameraMatrix", lidarCameras[0].projectionMatrix.inverse);
            computeShader.SetMatrix("CameraMatrix", lidarCameras[0].projectionMatrix);

            computeShader.SetFloat("VFOV_camera", VerticalAngle * Mathf.Deg2Rad);
            computeShader.SetFloat("VFOV_lidar", lidarVerticalAngle * Mathf.Deg2Rad);
            computeShader.SetInt("NrOfLasers", NrOfLasers);
            computeShader.SetInt("HRES_lidar", LidarHorisontalRes);

            computeShader.SetInt("ImageWidthRes", WidthRes);
            computeShader.SetInt("ImageHeightRes", HeightRes);
            computeShader.SetInt("NrOfImages", NrOfCameras);

            SphericalPixelCoordinates(NrOfLasers, (int)Mathf.Ceil(LidarHorisontalRes));

            StartCoroutine(UpdateLidarLines());


        }

        UnifiedArray<Vector3> particleUnifiedArray;
        UnifiedArray<byte> lidarDataByte;
        UnifiedArray<LidarFieldData> lidarData;
        LidarFieldData[] LidarDataFormated;
        //[System.Serializable]
        private struct LidarFieldData {
            public Vector3 position;
            public float intensity;
            private readonly uint ring;
            private readonly float time;
        }

        private struct UnifiedArray<T> where T : struct {
            public NativeArray<T> nativeArray;
            public ComputeBuffer buffer;
            public T[] array;
            public string bufferName;
            public UnifiedArray(int numberOfElements, int elementSizeBytes, string gpuBufferName)
            {
                ComputeBuffer dataBuffer = new ComputeBuffer(numberOfElements, elementSizeBytes);
                NativeArray<T> dataArray;
                var VarType = typeof(T);
                if (VarType == typeof(byte)) {
                    dataArray = new NativeArray<T>(numberOfElements * elementSizeBytes, Allocator.Temp, NativeArrayOptions.ClearMemory);
                } else {
                    dataArray = new NativeArray<T>(numberOfElements, Allocator.Temp, NativeArrayOptions.ClearMemory);
                }
                dataBuffer.SetData(dataArray);
                bufferName = gpuBufferName;
                nativeArray = dataArray;
                array = nativeArray.ToArray();
                buffer = dataBuffer;
            }
            public void SetBuffer(ComputeShader shader, string kernelName) {
                int kernelHandle = shader.FindKernel(kernelName);
                shader.SetBuffer(kernelHandle, bufferName, buffer);
            }
        }
        /*
        private struct UnifiedCompute<T> where T: struct    {
            private UnifiedArray<T>[] _unifiedArrays;
            private ComputeShader _kernelShader;
            private string _kernelName;
            private int _kernelHandle;
            public UnifiedCompute(ComputeShader kernelShader, string kernelName, params UnifiedArray<T>[] unifiedArrays)
            {
                _unifiedArrays = unifiedArrays;
                _kernelShader = kernelShader;
                _kernelName = kernelName;
                _kernelHandle = _kernelShader.FindKernel(_kernelName);
                for (int i = 0; i < _unifiedArrays.Length; i++)
                {
                    _kernelShader.SetBuffer(_kernelHandle, _unifiedArrays[i].bufferName, _unifiedArrays[i].buffer);
                }

            }
            private void UpdateBuffers()
            {
                _kernelShader.Dispatch(_kernelHandle, (int)Mathf.Ceil((float)_unifiedArrays.Length / 1024.0f), 1, 1);
            }
            private int GetNativeArray(int index)
            {
                var request = AsyncGPUReadback.Request(_unifiedArrays[index].buffer);
                //yield return new WaitUntil(() => request2.done);
                //lidarDataByte.nativeArray = request2.GetData<byte>();
                //byte[] LidarByteArray = lidarDataByte.nativeArray.ToArray();
                return 1;
            }
        }*/

       


        private void Update()
        {
            //Debug.Log("tes: " + tes.ToString());

            //Debug.Log("Intensity in update: " + lidarData.array[0].intensity.ToString());

            if (lidarDataByte.nativeArray != null)
            {
                //particleMesh.SetVertices(particleUnifiedArray.nativeArray);
                //byte[] LidarByteArray = lidarDataByte.nativeArray.ToArray();

                // https://answers.unity.com/questions/1581776/bytearray-impossible-to-convert-1.html
                // Vær obs på om systemene kjører big / small endian
                //var stringarray = System.BitConverter.ToSingle(LidarByteArray,12);

                //Debug.Log("Intensity in bytes: " + stringarray.ToString());

            }

        }



        private IEnumerator UpdateLidarLines()
        {
            // Setting up Compute Buffers

            int kernelHandle = computeShader.FindKernel("CSMain");

            SetCameraBuffers(computeShader, "CSMain", lidarCameras);

            computeShader.SetTexture(kernelHandle, "sphericalMaskImage", sphericalMaskImage);
            computeShader.SetBuffer(kernelHandle, "sphericalPixelCoordinates", pixelCoordinatesBuffer);

            UnifiedArray<Vector3> particleUnifiedArray = new UnifiedArray<Vector3>(NrOfCameras * NrOfLasers * LidarHorisontalRes, sizeof(float) * 3, "lines");
            particleUnifiedArray.SetBuffer(computeShader, "CSMain");

            lidarDataByte = new UnifiedArray<byte>(NrOfCameras * NrOfLasers * LidarHorisontalRes, sizeof(float) * 6, "LidarData");
            lidarDataByte.SetBuffer(computeShader, "CSMain");

            lidarData = new UnifiedArray<LidarFieldData>(NrOfCameras * NrOfLasers * LidarHorisontalRes, sizeof(float) * 6, "LidarData2");
            lidarData.SetBuffer(computeShader, "CSMain");

            while (true)
            {
                computeShader.Dispatch(kernelHandle, (int)Mathf.Ceil((float)NrOfCameras * (float)NrOfLasers * (float)LidarHorisontalRes / 1024.0f), 1, 1);

                var request = AsyncGPUReadback.Request(particleUnifiedArray.buffer);
                yield return new WaitUntil(() => request.done);
                particleUnifiedArray.nativeArray = request.GetData<Vector3>();
                if (ShowPointCloud) { particleMesh.SetVertices(particleUnifiedArray.nativeArray); }

                var request2 = AsyncGPUReadback.Request(lidarDataByte.buffer);
                yield return new WaitUntil(() => request2.done);
                lidarDataByte.nativeArray = request2.GetData<byte>();
                byte[] LidarByteArray = lidarDataByte.nativeArray.ToArray();

                var request3 = AsyncGPUReadback.Request(lidarData.buffer);
                yield return new WaitUntil(() => request3.done);
                lidarData.nativeArray = request3.GetData<LidarFieldData>();

                // https://answers.unity.com/questions/1581776/bytearray-impossible-to-convert-1.html
                // Vær obs på om systemene kjører big / small endian
                var stringarray = System.BitConverter.ToSingle(LidarByteArray, 0);
                Debug.Log("x in bytes: " + stringarray.ToString());


                Debug.Log("Intesnsity: " + lidarData.nativeArray[0].intensity.ToString());
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
                CameraObject.transform.localRotation = Quaternion.Euler(0, i * 360.0f / numbers, 0);
                CameraObject.transform.localPosition = new Vector3(0, 0, 0);
                //CameraObject.layer = LayerMask.NameToLayer(LidarLayer);
                CameraObject.AddComponent<Camera>();
                Camera cam = CameraObject.GetComponent<Camera>();

                if (cam.targetTexture == null)
                {
                    var depthBuffer = new RenderTexture(Width, Height, 16, format);//,// format);
                    depthBuffer.enableRandomWrite = true;
                    if (DepthBufferPrecision == DepthPrecision.bit16)
                    {
                        depthBuffer.depth = 16;
                    }
                    else if (DepthBufferPrecision == DepthPrecision.bit24)
                    {
                        depthBuffer.depth = 16;

                    }
                    else if (DepthBufferPrecision == DepthPrecision.bit32)
                    {
                        depthBuffer.depth = 32;
                    }
                    cam.targetTexture = depthBuffer;
                }

                cam.usePhysicalProperties = false;

                // Projection Matrix Setup
                cam.aspect = Mathf.Tan(Mathf.PI / numbers) / Mathf.Tan(verticalAngle * Mathf.Deg2Rad / 2.0f);
                cam.fieldOfView = Camera.HorizontalToVerticalFieldOfView(360.0f / numbers, cam.aspect);
                cam.farClipPlane = MaxDistance;
                cam.enabled = false;
                cam.nearClipPlane = MinDistance;
                //Matrix4x4 Proj = new Matrix4x4();
                //Proj = cam.projectionMatrix;
                //Proj.m22 *= -1;
                //Proj.m23 *= -1;
                //Proj.m32 *= -1;
                //cam.projectionMatrix = Proj;
                Cameras[i] = cam;
            }
            return Cameras;
        }

        private void SetCameraBuffers(ComputeShader shader, string kernelName, Camera[] cameras)
        {
            int kernelHandle = shader.FindKernel(kernelName);

            for (int i = 0; i < cameras.Length; i++)
            {
                Quaternion angle = Quaternion.Euler(0, i * 360.0f / cameras.Length, 0);
                Matrix4x4 m = Matrix4x4.Rotate(angle);

                computeShader.SetTexture(kernelHandle, "depthImage" + i.ToString(), cameras[i].targetTexture);
                computeShader.SetMatrix("CameraRotationMatrix" + i.ToString(), m);
            }
        }

        private NativeArray<Vector2> SphericalPixelCoordinates(int nrOfLasers, int horisontalRes)
        {
            int kernelHandle = computeShader.FindKernel("SphericalPixelCoordinates");

            // Create mask image
            int dataSize = 24;
            if (sphericalMask != null)
            {
                sphericalMaskImage = new RenderTexture(WidthRes, HeightRes, dataSize);
                // sphericalMaskImage.enableRandomWrite = true;
                sphericalMaskImage.Create();
                computeShader.SetTexture(kernelHandle, "sphericalMaskImage", sphericalMaskImage);
            }

            // Debug Vector

            ComputeBuffer debugBuffer = new ComputeBuffer(nrOfLasers * horisontalRes, sizeof(float));
            float[] array = new float[nrOfLasers * horisontalRes];
            debugBuffer.SetData(array);
            computeShader.SetBuffer(kernelHandle, "Debug_vector", debugBuffer);


            // Create pixel coordinates
            dataSize = sizeof(int) * 2;
            pixelCoordinatesBuffer = new ComputeBuffer(nrOfLasers * horisontalRes, dataSize);
            NativeArray<Vector2> pixelCoordinates = new NativeArray<Vector2>(nrOfLasers * horisontalRes, Allocator.Temp, NativeArrayOptions.ClearMemory);
            pixelCoordinatesBuffer.SetData(pixelCoordinates);
            computeShader.SetBuffer(kernelHandle, "sphericalPixelCoordinates", pixelCoordinatesBuffer);

            // Wait for data from GPU
            int Batch = (int)Mathf.Ceil((float)nrOfLasers * (float)horisontalRes / 1024.0f);
            //Debug.Log("Nr of batches: " + Batch.ToString());
            computeShader.Dispatch(kernelHandle, Batch, 1, 1);
            debugBuffer.GetData(array);
            //foreach(float element in array)
            //{
            //Debug.Log(180*element/Mathf.PI);
            //  Debug.Log(element);
            //}

            //var request = AsyncGPUReadback.Request(pixelCoordinatesBuffer);
            //while (!request.done) { Debug.Log("Fuck"); }
            //pixelCoordinates = request.GetData<Vector2>();


            // Cleanup and return
            debugBuffer.Release();
            debugBuffer.Dispose();
            //pixelCoordinatesBuffer.Release();
            //pixelCoordinatesBuffer.Dispose();

            return pixelCoordinates;
        }

        private void SetupLidarPointCloud()
        {
            int[] indices = new int[LidarHorisontalRes * NrOfCameras * NrOfLasers];
            //int[] indices = new int[HeightRes * NrOfCameras * WidthRes];
            indices = ArrayAllocator(LidarHorisontalRes * NrOfCameras * NrOfLasers);
            //indices = ArrayAllocator(HeightRes * NrOfCameras * WidthRes);
            particleMesh = new Mesh { vertices = new Vector3[LidarHorisontalRes * NrOfCameras * NrOfLasers] };
            //particleMesh = new Mesh { vertices = new Vector3[HeightRes * NrOfCameras * WidthRes] };

            particleMesh.SetIndices(indices, MeshTopology.Points, 0);

            GameObject obj = CreateMeshObject();

            obj.GetComponent<MeshFilter>().mesh = particleMesh;
            obj.GetComponent<Renderer>().material = particleMaterial;
        }

        private GameObject CreateMeshObject() {
            GameObject obj = new GameObject();
            obj.name = "ParticleMesh";
            obj.transform.SetParent(transform);
            obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            obj.transform.localPosition = new Vector3(0, 0, 0);
            obj.layer = LayerMask.NameToLayer(LidarLayer);

            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            return obj;
        }

        private int[] ArrayAllocator(int length)
        {
            int kernel = computeShader.FindKernel("QuickArrayAllocation");

            // Set up buffers to GPU
            ComputeBuffer arrayBuffer = new ComputeBuffer(length, sizeof(int));
            int[] array = new int[length];
            arrayBuffer.SetData(array);
            computeShader.SetBuffer(kernel, "array", arrayBuffer);

            // Wait for data from GPU
            computeShader.Dispatch(kernel, (int)Mathf.Ceil((float)length / 1024.0f), 1, 1);
            arrayBuffer.GetData(array);

            // Cleanup and return
            arrayBuffer.Release();
            arrayBuffer.Dispose();
            return array;
        }
        void OnEnable()
        {
            RenderPipelineManager.endFrameRendering += EndCameraRendering;
        }

        void OnDisable()
        {
            RenderPipelineManager.endFrameRendering -= EndCameraRendering;
            if (particleUnifiedArray.buffer != null) { particleUnifiedArray.buffer.Release(); particleUnifiedArray.buffer.Dispose(); }
            if (lidarDataByte.buffer != null) { lidarDataByte.buffer.Release(); lidarDataByte.buffer.Dispose(); }
            if (lidarData.buffer != null) { lidarData.buffer.Release(); lidarData.buffer.Dispose(); }
        }

        private void OnDestroy()
        {
            if (particleUnifiedArray.buffer != null) { particleUnifiedArray.buffer.Release(); }
            if (lidarDataByte.buffer != null) { lidarDataByte.buffer.Release(); }
            if (lidarData.buffer != null) { lidarData.buffer.Release(); }

        }

        void EndCameraRendering(ScriptableRenderContext context, Camera[] cam)
        {
            if (sphericalMaskImage != null)
            {
                Graphics.Blit(sphericalMaskImage, sphericalMask);
            }
        }
    }
//}