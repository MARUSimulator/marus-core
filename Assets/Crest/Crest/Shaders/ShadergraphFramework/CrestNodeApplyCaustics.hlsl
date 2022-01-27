// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanGlobals.hlsl"

void CrestNodeApplyCaustics_float
(
	in const half3 i_sceneColour,
	in const float3 i_scenePos,
	in const float i_waterSurfaceY,
	in const half3 i_depthFogDensity,
	in const half3 i_lightCol,
	in const half3 i_lightDir,
	in const float i_sceneZ,
	in const Texture2D<float4> i_texture,
	in const half i_textureScale,
	in const half i_textureAverage,
	in const half i_strength,
	in const half i_focalDepth,
	in const half i_depthOfField,
	in const Texture2D<float4> i_distortion,
	in const half i_distortionStrength,
	in const half i_distortionScale,
	in const bool i_underwater,
	out half3 o_sceneColour
)
{
	o_sceneColour = i_sceneColour;

	// @HACK: When used by the underwater effect, either scene position or surface height is out of sync leading to
	// caustics rendering short of the surface. CREST_GENERATED_SHADER_ON limits this to the ocean shader.
#if CREST_GENERATED_SHADER_ON
	// We don't want caustics showing above the surface until we can implement it for both cases when the view is either
	// above or below the surface. We can only do the latter scenario at the moment.
	if (i_scenePos.y > i_waterSurfaceY) return;
#endif

	half sceneDepth = i_waterSurfaceY - i_scenePos.y;

	// Compute mip index manually, with bias based on sea floor depth. We compute it manually because if it is computed automatically it produces ugly patches
	// where samples are stretched/dilated. The bias is to give a focusing effect to caustics - they are sharpest at a particular depth. This doesn't work amazingly
	// well and could be replaced.
	float mipLod = log2(i_sceneZ) + abs(sceneDepth - i_focalDepth) / i_depthOfField;

	// Project along light dir, but multiply by a fudge factor reduce the angle bit - compensates for fact that in real life
	// caustics come from many directions and don't exhibit such a strong directonality
	// Removing the fudge factor (4.0) will cause the caustics to move around more with the waves. But this will also
	// result in stretched/dilated caustics in certain areas. This is especially noticeable on angled surfaces.
	float2 surfacePosXZ = i_scenePos.xz + i_lightDir.xz * sceneDepth / (4.0*i_lightDir.y);
	float2 cuv1 = surfacePosXZ / i_textureScale + float2(0.044*_CrestTime + 17.16, -0.169*_CrestTime);
	float2 cuv2 = 1.37*surfacePosXZ / i_textureScale + float2(0.248*_CrestTime, 0.117*_CrestTime);

	if (i_underwater)
	{
		// Add distortion if we're not getting the refraction
		half2 causticN = i_distortionStrength  * UnpackNormal(i_distortion.Sample(sampler_Crest_linear_repeat, surfacePosXZ / i_distortionScale)).xy;
		cuv1.xy += 1.30 * causticN;
		cuv2.xy += 1.77 * causticN;
	}

	half causticsStrength = i_strength;

// #if CREST_SHADOWS_ON
	{
		// Calculate projected position again as we do not want the fudge factor. If we include the fudge factor, the
		// caustics will not be aligned with shadows.
		const float2 shadowSurfacePosXZ = i_scenePos.xz + i_lightDir.xz * sceneDepth / i_lightDir.y;
		real2 causticShadow = 0.0;

		// TODO - pass in to avoid reading here?
		const CascadeParams cascadeData0 = _CrestCascadeData[_LD_SliceIndex];
		const CascadeParams cascadeData1 = _CrestCascadeData[_LD_SliceIndex + 1];

		// As per the comment for the underwater code in ScatterColour,
		// LOD_1 data can be missing when underwater
		if (i_underwater)
		{
			const float3 uv_smallerLod = WorldToUV(shadowSurfacePosXZ, cascadeData0, _LD_SliceIndex);
			SampleShadow(_LD_TexArray_Shadow, uv_smallerLod, 1.0, causticShadow);
		}
		else
		{
			// only sample the bigger lod. if pops are noticeable this could lerp the 2 lods smoothly, but i didnt notice issues.
			float3 uv_biggerLod = WorldToUV(shadowSurfacePosXZ, cascadeData1, _LD_SliceIndex + 1);
			SampleShadow(_LD_TexArray_Shadow, uv_biggerLod, 1.0, causticShadow);
		}
		causticsStrength *= 1.0 - causticShadow.y;
	}
// #endif // CREST_SHADOWS_ON

	o_sceneColour.xyz *= 1.0 + causticsStrength * (
		0.5 * i_texture.SampleLevel(sampler_Crest_linear_repeat, cuv1, mipLod).xyz +
		0.5 * i_texture.SampleLevel(sampler_Crest_linear_repeat, cuv2, mipLod).xyz
		- i_textureAverage);
}
