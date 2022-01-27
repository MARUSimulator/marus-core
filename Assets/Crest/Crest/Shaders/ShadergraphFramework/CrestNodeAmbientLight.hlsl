// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

#include "OceanGraphConstants.hlsl"

void ApplyIndirectLightingMultiplier
(
	inout half3 io_ambientLight
)
{
	// Allows control of baked lighting through volume framework.
#if !defined(SHADERGRAPH_PREVIEW)
	// We could create a BuiltinData struct which would have rendering layers on it, but it seems more complicated.
	uint renderingLayers = _EnableLightLayers ? asuint(unity_RenderingLayer.x) : DEFAULT_LIGHT_LAYERS;
	io_ambientLight *= GetIndirectDiffuseMultiplier(renderingLayers);
#endif
}

void CrestNodeAmbientLight_half
(
	out half3 o_ambientLight
)
{
	// Use the constant term (0th order) of SH stuff - this is the average
	o_ambientLight = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
	ApplyIndirectLightingMultiplier(o_ambientLight);
}
