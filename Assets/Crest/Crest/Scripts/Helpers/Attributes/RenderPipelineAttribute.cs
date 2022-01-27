// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

namespace Crest
{
#if UNITY_EDITOR
    using Crest.EditorHelpers;
#endif
    using UnityEditor;
    using UnityEngine;

    public class RenderPipelineAttribute : DecoratorAttribute
    {
        readonly RenderPipeline pipeline;

        public RenderPipelineAttribute(RenderPipeline pipeline)
        {
            this.pipeline = pipeline;
        }

#if UNITY_EDITOR
        internal override void Decorate(Rect position, SerializedProperty property, GUIContent label, DecoratedDrawer drawer)
        {
            switch (pipeline)
            {
                case RenderPipeline.Legacy:
                    if (!RenderPipelineHelper.IsLegacy) DecoratedDrawer.s_HideInInspector = true;
                    break;
                case RenderPipeline.HighDefinition:
                    if (!RenderPipelineHelper.IsHighDefinition) DecoratedDrawer.s_HideInInspector = true;
                    break;
                case RenderPipeline.Universal:
                    if (!RenderPipelineHelper.IsUniversal) DecoratedDrawer.s_HideInInspector = true;
                    break;
                default: break;
            }
        }
#endif
    }
}
