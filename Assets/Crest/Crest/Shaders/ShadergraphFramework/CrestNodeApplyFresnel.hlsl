// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanGlobals.hlsl"

float CalculateFresnelReflectionCoefficient(in const float cosTheta, in const float i_refractiveIndexOfAir, in const float i_refractiveIndexOfWater)
{
	// Fresnel calculated using Schlick's approximation
	// See: http://www.cs.virginia.edu/~jdl/bib/appearance/analytic%20models/schlick94b.pdf
	// reflectance at facing angle
	float R_0 = (i_refractiveIndexOfAir - i_refractiveIndexOfWater) / (i_refractiveIndexOfAir + i_refractiveIndexOfWater); R_0 *= R_0;
	const float R_theta = R_0 + (1.0 - R_0) * pow(max(0., 1.0 - cosTheta), 5.0);
	return R_theta;
}

void ApplyReflectionUnderwater(
	in const half3 i_view,
	in const half3 i_nPixelWS,
	in const float i_refractiveIndexOfAir,
	in const float i_refractiveIndexOfWater,
	out float o_lightTransmitted,
	out float o_lightReflected
) {
	// The the angle of outgoing light from water's surface
	// (whether refracted form outside or internally reflected)
	const float cosOutgoingAngle = max(dot(i_nPixelWS, i_view), 0.);

	// calculate the amount of light transmitted from the sky (o_lightTransmitted)
	{
		// have to calculate the incident angle of incoming light to water
		// surface based on how it would be refracted so as to hit the camera
		const float cosIncomingAngle = cos(asin(clamp((i_refractiveIndexOfWater * sin(acos(cosOutgoingAngle))) / i_refractiveIndexOfAir, -1.0, 1.0)));
		const float reflectionCoefficient = CalculateFresnelReflectionCoefficient(cosIncomingAngle, i_refractiveIndexOfAir, i_refractiveIndexOfWater);
		o_lightTransmitted = (1.0 - reflectionCoefficient);
		o_lightTransmitted = max(o_lightTransmitted, 0.0);
	}

	// calculate the amount of light reflected from below the water
	{
		// angle of incident is angle of reflection
		const float cosIncomingAngle = cosOutgoingAngle;
		const float criticalAngle = asin(i_refractiveIndexOfAir/i_refractiveIndexOfWater);
		const float reflectionCoefficient = cos(criticalAngle) > cosIncomingAngle ? 1.0 : 0.0;
		o_lightReflected = reflectionCoefficient;
	}
}

void CrestNodeApplyFresnel_float
(
	in const half3 i_view,
	in const half3 i_nPixelTS,
	in const bool i_isUnderwater,
	in const float i_refractiveIndexOfAir,
	in const float i_refractiveIndexOfWater,
	out float o_lightTransmitted,
	out float o_lightReflected
)
{
	o_lightTransmitted = 1.0;

	const float3 normalWS = float3(i_nPixelTS.x, i_nPixelTS.z, i_nPixelTS.y);

	if (i_isUnderwater)
	{
		ApplyReflectionUnderwater(i_view, normalWS, i_refractiveIndexOfAir, i_refractiveIndexOfWater, o_lightTransmitted, o_lightReflected);
	}
	else
	{
		const float cosAngle = max(dot(normalWS, i_view), 0.0);
		o_lightReflected = CalculateFresnelReflectionCoefficient(cosAngle, i_refractiveIndexOfAir, i_refractiveIndexOfWater);
	}
}
