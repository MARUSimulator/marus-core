using UnityEngine;
using System.Collections;

using UnityEngine.Rendering;

using Labust.Sensors.Core.ZBuffer;

namespace Labust.Sensors.Core
{
    public class SphericalProjectionFilter: MonoBehaviour
    {
        public ComputeShader computeFilter;
        public UnifiedArray<Vector2> filterCoordinates;
        public RenderTexture SphericalProjectionFilterImage;
        private RenderTexture SphericalProjectionFilterMask;
        private CameraFrustum frustum;
        private int sphericalWidthRes;
        private int sphericalHeightRes;
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
        private UnifiedArray<Vector2> SphericalPixelCoordinates(int N_theta, int N_phi)
        {
            UnifiedArray<float> debugUnifiedArray = new UnifiedArray<float>(sphericalWidthRes * sphericalHeightRes, sizeof(float), projectionFilterDebug);
            debugUnifiedArray.SetBuffer(computeFilter, projectionFilterKernel);

            UnifiedArray<Vector2> pixelCoordinates = new UnifiedArray<Vector2>(sphericalWidthRes * sphericalHeightRes, sizeof(uint) * 2, projectionFilterBuffer);
            pixelCoordinates.SetBuffer(computeFilter, projectionFilterKernel);

            //Debug.Log(frustum._cameraMatrix);

            computeFilter.SetMatrix("CameraMatrix", frustum._cameraMatrix);
            computeFilter.SetInt("N_W",frustum._pixelWidth);
            computeFilter.SetInt("N_H",frustum._pixelHeight);
            computeFilter.SetInt("N_theta", sphericalWidthRes);
            computeFilter.SetInt("N_phi", sphericalHeightRes);

            computeFilter.SetFloat("VFOV_c", frustum._verticalAngle);
            computeFilter.SetFloat("VFOV_s", frustum._verticalSideAngles);
            computeFilter.SetFloat("HFOV_c", frustum._horisontalAngle);
            computeFilter.SetFloat("HFOV_s", frustum._horisontalAngle);

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

            RenderTexture sphericalMaskImage = new RenderTexture(frustum._pixelWidth, frustum._pixelHeight, dataSize);
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
