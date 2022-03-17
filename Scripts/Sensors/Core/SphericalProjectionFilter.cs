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
using System.Collections;

using UnityEngine.Rendering;
using System;
using System.Linq;

namespace Labust.Sensors.Core
{
    public class SphericalProjectionFilter: MonoBehaviour
    {
        public ComputeShader computeFilter;
        public ComputeBufferDataExtractor<Vector2Int> filterCoordinates;

        [Header("Debug")]
        public RenderTexture SphericalProjectionFilterMask;
        CameraFrustum frustum;
        int sphericalWidthRes;
        int sphericalHeightRes;
        public void SetupSphericalProjectionFilter(int N_theta, int N_phi, CameraFrustum cameraFrustum)
        {
            sphericalWidthRes = N_theta;
            sphericalHeightRes = N_phi;
            frustum = cameraFrustum;

            filterCoordinates = SphericalPixelCoordinates();
            SphericalProjectionFilterMask = SphericalPixelCoordinatesImage();
        }

        // There are unknown problems with gpu read and write in multiple kernels to the same buffer
        // Fix is to have separate buffers for read and write. Read - GPU reads from; Write - GPU writes to
        private string projectionFilterBufferRead = "projectionFilterBufferRead";
        private string projectionFilterBuffer = "projectionFilterBuffer";
        private string projectionFilterDebug = "projectionFilterDebug";
        private string projectionFilterKernel = "projectionFilterKernel";


        private ComputeBufferDataExtractor<Vector2Int> SphericalPixelCoordinates()
        {
            var debugUnifiedArray = new ComputeBufferDataExtractor<float>(sphericalWidthRes * sphericalHeightRes, sizeof(float), projectionFilterDebug);
            debugUnifiedArray.SetBuffer(computeFilter, projectionFilterKernel);

            filterCoordinates = new ComputeBufferDataExtractor<Vector2Int>(sphericalWidthRes * sphericalHeightRes, sizeof(int) * 2, projectionFilterBuffer);
            filterCoordinates.SetBuffer(computeFilter, projectionFilterKernel);

            //Debug.Log(frustum._cameraMatrix);

            computeFilter.SetMatrix("CameraMatrix", frustum.cameraMatrix);
            computeFilter.SetInt("N_W",frustum.pixelWidth);
            computeFilter.SetInt("N_H",frustum.pixelHeight);
            computeFilter.SetInt("N_theta", sphericalWidthRes);
            computeFilter.SetInt("N_phi", sphericalHeightRes);

            computeFilter.SetFloat("VFOV_c", frustum.verticalAngle);
            computeFilter.SetFloat("VFOV_s", frustum.verticalSideAngles);
            computeFilter.SetFloat("HFOV_c", frustum.horisontalAngle);
            computeFilter.SetFloat("HFOV_s", frustum.horisontalAngle);

            debugUnifiedArray.SynchUpdate(computeFilter, projectionFilterKernel);
            filterCoordinates.SynchUpdate(computeFilter, projectionFilterKernel);
            debugUnifiedArray.Delete();

            return filterCoordinates;
        }

        private string projectionFilterImageKernel = "projectionFilterImageKernel";
        private string projectionFilterImageBuffer = "projectionFilterImageBuffer";
        public RenderTexture SphericalPixelCoordinatesImage()
        {
            int kernelHandle = computeFilter.FindKernel(projectionFilterImageKernel);

            RenderTexture sphericalMaskImage = new RenderTexture(frustum.pixelWidth, frustum.pixelHeight, 24);
            sphericalMaskImage.enableRandomWrite = true;
            sphericalMaskImage.Create();

            filterCoordinates.SetBuffer(computeFilter, projectionFilterImageKernel);
            computeFilter.SetTexture(kernelHandle, projectionFilterImageBuffer, sphericalMaskImage);

            computeFilter.Dispatch(kernelHandle, (int)Mathf.Ceil((float)sphericalWidthRes * (float)sphericalHeightRes / 1024.0f), 1, 1);
            return sphericalMaskImage;
        }

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        }

        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            // filterCoordinates.Delete();
        }


        void OnDestroy()
        {
            filterCoordinates.Delete();
        }

        private void BeginCameraRendering(ScriptableRenderContext arg1, Camera arg2)
        {
            int kernelHandle = computeFilter.FindKernel(projectionFilterImageKernel);
            computeFilter.Dispatch(kernelHandle, (int)Mathf.Ceil((float)sphericalWidthRes * (float)sphericalHeightRes / 1024.0f), 1, 1);
            // float[] data = debug.data;
        }

    }
}
