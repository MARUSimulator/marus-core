// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanConstants.hlsl"
#include "../OceanGlobals.hlsl"
#include "../OceanInputsDriven.hlsl"
#include "../OceanHelpersNew.hlsl"

void CrestNodeSampleClipSurfaceData_float
(
	in const float2 i_positionXZWS,
	in const float i_lodAlpha,
	in const float3 i_oceanPosScale0,
	in const float3 i_oceanPosScale1,
	in const float4 i_oceanParams0,
	in const float4 i_oceanParams1,
	in const float i_sliceIndex0,
	out float o_clipSurface
)
{
	o_clipSurface = 0.0;

	// Calculate sample weights. params.z allows shape to be faded out (used on last lod to support pop-less scale transitions)
	const float wt_smallerLod = (1.0 - i_lodAlpha) * i_oceanParams0.z;
	const float wt_biggerLod = (1.0 - wt_smallerLod) * i_oceanParams1.z;

	// Data that needs to be sampled at the undisplaced position
	if (wt_smallerLod > 0.001)
	{
		CascadeParams cascadeData = MakeCascadeParams(i_oceanPosScale0, i_oceanParams0);
		const float3 uv_slice_smallerLod = WorldToUV(i_positionXZWS, cascadeData, i_sliceIndex0);
		SampleClip(_LD_TexArray_ClipSurface, uv_slice_smallerLod, wt_smallerLod, o_clipSurface);
	}
	if (wt_biggerLod > 0.001)
	{
		CascadeParams cascadeData = MakeCascadeParams(i_oceanPosScale1, i_oceanParams1);
		const float3 uv_slice_biggerLod = WorldToUV(i_positionXZWS, cascadeData, i_sliceIndex0 + 1.0);
		SampleClip(_LD_TexArray_ClipSurface, uv_slice_biggerLod, wt_biggerLod, o_clipSurface);
	}

	// 0.5 mip bias for LOD blending and texel resolution correction. This will help to tighten and smooth clipped edges.
	// We set to 2 or 0 to work correctly with other alpha inputs like feathering.
	o_clipSurface = o_clipSurface > 0.5 ? 2.0 : 0.0;
}
