using UnityEngine;
using System.Collections;

using UnityEngine.Rendering;
using System;

namespace Labust.Sensors.Core
{
    public class SphericalProjectionFilter: MonoBehaviour
    {
        public ComputeShader computeFilter;
        public ComputeBufferDataExtractor<Vector2> filterCoordinates;
        public RenderTexture SphericalProjectionFilterImage;
        RenderTexture SphericalProjectionFilterMask;
        CameraFrustum frustum;
        int sphericalWidthRes;
        int sphericalHeightRes;
        public void SetupSphericalProjectionFilter(int N_theta, int N_phi, CameraFrustum cameraFrustum)
        {
            sphericalWidthRes = N_theta;
            sphericalHeightRes = N_phi;
            frustum = cameraFrustum;

            filterCoordinates = SphericalPixelCoordinates(N_theta, N_phi);
            SphericalProjectionFilterMask = SphericalPixelCoordinatesImage();
        }

        private string projectionFilterKernel = "projectionFilterKernel";
        private string projectionFilterBuffer = "projectionFilterBuffer";
        private string projectionFilterDebug = "projectionFilterDebug";
        private ComputeBufferDataExtractor<Vector2> SphericalPixelCoordinates(int N_theta, int N_phi)
        {
            var debugUnifiedArray = new ComputeBufferDataExtractor<float>(sphericalWidthRes * sphericalHeightRes, sizeof(float), projectionFilterDebug);
            debugUnifiedArray.SetBuffer(computeFilter, projectionFilterKernel);

            var pixelCoordinates = new ComputeBufferDataExtractor<Vector2>(sphericalWidthRes * sphericalHeightRes, sizeof(uint) * 2, projectionFilterBuffer);
            pixelCoordinates.SetBuffer(computeFilter, projectionFilterKernel);

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
            pixelCoordinates.SynchUpdate(computeFilter, projectionFilterKernel);

            debugUnifiedArray.Delete();

            return pixelCoordinates;
        }

        private string projectionFilterImageKernel = "projectionFilterImageKernel";
        private string projectionFilterImageBuffer = "projectionFilterImageBuffer";
        public RenderTexture SphericalPixelCoordinatesImage()
        {
            int dataSize = 24;

            int kernelHandle = computeFilter.FindKernel(projectionFilterImageKernel);

            RenderTexture sphericalMaskImage = new RenderTexture(frustum.pixelWidth, frustum.pixelHeight, dataSize);
            sphericalMaskImage.enableRandomWrite = true;
            sphericalMaskImage.Create();


            filterCoordinates.SetBuffer(computeFilter, projectionFilterImageKernel);
            computeFilter.SetTexture(kernelHandle, projectionFilterImageBuffer, sphericalMaskImage);

            computeFilter.Dispatch(kernelHandle, (int)Mathf.Ceil((float)sphericalWidthRes * (float)sphericalHeightRes / 1024.0f), 1, 1);

            return sphericalMaskImage;
        }

        void OnEnable()
        {
            RenderPipelineManager.endFrameRendering += EndCameraRendering;
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        }

        private void BeginCameraRendering(ScriptableRenderContext arg1, Camera arg2)
        {
            int kernelHandle = computeFilter.FindKernel(projectionFilterImageKernel);
            computeFilter.Dispatch(kernelHandle, (int)Mathf.Ceil((float)sphericalWidthRes * (float)sphericalHeightRes / 1024.0f), 1, 1);
        }

        void OnDisable()
        {
            RenderPipelineManager.endFrameRendering -= EndCameraRendering;
        }

        void EndCameraRendering(ScriptableRenderContext context, Camera[] cam)
        {
            if (SphericalProjectionFilterImage != null)
            {
                Graphics.Blit(SphericalProjectionFilterMask, SphericalProjectionFilterImage);
            }
        }
    }
}
