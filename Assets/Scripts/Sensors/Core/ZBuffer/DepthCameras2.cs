using UnityEngine;
using System.Collections;



namespace Labust.Sensors.Core
{
    public class DepthCameras2
    {
        public enum BufferPrecision // your custom enumeration
        {
            bit16,
            bit24,
            bit32
        };

        private string CameraTag = "DepthCam";
        private CameraFrustum frustums;
        public Camera[] cameras;


        public DepthCameras2(int cameraNumbers, CameraFrustum cameraFrustums, Transform transform)
        {
            frustums = cameraFrustums;
            cameras = SpawnDepthCameras(cameraNumbers, transform);
        }
        public DepthCameras2(int cameraNumbers, CameraFrustum cameraFrustums, Transform transform, ComputeShader shader, string kernelName)
        {
            frustums = cameraFrustums;
            cameras = SpawnDepthCameras(cameraNumbers, transform);
            SetCameraBuffers(shader, kernelName);
        }

        private BufferPrecision DepthBufferPrecision = BufferPrecision.bit24;

        private Camera[] SpawnDepthCameras(int numbers, Transform transform)
        {
            RenderTextureFormat format = RenderTextureFormat.Depth;
            Camera[] Cameras = new Camera[numbers];
            for (int i = 0; i < numbers; i++)
            {
                GameObject CameraObject = new GameObject();
                CameraObject.name = CameraTag + i;
                CameraObject.transform.SetParent(transform);
                CameraObject.transform.localRotation = Quaternion.Euler(0, i * 360.0f / numbers, 0);
                CameraObject.transform.localPosition = new Vector3(0, 0, 0);
                //CameraObject.layer = LayerMask.NameToLayer(LidarLayer);
                CameraObject.AddComponent<Camera>();
                Camera cam = CameraObject.GetComponent<Camera>();

                if (cam.targetTexture == null)
                {
                    var depthBuffer = new RenderTexture(frustums.pixelWidth, frustums.pixelHeight, 16, format);//,// format);
                    depthBuffer.enableRandomWrite = true;
                    if (DepthBufferPrecision == BufferPrecision.bit16)
                    {
                        depthBuffer.depth = 16;
                    }
                    else if (DepthBufferPrecision == BufferPrecision.bit24)
                    {
                        depthBuffer.depth = 24;

                    }
                    else if (DepthBufferPrecision == BufferPrecision.bit32)
                    {
                        depthBuffer.depth = 32;
                    }
                    cam.targetTexture = depthBuffer;
                }

                cam.usePhysicalProperties = false;

                // Projection Matrix Setup
                cam.aspect = frustums.aspectRatio;//Mathf.Tan(Mathf.PI / numbers) / Mathf.Tan(frustums._verticalAngle / 2.0f);
                cam.fieldOfView = frustums.verticalAngle*Mathf.Rad2Deg;//Camera.HorizontalToVerticalFieldOfView(360.0f / numbers, cam.aspect);
                cam.farClipPlane = frustums.farPlane;
                cam.enabled = false;
                cam.nearClipPlane = frustums.nearPlane;
                Cameras[i] = cam;
                //Debug.Log(cam.projectionMatrix);
            }
            return Cameras;
        }

        public void SetCameraBuffers(ComputeShader shader, string kernelName)
        {
            int kernelHandle = shader.FindKernel(kernelName);
            shader.SetMatrix("inv_CameraMatrix", cameras[0].projectionMatrix.inverse);
            shader.SetMatrix("CameraMatrix", cameras[0].projectionMatrix);
            shader.SetInt("ImageWidthRes", frustums.pixelWidth);
            shader.SetInt("ImageHeightRes", frustums.pixelHeight);
            shader.SetFloat("VFOV_camera", frustums.verticalAngle);
            shader.SetFloat("HFOV_camera", frustums.horisontalAngle);
            shader.SetInt("NrOfImages", cameras.Length);

            for (int i = 0; i < cameras.Length; i++)
            {
                Quaternion angle = Quaternion.Euler(0, i * 360.0f / cameras.Length, 0);
                Matrix4x4 m = Matrix4x4.Rotate(angle);

                //Debug.Log("Camera depth texture set for: " + i.ToString());

                shader.SetTexture(kernelHandle, "depthImage" + i.ToString(), cameras[i].targetTexture);
                shader.SetMatrix("CameraRotationMatrix" + i.ToString(), m);
            }
        }

    }
}