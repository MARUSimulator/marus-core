// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanGlobals.hlsl"
#include "../OceanInputsDriven.hlsl"
#include "../OceanVertHelpers.hlsl"

void GeoMorph_half
(
	in const float3 i_positionWS,
	in const float3 i_oceanPosScale0,
	in const float i_meshScaleAlpha,
	in const float i_geometryGridSize,
	out float3 o_positionMorphedWS,
	out float o_lodAlpha
)
{
	const CascadeParams cascadeData0 = _CrestCascadeData[_LD_SliceIndex];
	o_positionMorphedWS = i_positionWS;

	// Vertex snapping and lod transition
	SnapAndTransitionVertLayout(i_meshScaleAlpha, cascadeData0, i_geometryGridSize, o_positionMorphedWS, o_lodAlpha);
}
