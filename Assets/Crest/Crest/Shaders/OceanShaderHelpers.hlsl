// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// Helpers that will only be used for shaders (eg depth, lighting etc).

#ifndef CREST_OCEAN_SHADER_HELPERS_H
#define CREST_OCEAN_SHADER_HELPERS_H

// Sample depth macros for all pipelines. Use macros as HDRP depth is a mipchain which can change according to:
// com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl
#if defined(SHADERGRAPH_SAMPLE_SCENE_DEPTH)
#define CREST_SAMPLE_SCENE_DEPTH(coordinates) SHADERGRAPH_SAMPLE_SCENE_DEPTH(coordinates)
#elif defined(TEXTURE2D_X)
#define CREST_SAMPLE_SCENE_DEPTH(coordinates) SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, coordinates).r
#elif defined(SAMPLE_DEPTH_TEXTURE)
#define CREST_SAMPLE_SCENE_DEPTH(coordinates) SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, coordinates)
#endif

#if UNITY_REVERSED_Z
#define CREST_DEPTH_COMPARE(depth1, depth2) min(depth1, depth2)
#else
#define CREST_DEPTH_COMPARE(depth1, depth2) max(depth1, depth2)
#endif

// Same as LinearEyeDepth except supports orthographic projection. Use projection keywords to restrict support to either
// of these modes as an optimisation.
float CrestLinearEyeDepth(const float i_rawDepth)
{
#if !defined(_PROJECTION_ORTHOGRAPHIC)
	// Handles UNITY_REVERSED_Z for us.
#if defined(UNITY_COMMON_INCLUDED)
	float perspective = LinearEyeDepth(i_rawDepth, _ZBufferParams);
#elif defined(UNITY_CG_INCLUDED)
	float perspective = LinearEyeDepth(i_rawDepth);
#endif
#endif // _PROJECTION

#if !defined(_PROJECTION_PERSPECTIVE)
	// Orthographic Depth taken and modified from:
	// https://github.com/keijiro/DepthInverseProjection/blob/master/Assets/InverseProjection/Resources/InverseProjection.shader
	float near = _ProjectionParams.y;
	float far  = _ProjectionParams.z;
	float isOrthographic = unity_OrthoParams.w;

#if defined(UNITY_REVERSED_Z)
	float orthographic = lerp(far, near, i_rawDepth);
#else
	float orthographic = lerp(near, far, i_rawDepth);
#endif // UNITY_REVERSED_Z
#endif // _PROJECTION

#if defined(_PROJECTION_ORTHOGRAPHIC)
	return orthographic;
#elif defined(_PROJECTION_PERSPECTIVE)
	return perspective;
#else
	// If a shader does not have the projection enumeration, then assume they want to support both projection modes.
	return lerp(perspective, orthographic, isOrthographic);
#endif // _PROJECTION
}

// Works for all pipelines.
float CrestMultiSampleSceneDepth(const float i_rawDepth, const float2 i_positionNDC)
{
	float rawDepth = i_rawDepth;

	if (_CrestDepthTextureOffset > 0)
	{
		// We could use screen size instead.
		float2 texelSize = _CameraDepthTexture_TexelSize.xy;
		int3 offset = int3(-_CrestDepthTextureOffset, 0, _CrestDepthTextureOffset);

		rawDepth = CREST_DEPTH_COMPARE(rawDepth, CREST_SAMPLE_SCENE_DEPTH(i_positionNDC + offset.xy * texelSize));
		rawDepth = CREST_DEPTH_COMPARE(rawDepth, CREST_SAMPLE_SCENE_DEPTH(i_positionNDC + offset.yx * texelSize));
		rawDepth = CREST_DEPTH_COMPARE(rawDepth, CREST_SAMPLE_SCENE_DEPTH(i_positionNDC + offset.yz * texelSize));
		rawDepth = CREST_DEPTH_COMPARE(rawDepth, CREST_SAMPLE_SCENE_DEPTH(i_positionNDC + offset.zy * texelSize));
	}

	return rawDepth;
}

// NOTE: Only here for HDRP to solve non MSAA outline for refractions.
float CrestMultiSampleSceneDepth(const float i_rawDepth, const float2 i_positionNDC, half i_refractionStrength)
{
	float rawDepth = i_rawDepth;

	int textureOffset = i_refractionStrength > 0 ? 2 : _CrestDepthTextureOffset;

	if (textureOffset > 0)
	{
		// We could use screen size instead.
		float2 texelSize = _CameraDepthTexture_TexelSize.xy;
		int3 offset = int3(-textureOffset, 0, textureOffset);

		rawDepth = CREST_DEPTH_COMPARE(rawDepth, CREST_SAMPLE_SCENE_DEPTH(i_positionNDC + offset.xy * texelSize));
		rawDepth = CREST_DEPTH_COMPARE(rawDepth, CREST_SAMPLE_SCENE_DEPTH(i_positionNDC + offset.yx * texelSize));
		rawDepth = CREST_DEPTH_COMPARE(rawDepth, CREST_SAMPLE_SCENE_DEPTH(i_positionNDC + offset.yz * texelSize));
		rawDepth = CREST_DEPTH_COMPARE(rawDepth, CREST_SAMPLE_SCENE_DEPTH(i_positionNDC + offset.zy * texelSize));
	}

	return rawDepth;
}

#ifdef TEXTURE2D_X
float CrestMultiLoadDepth(TEXTURE2D_X(i_texture), const float i_rawDepth, const uint2 i_positionSS)
{
	float rawDepth = i_rawDepth;

	if (_CrestDepthTextureOffset > 0)
	{
		int3 offset = int3(-_CrestDepthTextureOffset, 0, _CrestDepthTextureOffset);

		rawDepth = CREST_DEPTH_COMPARE(rawDepth, LOAD_TEXTURE2D_X(i_texture, i_positionSS + offset.xy).r);
		rawDepth = CREST_DEPTH_COMPARE(rawDepth, LOAD_TEXTURE2D_X(i_texture, i_positionSS + offset.yx).r);
		rawDepth = CREST_DEPTH_COMPARE(rawDepth, LOAD_TEXTURE2D_X(i_texture, i_positionSS + offset.yz).r);
		rawDepth = CREST_DEPTH_COMPARE(rawDepth, LOAD_TEXTURE2D_X(i_texture, i_positionSS + offset.zy).r);
	}

	return rawDepth;
}
#endif // TEXTURE2D_X

#endif // CREST_OCEAN_SHADER_HELPERS_H
