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

        new Camera camera;
        RenderTextureFormat renderTextureFormat = RenderTextureFormat.Default;
        TextureFormat textureFormat = TextureFormat.RGB24;

        [Space]
        [Header("Camera Parameters")]
        public int ImageWidth = 1920;
        public int ImageHeight = 1080;
        void Start()
        {
            camera = gameObject.GetComponent<Camera>();
        }

        public byte[] Data { get; private set; } = new byte[0];

        protected override void SampleSensor()
        {
            camera.targetTexture = RenderTexture.GetTemporary(ImageWidth, ImageHeight, 16);
            RenderTexture r = camera.targetTexture;
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = r;
            camera.Render();
            AsyncGPUReadback.Request(camera.targetTexture, 0, textureFormat, ReadbackCompleted);
            RenderTexture.ReleaseTemporary(r);
            camera.targetTexture = null;
            RenderTexture.active = null;
        }

        void ReadbackCompleted(AsyncGPUReadbackRequest request)
        {
            Data = request.GetData<byte>().ToArray();
            hasData = true;
        }
    }
}
