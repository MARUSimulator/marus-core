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
#if UNITY_EDITOR
using UnityEditor;
#endif
using Marus.Sensors.Core;

namespace Marus.Sensors
{
    [RequireComponent(typeof(Camera))]
    /// <summary>
    /// Camera sensor implementation
    /// </summary>
    public class CameraSensor : SensorBase
    {
        [ReadOnly]
        public int ImageWidth = 1920;

        [ReadOnly]
        public int ImageHeight = 1080;

        Camera _camera;
        RenderTexture _tmpTexture;
        TextureFormat _textureFormat = TextureFormat.RGB24;
        Texture2D _texture;

        [HideInInspector]
        public byte[] Data;

        void Start()
        {

#if UNITY_EDITOR
            string[] res = UnityStats.screenRes.Split('x');
            ImageWidth =  int.Parse(res[0]);
            ImageHeight = int.Parse(res[1]);
#else
            ImageWidth =  Screen.width;
            ImageHeight = Screen.height;
#endif

            _camera = GetComponent<Camera>();
            _camera.enabled = false;
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
            if (_camera.targetTexture != null)
            {
                RenderTexture.ReleaseTemporary(_camera.targetTexture);
            }
            _camera.targetTexture = RenderTexture.GetTemporary(ImageWidth, ImageHeight, 16);
            RenderTexture.active = _camera.targetTexture;
            _camera.Render();
            AsyncGPUReadback.Request(_camera.targetTexture, 0, _textureFormat, ReadbackCompleted);
        }

        void ReadbackCompleted(AsyncGPUReadbackRequest request)
        {
            Data = request.GetData<byte>().ToArray();
            hasData = true;
        }
    }
}
