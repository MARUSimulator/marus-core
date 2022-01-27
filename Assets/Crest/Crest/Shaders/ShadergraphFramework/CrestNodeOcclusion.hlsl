// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"

void CrestNodeOcclusion_half
(
	in const half i_occlusion,
	in const bool i_underwater,
	out half o_occlusion
)
{
	o_occlusion = i_underwater ? 0 : i_occlusion;
}
