// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

using UnityEngine.Rendering;
#if CREST_URP
using UnityEngine.Rendering.Universal;
#endif
#if CREST_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Crest
{
    public enum RenderPipeline
    {
        Legacy,
        HighDefinition,
        Universal,
    }

    public class RenderPipelineHelper
    {
        public static bool IsLegacy => GraphicsSettings.renderPipelineAsset == null;

        public static bool IsUniversal
        {
            get
            {
#if CREST_URP
                return GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset;
#else
                return false;
#endif
            }
        }

        public static bool IsHighDefinition
        {
            get
            {
#if CREST_HDRP
                return GraphicsSettings.renderPipelineAsset is HDRenderPipelineAsset;
#else
                return false;
#endif
            }
        }
    }
}
