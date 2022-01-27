// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanGlobals.hlsl"

half FoamBubblesTexture
(
	in const Texture2D<float4> i_texture,
	in const half2 i_texelSize,
	in const half i_scale,
	in const half i_parallax,
	in const float i_texelSize0,
	in const float i_texelSize1,
	in const float2 i_worldXZ,
	in const float2 i_worldXZUndisplaced,
	in const half3 i_n,
	in const half3 i_view,
	in const half i_lodVal
)
{
	float2 windDir = float2(0.866, 0.5);
	float2 foamUVBubbles = 0.74 * (lerp(i_worldXZUndisplaced, i_worldXZ, 0.7) + 0.5 * _CrestTime * windDir) / i_scale + 0.125 * i_n.xz;
	float2 parallaxOffset = -i_parallax * i_view.xz / dot(i_n, i_view);
	half ft = lerp
	(
		SAMPLE_TEXTURE2D_LOD(i_texture, sampler_Crest_linear_repeat, (foamUVBubbles + parallaxOffset) / (4.0 * i_texelSize0 * i_scale), 3.0).x,
		SAMPLE_TEXTURE2D_LOD(i_texture, sampler_Crest_linear_repeat, (foamUVBubbles + parallaxOffset) / (4.0 * i_texelSize1 * i_scale), 3.0).x,
		i_lodVal
	);

	return ft;
}

void CrestNodeFoamBubbles_half
(
	in const half3 i_color,
	in const half i_parallax,
	in const half i_coverage,
	in const Texture2D<float4> i_texture,
	in const half2 i_texelSize,
	in const half i_scale,
	in const half i_foamAmount,
	in const half3 i_n,
	in const float4 i_oceanParams0,
	in const float4 i_oceanParams1,
	in const float2 i_worldXZ,
	in const float2 i_worldXZUndisplaced,
	in const half i_lodVal,
	in const half3 i_view,
	in const half3 i_ambientLight,
	in const half2 i_flow,
	out half3 o_color
)
{
	#if CREST_FLOW_ON
		const float half_period = 1;
		const float period = half_period * 2;
		float sample1_offset = fmod(_CrestTime, period);
		float sample1_weight = sample1_offset / half_period;
		if (sample1_weight > 1.0) sample1_weight = 2.0 - sample1_weight;
		float sample2_offset = fmod(_CrestTime + half_period, period);
		float sample2_weight = 1.0 - sample1_weight;
	#endif

	// Additive underwater foam - use same foam texture but add mip bias to blur for free
	half3 bubbleFoamTexValue = (half3)FoamBubblesTexture
	(
		i_texture,
		i_texelSize,
		i_scale,
		i_parallax,
		i_oceanParams0.x,
		i_oceanParams1.x,
		i_worldXZ,
		i_worldXZUndisplaced
#if CREST_FLOW_ON
		- (i_flow * sample1_offset)
#endif
		,
		i_n,
		i_view,
		i_lodVal
	);

	o_color = bubbleFoamTexValue;

	#if CREST_FLOW_ON
		o_color *= sample1_weight;
		bubbleFoamTexValue = (half3)FoamBubblesTexture
		(
			i_texture,
			i_texelSize,
			i_scale,
			i_parallax,
			i_oceanParams0.x,
			i_oceanParams1.x,
			i_worldXZ,
			i_worldXZUndisplaced - (i_flow * sample2_offset),
			i_n,
			i_view,
			i_lodVal
		);
		o_color += bubbleFoamTexValue * sample2_weight;
	#endif

	// Finally, apply colour, coverage and lighting
	 o_color *= i_color.rgb * saturate(i_foamAmount * i_coverage) * i_ambientLight;
}
