// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanGlobals.hlsl"

half WhiteFoamTexture
(
	in const Texture2D<float4> i_texture,
	in const half2 i_texelSize,
	in const half i_scale,
	in const half i_feather,
	in const half i_foam,
	in const float i_texelSize0,
	in const float i_texelSize1,
	in const float2 i_worldXZUndisplaced,
	in const float2 i_texelOffset,
	in const half lodVal
)
{
	half ft = lerp(
		SAMPLE_TEXTURE2D(i_texture, sampler_Crest_linear_repeat, (1.25 * (i_worldXZUndisplaced + i_texelOffset * i_texelSize) + _CrestTime / 10.0) / (4.0 * i_texelSize0 * i_scale)).x,
		SAMPLE_TEXTURE2D(i_texture, sampler_Crest_linear_repeat, (1.25 * (i_worldXZUndisplaced + i_texelOffset * i_texelSize) + _CrestTime / 10.0) / (4.0 * i_texelSize1 * i_scale)).x,
		lodVal);

	// black point fade
	half result = saturate(1.0 - i_foam);
	return smoothstep(result, result + i_feather, ft);
}

void CrestNodeFoam_half
(
	in const Texture2D<float4> i_texture,
	in const half2 i_texelSize,
	in const half i_scale,
	in const half i_feather,
	in const half i_albedoIntensity,
	in const half i_emissiveIntensity,
	in const half i_foamSmoothness,
	in const half i_normalStrength,
	in const float4 i_oceanParams0,
	in const float4 i_oceanParams1,
	in const half i_foam,
	in const half lodVal,
	in const float2 i_worldXZUndisplaced,
	in const float i_pixelZ,
	in half3 i_n,
	in half3 i_emission,
	in float i_smoothness,
	out half3 o_albedo,
	out half3 o_n,
	out half3 o_emission,
	out float o_smoothness
)
{
	// Get the "special" properties from the texture. The texel node only exposes zw (width and height). We could
	// compute this ourselves.
	half2 texelSizeXY =
#ifdef SHADERGRAPH_PREVIEW
	i_texelSize;
#else
	 _TextureFoam_TexelSize.xy;
#endif


	float whiteFoam = WhiteFoamTexture(i_texture, texelSizeXY, i_scale, i_feather, i_foam, i_oceanParams0.x, i_oceanParams1.x, i_worldXZUndisplaced, (float2)0.0, lodVal);
	o_albedo = saturate(whiteFoam * i_albedoIntensity);
	o_emission = lerp(i_emission, i_emissiveIntensity, whiteFoam);
	o_smoothness = lerp(i_smoothness, i_foamSmoothness, whiteFoam);

	//#if _FOAM3DLIGHTING_ON
	float2 dd = float2(0.25 * i_pixelZ, 0.0);
	half whiteFoam_x = WhiteFoamTexture(i_texture, texelSizeXY, i_scale, i_feather, i_foam, i_oceanParams0.x, i_oceanParams1.x, i_worldXZUndisplaced, dd.xy, lodVal);
	half whiteFoam_z = WhiteFoamTexture(i_texture, texelSizeXY, i_scale, i_feather, i_foam, i_oceanParams0.x, i_oceanParams1.x, i_worldXZUndisplaced, dd.yx, lodVal);

	// Compute a foam normal - manually push in derivatives. If i used blend smooths all the normals towards straight up when there is no foam.
	o_n = i_n;
	o_n.xy -= i_normalStrength * float2(whiteFoam_x - whiteFoam, whiteFoam_z - whiteFoam);
	o_n = normalize(o_n);
	//#endif // _FOAM3DLIGHTING_ON
}
