using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Labust.Sensors.Core
{
    public static class DepthCameras
    {
        public enum BufferPrecision // your custom enumeration
        {
            bit16=16,
            bit24=24,
            bit32=32
        };

        static string CameraTag = "DepthCam";

        private static BufferPrecision DepthBufferPrecision = BufferPrecision.bit24;

        public static (Camera[], CameraFrustum) SpawnDepthCameras(Transform transform, int numCameras, int WidthRes, float farPlane, 
                float nearPlane, float verticalAngle)
        {
            var frustumTemplate = new CameraFrustum(WidthRes, farPlane, nearPlane, 2 * Mathf.PI / numCameras, verticalAngle * Mathf.Deg2Rad);
            RenderTextureFormat format = RenderTextureFormat.Depth;
            Camera[] Cameras = new Camera[numCameras];
            for (int i = 0; i < numCameras; i++)
            {
                GameObject CameraObject = new GameObject();
                CameraObject.name = CameraTag + i;
                CameraObject.transform.SetParent(transform);
                CameraObject.transform.localRotation = Quaternion.Euler(0, i * 360.0f / numCameras, 0);
                CameraObject.transform.localPosition = new Vector3(0, 0, 0);
                //CameraObject.layer = LayerMask.NameToLayer(LidarLayer);
                CameraObject.AddComponent<Camera>();
                CameraObject.AddComponent<HDAdditionalCameraData>();
                Camera cam = CameraObject.GetComponent<Camera>();
                if (cam.targetTexture == null)
                {
                    var depthBuffer = new RenderTexture(frustumTemplate.pixelWidth, frustumTemplate.pixelHeight, 16, format);
                    depthBuffer.depth = (int)DepthBufferPrecision;
                    cam.targetTexture = depthBuffer;
                }
                // cam.depth = -5;
                cam.usePhysicalProperties = false;
                // Projection Matrix Setup
                cam.aspect = frustumTemplate.aspectRatio;//Mathf.Tan(Mathf.PI / numbers) / Mathf.Tan(frustums._verticalAngle / 2.0f);
                cam.fieldOfView = frustumTemplate.verticalAngle*Mathf.Rad2Deg;//Camera.HorizontalToVerticalFieldOfView(360.0f / numbers, cam.aspect);
                cam.farClipPlane = frustumTemplate.farPlane;
                cam.enabled = true;
                cam.nearClipPlane = frustumTemplate.nearPlane;
                cam.depthTextureMode = DepthTextureMode.Depth;
                // cam.clearFlags = CameraClearFlags.Depth;
                Cameras[i] = cam;
                var hdCam = CameraObject.GetComponent<HDAdditionalCameraData>();
                hdCam.customRender += CustomRender;
            }
            return (Cameras, frustumTemplate);
        }

        public static void SetCameraBuffers(Camera[] cameras, CameraFrustum frustum, ComputeShader shader, string kernelName)
        {
            int kernelHandle = shader.FindKernel(kernelName);
            shader.SetMatrix("inv_CameraMatrix", cameras[0].projectionMatrix.inverse);
            shader.SetMatrix("CameraMatrix", cameras[0].projectionMatrix);
            shader.SetInt("ImageWidthRes", frustum.pixelWidth);
            shader.SetInt("ImageHeightRes", frustum.pixelHeight);
            shader.SetFloat("VFOV_camera", frustum.verticalAngle * Mathf.Rad2Deg);
            shader.SetFloat("HFOV_camera", frustum.horisontalAngle * Mathf.Rad2Deg);
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
        
        public static void CustomRender(ScriptableRenderContext context, HDCamera camera)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera.camera);
            context.SetupCameraProperties(camera.camera);
            var cameraBuffer = new CommandBuffer { name="Render Camera" + camera.camera.name};
            cameraBuffer.SetRenderTarget(camera.camera.targetTexture);
            cameraBuffer.ClearRenderTarget(true, false, Color.black);
        
            cameraBuffer.BeginSample("Render camera" + camera.camera.name);
            context.ExecuteCommandBuffer(cameraBuffer);
            cameraBuffer.Clear();

            camera.camera.TryGetCullingParameters(out var cullingParameters);
            var cullingResults = context.Cull(ref cullingParameters);

            cameraBuffer.SetRenderTarget(camera.camera.targetTexture);
            context.ExecuteCommandBuffer(cameraBuffer);
            cameraBuffer.Clear();

            var drawingSettings = new DrawingSettings();
            drawingSettings.SetShaderPassName(0, new ShaderTagId("DepthOnly"));
            drawingSettings.SetShaderPassName(1, new ShaderTagId("DepthForwardOnly"));
            var filteringSettings = new FilteringSettings(RenderQueueRange.all);
            context.SetupCameraProperties(camera.camera);
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            cameraBuffer.EndSample("Render camera" + camera.camera.name);
            context.ExecuteCommandBuffer(cameraBuffer);
            cameraBuffer.Clear();
            context.Submit();
        }
    }
}