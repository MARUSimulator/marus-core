// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#ifndef CREST_OCEAN_NORMAL_MAPPING_INCLUDED
#define CREST_OCEAN_NORMAL_MAPPING_INCLUDED

#include "../OceanGlobals.hlsl"

half2 SampleNormalMaps
(
	float2 worldXZUndisplaced,
	in const Texture2D<float4> i_normals,
	half i_normalsScale,
	half i_normalsStrength,
	float lodAlpha,
	in const CascadeParams cascadeData,
	in const PerCascadeInstanceData instanceData
)
{
	const float lodDataGridSize = cascadeData._texelWidth;
	float2 normalScrollSpeeds = instanceData._normalScrollSpeeds;

#ifdef SHADERGRAPH_PREVIEW
	// sampler_TextureNormals is not defined in shader graph. Silence error.
	SamplerState sampler_TextureNormals = LODData_linear_clamp_sampler;
#endif

	const float2 v0 = float2(0.94, 0.34), v1 = float2(-0.85, -0.53);
	float nstretch = i_normalsScale * lodDataGridSize; // normals scaled with geometry
	const float spdmulL = normalScrollSpeeds[0];
	half2 norm =
		UnpackNormal(i_normals.Sample(sampler_TextureNormals, (v0*_CrestTime*spdmulL + worldXZUndisplaced) / nstretch)).xy +
		UnpackNormal(i_normals.Sample(sampler_TextureNormals, (v1*_CrestTime*spdmulL + worldXZUndisplaced) / nstretch)).xy;

	// blend in next higher scale of normals to obtain continuity
	const float farNormalsWeight = instanceData._farNormalsWeight;
	const half nblend = lodAlpha * farNormalsWeight;
	if (nblend > 0.001)
	{
		// next lod level
		nstretch *= 2.0;
		const float spdmulH = normalScrollSpeeds[1];
		norm = lerp(norm,
			UnpackNormal(i_normals.Sample(sampler_TextureNormals, (v0*_CrestTime*spdmulH + worldXZUndisplaced) / nstretch)).xy +
			UnpackNormal(i_normals.Sample(sampler_TextureNormals, (v1*_CrestTime*spdmulH + worldXZUndisplaced) / nstretch)).xy,
			nblend);
	}

	// approximate combine of normals. would be better if normals applied in local frame.
	return i_normalsStrength * norm;
}

void ApplyNormalMapsWithFlow
(
	const in half2 flow,
	const in float2 worldXZUndisplaced,
	const in Texture2D<float4> i_normals,
	const in half i_normalsScale,
	const in half i_normalsStrength,
	const in float lodAlpha,
	in const CascadeParams cascadeData,
	in const PerCascadeInstanceData instanceData,
	inout float3 io_n
)
{
	// When converting to Shader Graph, this code is already in the CrestFlow subgraph
	const float half_period = 1;
	const float period = half_period * 2;
	float sample1_offset = fmod(_CrestTime, period);
	float sample1_weight = sample1_offset / half_period;
	if (sample1_weight > 1.0) sample1_weight = 2.0 - sample1_weight;
	float sample2_offset = fmod(_CrestTime + half_period, period);
	float sample2_weight = 1.0 - sample1_weight;

	// In order to prevent flow from distorting the UVs too much,
	// we fade between two samples of normal maps so that for each
	// sample the UVs can be reset
	half2 io_n_1 = SampleNormalMaps(worldXZUndisplaced - (flow * sample1_offset), i_normals, i_normalsScale, i_normalsStrength, lodAlpha, cascadeData, instanceData);
	half2 io_n_2 = SampleNormalMaps(worldXZUndisplaced - (flow * sample2_offset), i_normals, i_normalsScale, i_normalsStrength, lodAlpha, cascadeData, instanceData);
	io_n.xz += sample1_weight * io_n_1;
	io_n.xz += sample2_weight * io_n_2;
}

#endif // CREST_OCEAN_NORMAL_MAPPING_INCLUDED
