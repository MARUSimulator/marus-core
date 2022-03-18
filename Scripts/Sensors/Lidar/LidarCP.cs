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
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using System.Reflection;

namespace Marus.Sensors.Core
{
    /// <summary>
    /// STIL DOES NOT WORK ON UNITY 2020. Works on Unity 2019
    /// </summary>
    class LidarCP : CustomPass
    {
        public GameObject lidar;
        Camera[] cameras;
        ShaderTagId[] shaderTags;
        FieldInfo cullingResultField;
        RTHandle[] rtHandles;

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            cullingResultField = typeof(CustomPassContext).GetField(nameof(CustomPassContext.cullingResults));
            shaderTags = new ShaderTagId[2]{
                new ShaderTagId("DepthOnly"),
                new ShaderTagId("DepthForwardOnly"),
            };
            var lidarObj = lidar.GetComponent<LidarScript>();
            cameras = lidarObj.lidarCameras;
            rtHandles = new RTHandle[cameras.Length];
            if (cameras != null && cameras.Length > 0)
            {
                using var handleSystem = new RTHandleSystem();
                for (int i = 0; i < cameras.Length; i++)
                {
                    var handle = handleSystem.Alloc(cameras[i].targetTexture);
                    rtHandles[i] = handle;
                    cameras[i].targetTexture = handle;
                    // cameras[i].targetTexture = handle;
                    var quad = GameObject.Find("Quad" + (i+1).ToString());
                    var c = quad.GetComponent<MeshRenderer>();
                    c.material.mainTexture = cameras[i].targetTexture;
                }
            }
        }

        protected override void Execute(CustomPassContext context)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].TryGetCullingParameters(out var cullingParams);
                cullingParams.cullingOptions = CullingOptions.ShadowCasters;

                // Assign the custom culling result to the context
                // so it'll be used for the following operations
                cullingResultField.SetValueDirect(__makeref(context), context.renderContext.Cull(ref cullingParams));
                var overrideDepthTest = new RenderStateBlock(RenderStateMask.Depth) { depthState = new DepthState(true, CompareFunction.LessEqual) };

                CoreUtils.SetRenderTarget(context.cmd, rtHandles[i], ClearFlag.Depth);
                CustomPassUtils.RenderDepthFromCamera(context, cameras[i], cameras[i].cullingMask, overrideRenderState: overrideDepthTest);

        // Sync baking camera aspect ratio with RT (see https://github.com/alelievr/HDRP-Custom-Passes/issues/24)
                // cameras[i].aspect = context.hdCamera.camera.aspect;
                // bakingCamera.pixelRect = ctx.hdCamera.camera.pixelRect;
                // cameras[i].targetTexturertHandles[i];

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
            }
        }
    }
}