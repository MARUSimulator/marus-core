﻿using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Labust.Sensors.Core
{
    /// <summary>
    /// STIL DOES NOT WORK ON UNITY 2020. Works on Unity 2019
    /// </summary>
    class LidarCP : CustomPass
    {
        public GameObject lidar;
        private Camera[] cameras;
        private ShaderTagId[] shaderTags;
        private RenderTexture[] handles;

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            shaderTags = new ShaderTagId[2]{
                new ShaderTagId("DepthOnly"),
                new ShaderTagId("DepthForwardOnly"),
            };
            var lidarObj = lidar.GetComponent<LidarScript>();
            cameras = lidarObj.lidarCameras;
            handles = new RenderTexture[cameras.Length];
            if (cameras != null && cameras.Length > 0)
            {
                for (int i = 0; i < cameras.Length; i++)
                {
                    var quad = GameObject.Find("Quad" + (i+1).ToString());
                    var c = quad.GetComponent<MeshRenderer>();
                    // handles[i] = RTHandles.Alloc(cameras[i].targetTexture);
                    handles[i] = new RenderTexture(cameras[i].targetTexture);
                    c.material.mainTexture = cameras[i].targetTexture;
                }
            }
        }

        protected override void Execute(CustomPassContext context)
        {
            // for (int i = 0; i < cameras.Length; i++)
            // {
            //     Camera bakingCamera = cameras[i];
            //     RenderTexture targetTexture = bakingCamera.targetTexture;
            //     bakingCamera.TryGetCullingParameters(out var cullingParams);
            //     cullingParams.cullingOptions = CullingOptions.ShadowCasters;
            //     var cullingResult = context.renderContext.Cull(ref cullingParams);

            //     var result = new RendererListDesc(shaderTags, context.cullingResults, bakingCamera)
            //     {
            //         rendererConfiguration = PerObjectData.None,
            //         renderQueueRange = RenderQueueRange.all,
            //         sortingCriteria = SortingCriteria.BackToFront,
            //         excludeObjectMotionVectors = false,
            //         layerMask = 0,
            //     };

            //     Matrix4x4 cameraProjMatrix = cameras[0].projectionMatrix;
            //     //cameraProjMatrix.m22 *= -1;
            //     //cameraProjMatrix.m23 *= -1;
            //     //cameraProjMatrix.m32 *= -1;
            //     //bakingCamera.projectionMatrix = cameraProjMatrix;
            //     //var p = cameraProjMatrix;
            //     var p = GL.GetGPUProjectionMatrix(cameraProjMatrix, true);
            //     //Debug.Log(p);
            //     //Debug.Log(cameraProjMatrix);

            //     Matrix4x4 scaleMatrix = Matrix4x4.identity;
            //     scaleMatrix.m22 = -1.0f;
            //     var v = scaleMatrix * bakingCamera.transform.localToWorldMatrix.inverse;
            //     var vp = p * v;

            //     var cmd = context.cmd;
            //     cmd.SetGlobalMatrix("_ViewMatrix", v);
            //     cmd.SetGlobalMatrix("_InvViewMatrix", v.inverse);
            //     cmd.SetGlobalMatrix("_ProjMatrix", p);
            //     cmd.SetGlobalMatrix("_InvProjMatrix", p.inverse);
            //     cmd.SetGlobalMatrix("_ViewProjMatrix", vp);
            //     cmd.SetGlobalMatrix("_InvViewProjMatrix", vp.inverse);
            //     cmd.SetGlobalMatrix("_CameraViewProjMatrix", vp);
            //     cmd.SetGlobalVector("_WorldSpaceCameraPos", Vector3.zero);

            //     CoreUtils.SetRenderTarget(cmd, targetTexture.colorBuffer, ClearFlag.Depth);
            //     CoreUtils.DrawRendererList(context.renderContext, cmd, RendererList.Create(result));
            // }
        }
    }
}