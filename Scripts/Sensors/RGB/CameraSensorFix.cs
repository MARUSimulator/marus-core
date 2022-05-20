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
    public class CameraSensorFix : SensorBase
    {
        public RenderTexture _cameraBuffer { get; set; }
        public int ImageWidth = 1920;
        public int ImageHeight = 1080;

        Camera _camera;
        RenderTextureFormat _renderTextureFormat = RenderTextureFormat.Default;
        TextureFormat _textureFormat = TextureFormat.RGB24;
        Texture2D _texture;

        public byte[] Data;

        void Start()
        {
            _camera = GetComponent<Camera>();
            Data = new byte[ImageHeight*ImageWidth*3];
            _texture = new Texture2D
            (
                ImageWidth,
                ImageHeight,
                _textureFormat,
                false
            );
        }

        protected override void SampleSensor()
        {
            _camera.targetTexture = RenderTexture.GetTemporary(ImageWidth, ImageHeight, 16);
            RenderTexture r = _camera.targetTexture;
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = r;
            _camera.Render();
            AsyncGPUReadback.Request(_camera.targetTexture, 0, _textureFormat, ReadbackCompleted);
            RenderTexture.ReleaseTemporary(r);
            //_camera.targetTexture = null;
        }

        void ReadbackCompleted(AsyncGPUReadbackRequest request)
        {
            Data = request.GetData<byte>().ToArray();
            hasData = true;
        }
    }
}
