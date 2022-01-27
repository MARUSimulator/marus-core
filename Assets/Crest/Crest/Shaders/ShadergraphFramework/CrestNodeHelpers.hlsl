// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

float4 _CameraDepthTexture_TexelSize;

#include "OceanGraphConstants.hlsl"
#include "../OceanShaderHelpers.hlsl"

void CrestNodeLinearEyeDepth_float
(
	in const float i_rawDepth,
	out float o_linearDepth
)
{
	o_linearDepth = CrestLinearEyeDepth(i_rawDepth);
}

void CrestNodeMultiSampleDepth_float
(
	in const float i_rawDepth,
	in const float2 i_positionNDC,
	out float o_rawDepth
)
{
	o_rawDepth = CrestMultiSampleSceneDepth(i_rawDepth, i_positionNDC);
}
