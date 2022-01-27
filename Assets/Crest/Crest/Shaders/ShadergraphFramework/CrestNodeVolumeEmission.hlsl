// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#include "OceanGraphConstants.hlsl"
#include "../OceanShaderHelpers.hlsl"

// We take the unrefracted scene colour (i_sceneColourUnrefracted) as input because having a Scene Colour node in the graph
// appears to be necessary to ensure the scene colours are bound?
void CrestNodeSceneColour_half
(
	in const half i_refractionStrength,
	in const half3 i_scatterCol,
	in const half3 i_normalTS,
	in const float4 i_screenPos,
	in const float i_pixelZ,
	in const half3 i_sceneColourUnrefracted,
	in const float i_sceneZ,
	in const float i_deviceSceneZ,
	in const bool i_underwater,
	out half3 o_sceneColour,
	out float o_sceneDistance,
	out float3 o_scenePositionWS
)
{
	//#if _TRANSPARENCY_ON

	// View ray intersects geometry surface either above or below ocean surface

	half2 refractOffset = i_refractionStrength * i_normalTS.xy;
	if (!i_underwater)
	{
		// We're above the water, so behind interface is depth fog
		refractOffset *= min(1.0, 0.5 * (i_sceneZ - i_pixelZ)) / i_sceneZ;
	}
	const float4 screenPosRefract = i_screenPos + float4(refractOffset, 0.0, 0.0);
	const float sceneZRefractDevice = SHADERGRAPH_SAMPLE_SCENE_DEPTH(screenPosRefract.xy);

	// Depth fog & caustics - only if view ray starts from above water
	if (!i_underwater)
	{
		float sceneZRefract = CrestLinearEyeDepth(sceneZRefractDevice);

		// Compute depth fog alpha based on refracted position if it landed on an underwater surface, or on unrefracted depth otherwise
		if (sceneZRefract > i_pixelZ)
		{
			// NOTE: For HDRP, refractions produce an outline which requires multisampling with a two pixel offset to
			// cover. This is without MSAA. A deeper investigation is needed.
			o_sceneDistance = CrestLinearEyeDepth(CrestMultiSampleSceneDepth(sceneZRefractDevice, screenPosRefract.xy, i_refractionStrength)) - i_pixelZ;

			o_sceneColour = SHADERGRAPH_SAMPLE_SCENE_COLOR(screenPosRefract.xy);

			// HDRP needs a different way to unproject to world space. I tried to put this code into URP but it didnt work on 2019.3.0f1
			PositionInputs posInput = GetPositionInput(screenPosRefract.xy * _ScreenSize.xy, _ScreenSize.zw, sceneZRefractDevice, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
			o_scenePositionWS = posInput.positionWS;
#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
			o_scenePositionWS += _WorldSpaceCameraPos;
#endif
		}
		else
		{
			// It seems that when MSAA is enabled this can sometimes be negative
			o_sceneDistance = max(CrestLinearEyeDepth(CrestMultiSampleSceneDepth(i_deviceSceneZ, i_screenPos.xy)) - i_pixelZ, 0.0);

			o_sceneColour = i_sceneColourUnrefracted;

			// HDRP needs a different way to unproject to world space. I tried to put this code into URP but it didnt work on 2019.3.0f1
			PositionInputs posInput = GetPositionInput(i_screenPos.xy * _ScreenSize.xy, _ScreenSize.zw, i_deviceSceneZ, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
			o_scenePositionWS = posInput.positionWS;
#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
			o_scenePositionWS += _WorldSpaceCameraPos;
#endif
		}
	}
	else
	{
		// Depth fog is handled by underwater shader
		o_sceneDistance = i_pixelZ;
		o_sceneColour = SHADERGRAPH_SAMPLE_SCENE_COLOR(screenPosRefract.xy);

		// HDRP needs a different way to unproject to world space. I tried to put this code into URP but it didnt work on 2019.3.0f1
		PositionInputs posInput = GetPositionInput(screenPosRefract.xy * _ScreenSize.xy, _ScreenSize.zw, sceneZRefractDevice, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
		o_scenePositionWS = posInput.positionWS;
#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
		o_scenePositionWS += _WorldSpaceCameraPos;
#endif
	}

	//#endif // _TRANSPARENCY_ON
}
