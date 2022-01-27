// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

Shader "Hidden/Crest/Underwater/Underwater Effect HDRP"
{
	SubShader
	{
		Cull Off ZWrite Off ZTest Always Blend Off

		HLSLINCLUDE
		#pragma multi_compile_instancing

		// Use multi_compile because these keywords are copied over from the ocean material. With shader_feature,
		// the keywords would be stripped from builds. Unused shader variants are stripped using a build processor.
		#pragma multi_compile_local __ CREST_CAUSTICS_ON
		#pragma multi_compile_local __ CREST_FLOW_ON
		#pragma multi_compile_local __ CREST_FOAM_ON
		#pragma multi_compile_local __ CREST_COMPILESHADERWITHDEBUGINFO_ON

		#pragma multi_compile_local __ CREST_MENISCUS
		#pragma multi_compile_local __ _FULL_SCREEN_EFFECT
		#pragma multi_compile_local __ _DEBUG_VIEW_OCEAN_MASK

#if _COMPILESHADERWITHDEBUGINFO_ON
		#pragma enable_d3d11_debug_symbols
#endif

		#pragma target 4.5

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

		float4 _CameraDepthTexture_TexelSize;

		#include "../../OceanConstants.hlsl"
		#include "../../OceanInputsDriven.hlsl"
		#include "../../OceanGlobals.hlsl"
		#include "../../OceanHelpersNew.hlsl"
		#include "../../OceanShaderHelpers.hlsl"
		#include "../../ShadergraphFramework/CrestNodeDrivenInputs.hlsl"
		#include "../../ShadergraphFramework/CrestNodeLightWaterVolume.hlsl"
		#include "../../ShadergraphFramework/CrestNodeApplyCaustics.hlsl"
		#include "../../ShadergraphFramework/CrestNodeAmbientLight.hlsl"

		half4 _ScatterColourBase;
		half3 _ScatterColourShadow;
		float4 _ScatterColourShallow;
		half _ScatterColourShallowDepthMax;
		half _ScatterColourShallowDepthFalloff;

		half _SSSIntensityBase;
		half _SSSIntensitySun;
		half4 _SSSTint;
		half _SSSSunFalloff;

		half3 _DepthFogDensity;

		float _CausticsTextureScale;
		float _CausticsTextureAverage;
		float _CausticsStrength;
		float _CausticsFocalDepth;
		float _CausticsDepthOfField;
		float _CausticsDistortionStrength;
		float _CausticsDistortionScale;

		TEXTURE2D_X(_CrestOceanMaskTexture);
		TEXTURE2D_X(_CrestOceanMaskDepthTexture);
		TEXTURE2D_X(_CrestCameraColorTexture);
		TEXTURE2D_X(_CrestCameraDepthTexture);

		TEXTURE2D(_CausticsTexture); SAMPLER(sampler_CausticsTexture); float4 _CausticsTexture_TexelSize;
		TEXTURE2D(_CausticsDistortionTexture); SAMPLER(sampler_CausticsDistortionTexture); float4 _CausticsDistortionTexture_TexelSize;

		#include "../UnderwaterEffectShared.hlsl"

		float LinearToDeviceDepth(float linearDepth, float4 zBufferParam)
		{
			//linear = 1.0 / (zBufferParam.z * device + zBufferParam.w);
			float device = (1.0 / linearDepth - zBufferParam.w) / zBufferParam.z;
			return device;
		}

		half3 ApplyUnderwaterEffect(
			half3 sceneColour,
			const float rawDepth,
			const float sceneZ,
			const half3 view,
			const float2 screenPos,
			bool isOceanSurface
		) {
			const bool isUnderwater = true;

			half3 volumeLight = 0.0;
			float3 displacement = 0.0;
			{
				// Offset slice so that we dont get high freq detail. But never use last lod as this has crossfading.
				int sliceIndex = clamp(_DataSliceOffset, 0, _SliceCount - 2);
				float3 uv_slice = WorldToUV(_WorldSpaceCameraPos.xz, _CrestCascadeData[sliceIndex], sliceIndex);
				SampleDisplacements(_LD_TexArray_AnimatedWaves, uv_slice, 1.0, displacement);

				half depth = CREST_OCEAN_DEPTH_BASELINE;
				half2 shadow = 0.0;
				{
					SampleSeaDepth(_LD_TexArray_SeaFloorDepth, uv_slice, 1.0, depth);
	// #if CREST_SHADOWS_ON
					SampleShadow(_LD_TexArray_Shadow, uv_slice, 1.0, shadow);
	// #endif
				}

				half3 ambientLighting = _AmbientLighting;
				ApplyIndirectLightingMultiplier(ambientLighting);

				CrestNodeLightWaterVolume_half
				(
					_ScatterColourBase.xyz,
					_ScatterColourShadow.xyz,
					_ScatterColourShallow.xyz,
					_ScatterColourShallowDepthMax,
					_ScatterColourShallowDepthFalloff,
					_SSSIntensityBase,
					_SSSIntensitySun,
					_SSSTint.xyz,
					_SSSSunFalloff,
					depth,
					shadow,
					1.0, // Skip SSS pinch calculation due to performance concerns.
					view,
					_WorldSpaceCameraPos,
					ambientLighting,
					_PrimaryLightDirection,
					_PrimaryLightIntensity,
					volumeLight
				);
			}

#if CREST_CAUSTICS_ON
			float3 worldPos;
			{

				// HDRP needs a different way to unproject to world space. I tried to put this code into URP but it didnt work on 2019.3.0f1
				float deviceZ = LinearToDeviceDepth(sceneZ, _ZBufferParams);
				PositionInputs posInput = GetPositionInput(screenPos * _ScreenSize.xy, _ScreenSize.zw, deviceZ, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
				worldPos = posInput.positionWS;
#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				worldPos += _WorldSpaceCameraPos;
#endif
			}

			if (rawDepth != 0.0 && !isOceanSurface)
			{
				CrestNodeApplyCaustics_float
				(
					sceneColour,
					worldPos,
					displacement.y + _OceanCenterPosWorld.y,
					_DepthFogDensity,
					_PrimaryLightIntensity,
					_PrimaryLightDirection,
					sceneZ,
					_CausticsTexture,
					_CausticsTextureScale,
					_CausticsTextureAverage,
					_CausticsStrength,
					_CausticsFocalDepth,
					_CausticsDepthOfField,
					_CausticsDistortionTexture,
					_CausticsDistortionStrength,
					_CausticsDistortionScale,
					isUnderwater,
					sceneColour
				);
			}
	#endif // CREST_CAUSTICS_ON

			return lerp(sceneColour, volumeLight * GetCurrentExposureMultiplier(), saturate(1.0 - exp(-_DepthFogDensity.xyz * sceneZ)));
		}
		ENDHLSL

		Pass
		{
			Name "Underwater Post Process"

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			struct Attributes
			{
				uint vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv         : TEXCOORD0;
				float3 viewWS     : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings Vert(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
				output.uv = GetFullScreenTriangleTexCoord(input.vertexID);

				// Compute world space view vector.
				output.viewWS = ComputeWorldSpaceView(output.uv);

				return output;
			}

			float4 Frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				uint2 uvScreenSpace = input.positionCS.xy;
				half3 sceneColour = LOAD_TEXTURE2D_X(_CrestCameraColorTexture, uvScreenSpace).rgb;
				float rawDepth = LOAD_TEXTURE2D_X(_CameraDepthTexture, uvScreenSpace).x;
				float mask = LOAD_TEXTURE2D_X(_CrestOceanMaskTexture, uvScreenSpace).x;
				const float rawOceanDepth = LOAD_TEXTURE2D_X(_CrestOceanMaskDepthTexture, uvScreenSpace).x;

				// NOTE: In HDRP PP we get given a depth buffer which contains the depths of rendered transparencies
				// (such as the ocean). We would preferably only have opaque objects in the depth buffer, so that we can
				// more easily tell whether the current pixel is rendering the ocean surface or not.
				// (We would do this by checking if the ocean mask pixel is in front of the scene pixel.)
				//
				// To workaround this. we provide a depth tolerance, to avoid ocean-surface z-fighting. (We assume that
				// anything in the depth buffer which has a depth within the ocean tolerance to the ocean mask itself is
				// the ocean).
				//
				// FUTURE NOTE: This issue is easily avoided with a small modification to HDRenderPipeline.cs
				// Look at the RenderPostProcess() method.
				// We get given m_SharedRTManager.GetDepthStencilBuffer(), having access
				// m_SharedRTManager.GetDepthTexture() would immediately resolve this issue.
				// - Tom Read Cutting - 2020-01-03
				//
				// FUTURE NOTE: The depth texture was accessible through reflection. But could no longer be accessed
				// with future HDRP versions.
				// - Dale Eidd - 2020-11-09
				const float oceanDepthTolerance = 0.0001;

				bool isOceanSurface; bool isUnderwater; float sceneZ;
				GetOceanSurfaceAndUnderwaterData(uvScreenSpace, rawOceanDepth, mask, rawDepth, isOceanSurface, isUnderwater, sceneZ, oceanDepthTolerance);

				const float wt = ComputeMeniscusWeight(uvScreenSpace, mask, _HorizonNormal, sceneZ);

#if _DEBUG_VIEW_OCEAN_MASK
				return DebugRenderOceanMask(isOceanSurface, isUnderwater, mask, sceneColour);
#endif // _DEBUG_VIEW_OCEAN_MASK

				if (isUnderwater)
				{
					const half3 view = normalize(input.viewWS);
					sceneColour = ApplyUnderwaterEffect(sceneColour, rawDepth, sceneZ, view, input.uv, isOceanSurface);
				}

				return half4(wt * sceneColour, 1.0);
			}
			ENDHLSL
		}

		Pass
		{
			Name "Underwater Custom Pass"

			HLSLPROGRAM
			#pragma fragment Frag
			#pragma vertex Vert

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

			float4 Frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				float rawDepth = LoadCameraDepth(input.positionCS.xy);

				PositionInputs posInput = GetPositionInput(input.positionCS.xy, _ScreenSize.zw, rawDepth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
				const half3 view = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
				float2 uv = posInput.positionNDC.xy;

				uint2 uvScreenSpace = input.positionCS.xy;
				half3 sceneColour = LOAD_TEXTURE2D_X(_CrestCameraColorTexture, uvScreenSpace).rgb;
				float mask = LOAD_TEXTURE2D_X(_CrestOceanMaskTexture, uvScreenSpace).x;
				const float rawOceanDepth = LOAD_TEXTURE2D_X(_CrestOceanMaskDepthTexture, uvScreenSpace).x;

				bool isOceanSurface; bool isUnderwater; float sceneZ;
				GetOceanSurfaceAndUnderwaterData(uvScreenSpace, rawOceanDepth, mask, rawDepth, isOceanSurface, isUnderwater, sceneZ, 0);

				const float wt = ComputeMeniscusWeight(uvScreenSpace, mask, _HorizonNormal, sceneZ);

#if _DEBUG_VIEW_OCEAN_MASK
				return DebugRenderOceanMask(isOceanSurface, isUnderwater, mask, sceneColour);
#endif // _DEBUG_VIEW_OCEAN_MASK

				if (isUnderwater)
				{
					sceneColour = ApplyUnderwaterEffect(sceneColour, rawDepth, sceneZ, view, uv, isOceanSurface);
				}

				return half4(wt * sceneColour, 1.0);
			}
			ENDHLSL
		}
	}
	Fallback Off
}
