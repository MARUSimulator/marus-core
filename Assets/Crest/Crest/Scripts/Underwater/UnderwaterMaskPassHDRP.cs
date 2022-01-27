// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

#if CREST_HDRP

namespace Crest
{
    using UnityEngine;
    using UnityEngine.Rendering.HighDefinition;
    using UnityEngine.Rendering;
    using UnityEngine.Experimental.Rendering;

    internal class UnderwaterMaskPassHDRP : CustomPass
    {
        const string k_Name = "Underwater Mask";
        const string k_ShaderPath = "Hidden/Crest/Underwater/Ocean Mask HDRP";

        Material _oceanMaskMaterial;
        RTHandle _maskTexture;
        RTHandle _depthTexture;
        Plane[] _cameraFrustumPlanes;

        static GameObject s_GameObject;
        static UnderwaterRenderer s_UnderwaterRenderer;

        public static void Enable(UnderwaterRenderer underwaterRenderer)
        {
            CustomPassHelpers.CreateOrUpdate<UnderwaterMaskPassHDRP>(ref s_GameObject, k_Name, CustomPassInjectionPoint.BeforeRendering);
            s_UnderwaterRenderer = underwaterRenderer;
        }

        public static void Disable()
        {
            // It should be safe to rely on this reference for this reference to fail.
            if (s_GameObject != null)
            {
                s_GameObject.SetActive(false);
            }
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            _oceanMaskMaterial = CoreUtils.CreateEngineMaterial(k_ShaderPath);

            _maskTexture = RTHandles.Alloc
            (
                scaleFactor: Vector2.one,
                slices: TextureXR.slices,
                dimension: TextureXR.dimension,
                colorFormat: GraphicsFormat.R16_SFloat,
                useDynamicScale: true,
                name: "Crest Ocean Mask"
            );

            _depthTexture = RTHandles.Alloc
            (
                scaleFactor: Vector2.one,
                slices: TextureXR.slices,
                dimension: TextureXR.dimension,
                depthBufferBits: DepthBits.Depth24,
                colorFormat: GraphicsFormat.R8_UNorm, // This appears to be used for depth.
                enableRandomWrite: false,
                useDynamicScale: true,
                name: "Crest Ocean Mask Depth"
            );
        }
        protected override void Cleanup()
        {
            CoreUtils.Destroy(_oceanMaskMaterial);
            _maskTexture.Release();
            _depthTexture.Release();
        }

        protected override void Execute(CustomPassContext context)
        {
            // Null check can be removed once post-processing is removed.
            if (s_UnderwaterRenderer != null && !s_UnderwaterRenderer.IsActive)
            {
                return;
            }

            var camera = context.hdCamera.camera;
            var commandBuffer = context.cmd;

            // Custom passes execute for every camera. We only support one camera for now.
            if (!ReferenceEquals(camera, OceanRenderer.Instance.ViewCamera) || camera.cameraType != CameraType.Game)
            {
                return;
            }

            if (_cameraFrustumPlanes == null)
            {
                _cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            }

            // This property is either on the UnderwaterRenderer or UnderwaterPostProcessHDRP.
            var debugDisableOceanMask = false;
            if (s_UnderwaterRenderer != null)
            {
                debugDisableOceanMask = s_UnderwaterRenderer._debug._disableOceanMask;
            }
            else if (UnderwaterPostProcessHDRP.Instance != null)
            {
                debugDisableOceanMask = UnderwaterPostProcessHDRP.Instance._disableOceanMask.value;
            }

            var farPlaneMultiplier = 1.0f;
            if (s_UnderwaterRenderer != null)
            {
                farPlaneMultiplier = s_UnderwaterRenderer._farPlaneMultiplier;
            }
            else if (UnderwaterPostProcessHDRP.Instance != null)
            {
                farPlaneMultiplier = UnderwaterPostProcessHDRP.Instance._farPlaneMultiplier.value;
            }

            CoreUtils.SetRenderTarget(commandBuffer, _maskTexture, _depthTexture);
            CoreUtils.ClearRenderTarget(commandBuffer, ClearFlag.All, Color.black);
            commandBuffer.SetGlobalTexture(UnderwaterRenderer.sp_CrestOceanMaskTexture, _maskTexture);
            commandBuffer.SetGlobalTexture(UnderwaterRenderer.sp_CrestOceanMaskDepthTexture, _depthTexture);

            UnderwaterRenderer.PopulateOceanMask(
                commandBuffer,
                camera,
                OceanRenderer.Instance.Tiles,
                _cameraFrustumPlanes,
                _oceanMaskMaterial,
                farPlaneMultiplier,
                debugDisableOceanMask
            );
        }
    }
}

#endif // CREST_HDRP
