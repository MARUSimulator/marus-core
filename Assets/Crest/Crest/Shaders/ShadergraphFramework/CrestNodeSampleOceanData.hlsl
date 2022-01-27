// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanConstants.hlsl"
#include "../OceanGlobals.hlsl"
#include "../OceanInputsDriven.hlsl"
#include "../OceanHelpersNew.hlsl"

void CrestNodeSampleOceanData_float
(
	in const float2 i_positionXZWS,
	in const float i_lodAlpha,
	in const float3 i_oceanPosScale0,
	in const float3 i_oceanPosScale1,
	in const float4 i_oceanParams0,
	in const float4 i_oceanParams1,
	in const float i_sliceIndex0,
	out float3 o_displacement,
	out half o_oceanDepth,
	out half o_foam,
	out half2 o_shadow,
	out half2 o_flow,
	out half o_sss // Unused
)
{
	o_displacement = 0.0;
	o_foam = 0.0;
	o_shadow = 0.0;
	o_flow = 0.0;
	o_sss = 0.0;
	o_oceanDepth = CREST_OCEAN_DEPTH_BASELINE;

	// Calculate sample weights. params.z allows shape to be faded out (used on last lod to support pop-less scale transitions)
	const float wt_smallerLod = (1. - i_lodAlpha) * i_oceanParams0.z;
	const float wt_biggerLod = (1. - wt_smallerLod) * i_oceanParams1.z;

	CascadeParams cascadeData0 = MakeCascadeParams(i_oceanPosScale0, i_oceanParams0);
	CascadeParams cascadeData1 = MakeCascadeParams(i_oceanPosScale1, i_oceanParams1);

	// Sample displacement textures, add results to current displacement/ normal / foam

	// Data that needs to be sampled at the undisplaced position
	if (wt_smallerLod > 0.001)
	{
		const float3 uv_slice_smallerLod = WorldToUV(i_positionXZWS, cascadeData0, i_sliceIndex0);

//#if !_DEBUGDISABLESHAPETEXTURES_ON
		SampleDisplacements(_LD_TexArray_AnimatedWaves, uv_slice_smallerLod, wt_smallerLod, o_displacement);
//#endif

//#if _FOAM_ON
		SampleFoam(_LD_TexArray_Foam, uv_slice_smallerLod, wt_smallerLod, o_foam);
//#endif

#if CREST_FLOW_ON
		SampleFlow(_LD_TexArray_Flow, uv_slice_smallerLod, wt_smallerLod, o_flow);
#endif
	}
	if (wt_biggerLod > 0.001)
	{
		const float3 uv_slice_biggerLod = WorldToUV(i_positionXZWS, cascadeData1, i_sliceIndex0 + 1.0);

//#if !_DEBUGDISABLESHAPETEXTURES_ON
		SampleDisplacements(_LD_TexArray_AnimatedWaves, uv_slice_biggerLod, wt_biggerLod, o_displacement);
//#endif

//#if _FOAM_ON
		SampleFoam(_LD_TexArray_Foam, uv_slice_biggerLod, wt_biggerLod, o_foam);
//#endif

#if CREST_FLOW_ON
		SampleFlow(_LD_TexArray_Flow, uv_slice_biggerLod, wt_biggerLod, o_flow.xy);
#endif
	}

	// Data that needs to be sampled at the displaced position
	if (wt_smallerLod > 0.0001)
	{
		const float3 uv_slice_smallerLodDisp = WorldToUV(i_positionXZWS + o_displacement.xz, cascadeData0, i_sliceIndex0);

//#if _SUBSURFACESHALLOWCOLOUR_ON
		// The minimum sampling weight is lower (0.0001) than others to fix shallow water colour popping.
		SampleSeaDepth(_LD_TexArray_SeaFloorDepth, uv_slice_smallerLodDisp, wt_smallerLod, o_oceanDepth);
//#endif

// #if CREST_SHADOWS_ON
		if (wt_smallerLod > 0.001)
		{
			SampleShadow(_LD_TexArray_Shadow, uv_slice_smallerLodDisp, wt_smallerLod, o_shadow);
		}
// #endif // CREST_SHADOWS_ON
	}
	if (wt_biggerLod > 0.0001)
	{
		const float3 uv_slice_biggerLodDisp = WorldToUV(i_positionXZWS + o_displacement.xz, cascadeData1, i_sliceIndex0 + 1.0);

//#if _SUBSURFACESHALLOWCOLOUR_ON
		// The minimum sampling weight is lower (0.0001) than others to fix shallow water colour popping.
		SampleSeaDepth(_LD_TexArray_SeaFloorDepth, uv_slice_biggerLodDisp, wt_biggerLod, o_oceanDepth);
//#endif

// #if CREST_SHADOWS_ON
		if (wt_biggerLod > 0.001)
		{
			SampleShadow(_LD_TexArray_Shadow, uv_slice_biggerLodDisp, wt_biggerLod, o_shadow);
		}
// #endif // CREST_SHADOWS_ON
	}

	// Foam can saturate
	o_foam = saturate(o_foam);
}

void CrestNodeSampleOceanDataSingle_float
(
	in const float2 i_positionXZWS,
	in const float3 i_oceanPosScale,
	in const float4 i_oceanParams,
	in const float i_sliceIndex,
	out float3 o_displacement,
	out half o_oceanDepth,
	out half o_foam,
	out half2 o_shadow,
	out half2 o_flow,
	out half o_sss // Unused
)
{
	o_displacement = 0.0;
	o_foam = 0.0;
	o_shadow = 1.0;
	o_flow = 0.0;
	o_sss = 0.0;
	o_oceanDepth = CREST_OCEAN_DEPTH_BASELINE;

	CascadeParams cascadeData = MakeCascadeParams(i_oceanPosScale, i_oceanParams);

	// Sample displacement texture, add results to current world pos / normal / foam

	// Data that needs to be sampled at the undisplaced position
	{
		const float3 uv_slice = WorldToUV(i_positionXZWS, cascadeData, i_sliceIndex);

		//#if !_DEBUGDISABLESHAPETEXTURES_ON
		SampleDisplacements(_LD_TexArray_AnimatedWaves, uv_slice, 1.0, o_displacement);
		//#endif

		//#if _FOAM_ON
		SampleFoam(_LD_TexArray_Foam, uv_slice, 1.0, o_foam);
		//#endif

#if CREST_FLOW_ON
		SampleFlow(_LD_TexArray_Flow, uv_slice, 1.0, o_flow);
#endif
	}

	// Data that needs to be sampled at the displaced position
	{
		const float3 uv_slice = WorldToUV(i_positionXZWS + o_displacement.xz, cascadeData, i_sliceIndex);

		//#if _SUBSURFACESHALLOWCOLOUR_ON
		SampleSeaDepth(_LD_TexArray_SeaFloorDepth, uv_slice, 1.0, o_oceanDepth);
		//#endif

// #if CREST_SHADOWS_ON
		SampleShadow(_LD_TexArray_Shadow, uv_slice, 1.0, o_shadow);
// #endif // CREST_SHADOWS_ON
	}

	// Foam can saturate
	o_foam = saturate(o_foam);
}
