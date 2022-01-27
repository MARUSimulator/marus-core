// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

namespace Crest
{
    using UnityEngine;
#if CREST_HDRP
    using UnityEngine.Rendering.HighDefinition;
#endif

    /// <summary>
    /// General purpose helpers which, at the moment, do not warrant a seperate file.
    /// </summary>
    public static class Helpers
    {
        public static bool IsMSAAEnabled(Camera camera)
        {
#if CREST_HDRP
            if (RenderPipelineHelper.IsHighDefinition)
            {
                var hdCamera = HDCamera.GetOrCreate(camera);
                return hdCamera.msaaSamples != UnityEngine.Rendering.MSAASamples.None && hdCamera.frameSettings.IsEnabled(FrameSettingsField.MSAA);
            }
            else
#endif
            {
                return camera.allowMSAA && QualitySettings.antiAliasing > 0f;
            }
        }
    }
}
