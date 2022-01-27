// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

// A custom shader GUI for the ocean shader.
//
// It is not possible to have a working material inspector for both Unity 2020 LTS and 2021 LTS for HDRP as there are
// significant changes to the material inspector architecture. The issue is that exposed properties were no longer
// shown. By using a custom shader GUI they are restored, but we lose the HDRP section of the inspector so resture the
// functionality as necessary.

#if UNITY_EDITOR

namespace Crest
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    static class Extensions
    {
        public static void SetKeyword(this Material material, string keyword, bool enabled)
        {
            if (enabled)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }
    }

    public class OceanShaderGUI : ShaderGUI
    {
        Material _material;
        MaterialProperty[] _properties;

        bool ToggleProperty(string label, string name, string keyword = null, bool inverted = false)
        {
            var property = FindProperty(name, _properties);
            property.floatValue = Convert.ToInt32(EditorGUILayout.Toggle(new GUIContent(label), Mathf.Approximately(property.floatValue, 1f)));
            var value = Mathf.Approximately(property.floatValue, 1f);
            if (keyword != null)
            {
                // Some keywords are present to disable feature....
                _material.SetKeyword(keyword, inverted ? !value : value);
            }
            else
            {
                _material.SetFloat(name, property.floatValue);
            }

            return value;
        }

        void ShowCullMode()
        {
            var property = FindProperty("_CullMode", _properties);
            property.floatValue = Convert.ToInt32(EditorGUILayout.EnumPopup(new GUIContent("Cull Mode"), (CullMode)property.floatValue));
            // There are more cull mode properties, but this appears to be all we need.
            _material.SetFloat("_CullModeForward", property.floatValue);
            _material.SetFloat("_TransparentCullMode", property.floatValue);
            // Also set double-sided rendering based on cull mode.
            _material.SetKeyword("_DOUBLESIDED_ON", property.floatValue == 0f);
        }

        // These properties do not need to be exposed.
        void ForceTransparency()
        {
            _material.SetInt("_SurfaceType", 1);
            _material.SetKeyword("_SURFACE_TYPE_TRANSPARENT", true);
            _material.SetKeyword("_BLENDMODE_OFF", true);
            _material.SetKeyword("_REFRACTION_OFF", true);
        }

        public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
        {
            // Render the default GUI.
            base.OnGUI(editor, properties);

            _properties = properties;
            _material = editor.target as Material;

            ForceTransparency();
            ShowCullMode();

            // Motion vectors do not work correctly.
            _material.SetKeyword("_TRANSPARENT_WRITES_MOTION_VEC", false);

            ToggleProperty("Enable Fog", "_EnableFogOnTransparent", "_ENABLE_FOG_ON_TRANSPARENT");
            var isSSR = ToggleProperty("Enable Screen-Space Reflections", "_ReceivesSSRTransparent", "_DISABLE_SSR_TRANSPARENT", inverted: true);
            ToggleProperty("Enable Decals", "_SupportDecals", "_DISABLE_DECALS", inverted: true);
            ToggleProperty("Alpha Clipping (Clip Surface)", "_AlphaCutoffEnable", "_ALPHATEST_ON");
            // SSR depends on depth prepass.
            var isDepthPrepass = ToggleProperty("Transparent Depth Prepass", "_TransparentDepthPrepassEnable") || isSSR;

            // Set shader passes.
            _material.SetShaderPassEnabled("TransparentDepthPrepass", isDepthPrepass);
        }
    }
}

#endif // UNITY_EDITOR
