// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanInputsDriven.hlsl"

void CrestOceanSurfaceValues_half
(
	out float o_meshScaleAlpha,
	out float o_lodDataTexelSize,
	out float o_geometryGridSize,
	out float3 o_oceanPosScale0,
	out float3 o_oceanPosScale1,
	out float4 o_oceanParams0,
	out float4 o_oceanParams1,
	out float o_sliceIndex0
)
{
	o_sliceIndex0 = _LD_SliceIndex;

	uint si0 = (uint)o_sliceIndex0;
	uint si1 = si0 + 1;

	o_oceanPosScale0 = float3(_CrestCascadeData[si0]._posSnapped, _CrestCascadeData[si0]._scale);
	o_oceanPosScale1 = float3(_CrestCascadeData[si1]._posSnapped, _CrestCascadeData[si1]._scale);

	o_oceanParams0 = float4(_CrestCascadeData[si0]._texelWidth, _CrestCascadeData[si0]._textureRes, _CrestCascadeData[si0]._weight, _CrestCascadeData[si0]._oneOverTextureRes);
	o_oceanParams1 = float4(_CrestCascadeData[si1]._texelWidth, _CrestCascadeData[si1]._textureRes, _CrestCascadeData[si1]._weight, _CrestCascadeData[si1]._oneOverTextureRes);

	o_meshScaleAlpha = _CrestPerCascadeInstanceData[si0]._meshScaleLerp;

	o_lodDataTexelSize = _CrestCascadeData[si0]._texelWidth;
	o_geometryGridSize = _CrestPerCascadeInstanceData[si0]._geoGridWidth;
}
