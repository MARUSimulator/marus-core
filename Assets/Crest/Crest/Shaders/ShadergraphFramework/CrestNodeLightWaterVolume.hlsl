// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanConstants.hlsl"
#include "../OceanGlobals.hlsl"
#include "../OceanInputsDriven.hlsl"
#include "../OceanHelpersNew.hlsl"

void CrestNodeLightWaterVolume_half
(
	const half3 i_scatterColourBase,
	const half3 i_scatterColourShadow,
	const half3 i_scatterColourShallow,
	const half i_scatterColourShallowDepthMax,
	const half i_scatterColourShallowDepthFalloff,
	const half i_sssIntensityBase,
	const half i_sssIntensitySun,
	const half3 i_sssTint,
	const half i_sssSunFalloff,
	const half i_surfaceOceanWaterDepth,
	const half2 i_shadow,
	const half i_sss,
	const half3 i_viewNorm,
	const half3 i_positionWS, // Unused
	const half3 i_ambientLighting,
	const half3 i_primaryLightDirection,
	const half3 i_primaryLightIntensity,
	out half3 o_volumeLight
)
{
	half shadow = 1.0 - i_shadow.x;

	// base colour
	o_volumeLight = i_scatterColourBase;

// #if CREST_SHADOWS_ON
	o_volumeLight = lerp(i_scatterColourShadow, o_volumeLight, shadow);
// #endif // CREST_SHADOWS_ON

	float shallowness = pow(1.0 - saturate(i_surfaceOceanWaterDepth / i_scatterColourShallowDepthMax), i_scatterColourShallowDepthFalloff);
	half3 shallowCol = i_scatterColourShallow;

// #if CREST_SHADOWS_ON
	shallowCol = lerp(i_scatterColourShadow, shallowCol, shadow);
// #endif // CREST_SHADOWS_ON

	o_volumeLight = lerp(o_volumeLight, shallowCol, shallowness);

	// Light the base colour. Use the constant term (0th order) of SH stuff - this is the average. Use the primary light integrated over the
	// hemisphere (divide by pi).
	o_volumeLight *= i_ambientLighting + shadow * i_primaryLightIntensity / 3.14159;

	// Approximate subsurface scattering - add light when surface faces viewer. Use geometry normal - don't need high freqs.
	half towardsSun = pow(max(0.0, dot(i_primaryLightDirection, -i_viewNorm)), i_sssSunFalloff);

	float v = abs(i_viewNorm.y);
	half3 subsurface = (i_sssIntensityBase + i_sssIntensitySun * towardsSun) * i_sssTint * i_primaryLightIntensity * shadow;
	subsurface *= (1.0 - v * v) * i_sss;
	o_volumeLight += subsurface;
}
