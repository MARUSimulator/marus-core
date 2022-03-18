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

using UnityEngine;
using UnityEngine.Rendering;
using Marus.Sensors.Core;

namespace Marus.Sensors
{
    [RequireComponent(typeof(Camera))]
    /// <summary>
    /// Camera sensor implementation
    /// </summary>
    public class CameraSensor : SensorBase
    {
        public RenderTexture _cameraBuffer { get; set; }
        public RenderTexture SampleCameraImage;
        public ComputeShader cameraShader;
        public int ImageCrop = 4;
        public bool SynchronousUpdate = false;

        new Camera camera;
        ComputeBufferDataExtractor<byte> cameraData;
        RenderTextureFormat renderTextureFormat = RenderTextureFormat.Default;
        TextureFormat textureFormat = TextureFormat.RGB24;

        [Space]
        [Header("Camera Parameters")]
        public int PixelWidth = 2448;
        public int PixelHeight = 2048;
        public float FarPlane = 10000f;
        public float NearPlane = 0.08f;
        public float focalLengthMilliMeters = 5.5f;
        public float pixelSizeInMicroMeters = 3.45f;

        void Start()
        {
            CameraSetup();

            int kernelIndex = cameraShader.FindKernel("CSMain");
            cameraData = new ComputeBufferDataExtractor<byte>(PixelHeight * PixelWidth, sizeof(float) * 3, "CameraData");
            cameraData.SetBuffer(cameraShader, "CSMain");
            cameraShader.SetTexture(kernelIndex, "RenderTexture", camera.targetTexture);
            cameraShader.SetInt("Width", PixelWidth / ImageCrop);
            cameraShader.SetInt("Height", PixelHeight / ImageCrop);
        }

        public byte[] Data { get; private set; } = new byte[0];

        protected override void SampleSensor()
        {
            if (SynchronousUpdate)
            {
                cameraData.SynchUpdate(cameraShader, "CSMain");
                Data = cameraData.data;
                hasData = true;
            }
            else
            {
                AsyncGPUReadback.Request(camera.activeTexture, 0, textureFormat, ReadbackCompleted);
            }
        }

        void ReadbackCompleted(AsyncGPUReadbackRequest request)
        {
            Data = request.GetData<byte>().ToArray();
            hasData = true;
        }

        byte[] RenderTextureToBinary(Camera cam)
        {
            // The Render Texture in RenderTexture.active is the one
            // that will be read by ReadPixels.
            var currentRT = RenderTexture.active;
            RenderTexture.active = cam.targetTexture;

            // Make a new texture and read the active Render Texture into it.
            Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, textureFormat, false, true);
            image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
            image.Apply();

            // Replace the original active Render Texture.
            RenderTexture.active = currentRT;
            return image.EncodeToPNG();
        }

        private void CameraSetup()
        {
            CameraFrustum frustums = new CameraFrustum(PixelWidth, PixelHeight, FarPlane, NearPlane, focalLengthMilliMeters, pixelSizeInMicroMeters);
            _cameraBuffer = new RenderTexture(PixelWidth / ImageCrop, PixelHeight / ImageCrop, 24, renderTextureFormat, 0);

            camera = gameObject.GetComponent<Camera>();
            camera.usePhysicalProperties = false;
            camera.targetTexture = _cameraBuffer;

            camera.aspect = frustums.aspectRatio;//Mathf.Tan(Mathf.PI / numbers) / Mathf.Tan(frustums._verticalAngle / 2.0f);
            camera.fieldOfView = frustums.verticalAngle * Mathf.Rad2Deg;//Camera.HorizontalToVerticalFieldOfView(360.0f / numbers, cam.aspect);
            camera.farClipPlane = frustums.farPlane;
            camera.nearClipPlane = frustums.nearPlane;
            //camera.enabled = false;
        }


        void OnDestroy()
        {
            cameraData.Delete();
        }

    }
}