// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

#if CREST_URP

namespace Crest
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    internal class UnderwaterMaskPassURP : ScriptableRenderPass
    {
        const string SHADER_OCEAN_MASK = "Hidden/Crest/Underwater/Ocean Mask URP";

        readonly PropertyWrapperMaterial _oceanMaskMaterial;

        RenderTargetIdentifier _maskTarget = new RenderTargetIdentifier(UnderwaterRenderer.sp_CrestOceanMaskTexture, 0, CubemapFace.Unknown, -1);
        RenderTargetIdentifier _depthTarget = new RenderTargetIdentifier(UnderwaterRenderer.sp_CrestOceanMaskDepthTexture, 0, CubemapFace.Unknown, -1);

        static UnderwaterMaskPassURP s_instance;
        UnderwaterRenderer _underwaterRenderer;

        public UnderwaterMaskPassURP()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            _oceanMaskMaterial = new PropertyWrapperMaterial(SHADER_OCEAN_MASK);
        }

        ~UnderwaterMaskPassURP()
        {
            CoreUtils.Destroy(_oceanMaskMaterial.material);
        }

        public static void Enable(UnderwaterRenderer underwaterRenderer)
        {
            if (s_instance == null)
            {
                s_instance = new UnderwaterMaskPassURP();
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

            // Enqueue the pass. This happens every frame.
            if (camera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData))
            {
                cameraData.scriptableRenderer.EnqueuePass(s_instance);
            }
        }

        // Called before Configure.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            // This will disable MSAA for our textures as MSAA will break sampling later on. This looks safe to do as
            // Unity's CopyDepthPass does the same, but a possible better way or supporting MSAA is worth looking into.
            descriptor.msaaSamples = 1;
            descriptor.colorFormat = RenderTextureFormat.RHalf;
            descriptor.depthBufferBits = 0;
            // Use temporary render textures because setting these up ourselves for XR SPI proved to be too difficult.
            cmd.GetTemporaryRT(UnderwaterRenderer.sp_CrestOceanMaskTexture, descriptor);
            descriptor.colorFormat = RenderTextureFormat.Depth;
            descriptor.depthBufferBits = 24;
            cmd.GetTemporaryRT(UnderwaterRenderer.sp_CrestOceanMaskDepthTexture, descriptor);
        }

        // Called before Execute.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(_maskTarget, _depthTarget);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
            {
                throw new System.ArgumentNullException("cmd");
            }

            cmd.ReleaseTemporaryRT(UnderwaterRenderer.sp_CrestOceanMaskTexture);
            cmd.ReleaseTemporaryRT(UnderwaterRenderer.sp_CrestOceanMaskDepthTexture);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

            XRHelpers.Update(camera);
            XRHelpers.UpdatePassIndex(ref UnderwaterRenderer.s_xrPassIndex);

            CommandBuffer commandBuffer = CommandBufferPool.Get("Ocean Mask");

            commandBuffer.SetGlobalTexture(UnderwaterRenderer.sp_CrestOceanMaskTexture, _maskTarget);
            commandBuffer.SetGlobalTexture(UnderwaterRenderer.sp_CrestOceanMaskDepthTexture, _depthTarget);

            UnderwaterRenderer.PopulateOceanMask(
                commandBuffer,
                camera,
                OceanRenderer.Instance.Tiles,
                _underwaterRenderer._cameraFrustumPlanes,
                _oceanMaskMaterial.material,
                _underwaterRenderer._farPlaneMultiplier,
                _underwaterRenderer._debug._disableOceanMask
            );

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
}

#endif // CREST_URP
