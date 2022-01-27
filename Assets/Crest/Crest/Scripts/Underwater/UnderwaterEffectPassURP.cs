// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

#if CREST_URP

namespace Crest
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    internal class UnderwaterEffectPassURP : ScriptableRenderPass
    {
        const string SHADER_UNDERWATER_EFFECT = "Hidden/Crest/Underwater/Underwater Effect URP";
        static readonly int sp_TemporaryRT = Shader.PropertyToID("_TemporaryRT");
        static readonly int sp_CameraForward = Shader.PropertyToID("_CameraForward");

        readonly PropertyWrapperMaterial _underwaterEffectMaterial;
        Material _blitMaterial;
        RenderTargetIdentifier _sourceIdentifierRT;
        RenderTargetIdentifier _temporaryIdentifierRT = new RenderTargetIdentifier(sp_TemporaryRT, 0, CubemapFace.Unknown, -1);
        bool _firstRender = true;

        static UnderwaterEffectPassURP s_instance;
        UnderwaterRenderer _underwaterRenderer;

        public UnderwaterEffectPassURP()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            _underwaterEffectMaterial = new PropertyWrapperMaterial(SHADER_UNDERWATER_EFFECT);
            // Located at: com.unity.render-pipelines.universal/Shaders/Utils/Blit.shader
            _blitMaterial = CoreUtils.CreateEngineMaterial("Hidden/Universal Render Pipeline/Blit");
        }

        ~UnderwaterEffectPassURP()
        {
            CoreUtils.Destroy(_underwaterEffectMaterial.material);
            CoreUtils.Destroy(_blitMaterial);
        }

        public static void Enable(UnderwaterRenderer underwaterRenderer)
        {
            if (s_instance == null)
            {
                s_instance = new UnderwaterEffectPassURP();
            }

            s_instance._underwaterRenderer = underwaterRenderer;

            RenderPipelineManager.beginCameraRendering -= EnqueuePass;
            RenderPipelineManager.beginCameraRendering += EnqueuePass;
        }

        public static void Disable()
        {
            RenderPipelineManager.beginCameraRendering -= EnqueuePass;
        }

        static void EnqueuePass(ScriptableRenderContext context, Camera camera)
        {
            if (!s_instance._underwaterRenderer.IsActive)
            {
                return;
            }

            // Only support main camera for now.
            if (!ReferenceEquals(OceanRenderer.Instance.ViewCamera, camera))
            {
                return;
            }

            // Only support game cameras for now.
            if (camera.cameraType != CameraType.Game)
            {
                return;
            }

            if (camera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData))
            {
                cameraData.scriptableRenderer.EnqueuePass(s_instance);
            }
        }

        // Called before Configure.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _sourceIdentifierRT = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.GetTemporaryRT(sp_TemporaryRT, renderingData.cameraData.cameraTargetDescriptor);
        }

        // Called before Execute.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(_sourceIdentifierRT);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(sp_TemporaryRT);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

            // Ensure legacy underwater fog is disabled.
            if (_firstRender)
            {
                OceanRenderer.Instance.OceanMaterial.DisableKeyword("_OLD_UNDERWATER");
            }

            CommandBuffer commandBuffer = CommandBufferPool.Get("Underwater Effect");

            UnderwaterRenderer.UpdatePostProcessMaterial(
                camera,
                _underwaterEffectMaterial,
                _underwaterRenderer._sphericalHarmonicsData,
                _underwaterRenderer._meniscus,
                _firstRender || _underwaterRenderer._copyOceanMaterialParamsEachFrame,
                _underwaterRenderer._debug._viewOceanMask,
                _underwaterRenderer._filterOceanData
            );

            // Required for XR SPI as forward vector in matrix is incorrect.
            _underwaterEffectMaterial.material.SetVector(sp_CameraForward, camera.transform.forward);

            if (renderingData.cameraData.xrRendering && XRGraphics.stereoRenderingMode == XRGraphics.StereoRenderingMode.SinglePassInstanced)
            {
                commandBuffer.SetRenderTarget(_temporaryIdentifierRT, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                // This works with Unity's copy color shader.
                commandBuffer.DrawProcedural(Matrix4x4.identity, _blitMaterial, 0, MeshTopology.Quads, 4, 1, null);

                commandBuffer.SetGlobalTexture(UnderwaterRenderer.sp_CrestCameraColorTexture, _temporaryIdentifierRT);
                commandBuffer.SetRenderTarget(_sourceIdentifierRT);
                // For XR SPI, this is the recommended approach as Blit does not work:
                // https://docs.google.com/document/d/1jdCu7_TqvRqkYFWa-EueSZXzvRZ3V9dZ9oXNKGo2cJY
                commandBuffer.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _underwaterEffectMaterial.material);
            }
            else
            {
                commandBuffer.SetGlobalTexture(UnderwaterRenderer.sp_CrestCameraColorTexture, _sourceIdentifierRT);
                // We cannot read and write using the same texture so use a temporary texture as an intermediary.
                Blit(commandBuffer, _sourceIdentifierRT, sp_TemporaryRT, _underwaterEffectMaterial.material);
                Blit(commandBuffer, sp_TemporaryRT, _sourceIdentifierRT);
            }

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);

            _firstRender = false;
        }
    }
}

#endif // CREST_URP
