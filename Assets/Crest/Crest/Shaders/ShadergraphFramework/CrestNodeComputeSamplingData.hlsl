// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanConstants.hlsl"
#include "../OceanGlobals.hlsl"
#include "../OceanInputsDriven.hlsl"
#include "../OceanHelpersNew.hlsl"

void CrestComputeSamplingData_half
(
	in const float2 worldXZ,
	out float lodAlpha,
	out float3 o_oceanPosScale0,
	out float3 o_oceanPosScale1,
	out float4 o_oceanParams0,
	out float4 o_oceanParams1,
	out float slice0,
	out float slice1
)
{
	PosToSliceIndices(worldXZ, 0.0, _CrestCascadeData[0]._scale, slice0, slice1, lodAlpha);

	uint si0 = (uint)slice0;
	uint si1 = si0 + 1;

	o_oceanPosScale0 = float3(_CrestCascadeData[si0]._posSnapped, _CrestCascadeData[si0]._scale);
	o_oceanPosScale1 = float3(_CrestCascadeData[si1]._posSnapped, _CrestCascadeData[si1]._scale);

	o_oceanParams0 = float4(_CrestCascadeData[si0]._texelWidth, _CrestCascadeData[si0]._textureRes, _CrestCascadeData[si0]._weight, _CrestCascadeData[si0]._oneOverTextureRes);
	o_oceanParams1 = float4(_CrestCascadeData[si1]._texelWidth, _CrestCascadeData[si1]._textureRes, _CrestCascadeData[si1]._weight, _CrestCascadeData[si1]._oneOverTextureRes);
}
