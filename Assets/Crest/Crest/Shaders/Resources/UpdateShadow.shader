// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

Shader "Hidden/Crest/Simulation/Update Shadow"
{
	SubShader
	{
		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			// TODO: We might be able to expose this to give developers the option.
			// #pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH

			// Low quality should be good enough since we jitter the results.
			#define SHADOW_LOW

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/HDShadow.hlsl"

			#include "../OceanConstants.hlsl"
			#include "../OceanGlobals.hlsl"
			#include "../OceanInputsDriven.hlsl"
			#include "../OceanHelpersNew.hlsl"
			// noise functions used for jitter
			#include "../GPUNoise/GPUNoise.hlsl"

			StructuredBuffer<CascadeParams> _CascadeDataSrc;

			CBUFFER_START(CrestPerMaterial)
			// Settings._jitterDiameterSoft, Settings._jitterDiameterHard, Settings._currentFrameWeightSoft, Settings._currentFrameWeightHard
			float4 _JitterDiameters_CurrentFrameWeights;
			float _SimDeltaTime;

			float3 _CenterPos;
			float3 _Scale;
			uint _LD_SliceIndex_Source;
			float4x4 _MainCameraProjectionMatrix;
			float4x4 _CrestViewProjectionMatrix;
			CBUFFER_END

			struct Attributes
			{
				uint vertexID : SV_VertexID;
				float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 _MainCameraCoords : TEXCOORD0;
				float3 _WorldPos : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			half ComputeShadow
			(
				in const float3 i_positionWS,
				in const float i_jitterDiameter,
				in const HDShadowContext i_shadows,
				in const DirectionalLightData i_light
			)
			{
				half shadows = 1.0;
				float3 positionWS = i_positionWS;

				if (i_jitterDiameter > 0.0)
				{
					// Add jitter.
					positionWS.xz += i_jitterDiameter * (hash33(uint3(abs(positionWS.xz * 10.0), _Time.y * 120.0)) - 0.5).xy;
				}

				// Zeros are for screen space position and world space normal. Position is for filtering and normal
				// is for normal bias. They did not appear to have an impact. But we might want to revisit.
				shadows = GetDirectionalShadowAttenuation(i_shadows, 0, positionWS, 0, _DirectionalShadowIndex, -i_light.forward);
				// Apply shadow strength from main light.
				shadows = LerpWhiteTo(shadows, i_light.shadowDimmer);

				return shadows;
			}

			Varyings Vert(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				// Use a custom matrix which is the value of unity_MatrixVP from the frame debugger.
				output.positionCS = mul(_CrestViewProjectionMatrix, float4(input.positionOS, 1.0));

				// World position from [0,1] quad
				output._WorldPos.xyz = float3(input.positionOS.x - 0.5, 0.0, input.positionOS.y - 0.5) * _Scale * 4.0 + _CenterPos;
				output._WorldPos.y = _OceanCenterPosWorld.y;
				output._MainCameraCoords = mul(_MainCameraProjectionMatrix, float4(output._WorldPos.xyz, 1.0));

				return output;
			}

			real2 Frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				const CascadeParams cascadeDataSrc = _CascadeDataSrc[_LD_SliceIndex_Source];

				half2 shadow = 0.0;
				const half r_max = 0.5 - cascadeDataSrc._oneOverTextureRes;

				float3 positionWS = input._WorldPos.xyz;

				float depth;
				{
					float width; float height;
					_LD_TexArray_Shadow_Source.GetDimensions(width, height, depth);
				}

				// Shadow from last frame - manually implement black border.
				const float3 uv_source = WorldToUV(positionWS.xz, cascadeDataSrc, _LD_SliceIndex_Source);
				half2 r = abs(uv_source.xy - 0.5);
				if (max(r.x, r.y) <= r_max)
				{
					SampleShadow(_LD_TexArray_Shadow_Source, uv_source, 1.0, shadow);
				}
				else if (_LD_SliceIndex_Source + 1.0 < depth)
				{
					const float3 uv_source_nextlod = WorldToUV(positionWS.xz, _CascadeDataSrc[_LD_SliceIndex_Source + 1], _LD_SliceIndex_Source + 1);
					half2 r2 = abs(uv_source_nextlod.xy - 0.5);
					if (max(r2.x, r2.y) <= r_max)
					{
						SampleShadow(_LD_TexArray_Shadow_Source, uv_source_nextlod, 1.0, shadow);
					}
				}

				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
					positionWS -= _WorldSpaceCameraPos.xyz;
				#endif

				// Check if the current sample is visible in the main camera (and therefore shadow map can be sampled). This
				// is required as the shadow buffer is world aligned and surrounds viewer.
				float3 projected = input._MainCameraCoords.xyz / input._MainCameraCoords.w;
				if (projected.z < 1.0 && projected.z > 0.0 && abs(projected.x) < 1.0 && abs(projected.y) < 1.0)
				{
					// Get directional light data. By definition we only have one directional light casting shadow.
					DirectionalLightData light = _DirectionalLightDatas[_DirectionalShadowIndex];
					HDShadowContext shadowContext = InitShadowContext();

					half2 shadowThisFrame;

					// Add soft shadowing data.
					shadowThisFrame[CREST_SHADOW_INDEX_SOFT] = ComputeShadow
					(
						positionWS,
						_JitterDiameters_CurrentFrameWeights[CREST_SHADOW_INDEX_SOFT],
						shadowContext,
						light
					);

					// Add hard shadowing data.
					shadowThisFrame[CREST_SHADOW_INDEX_HARD] = ComputeShadow
					(
						positionWS,
						_JitterDiameters_CurrentFrameWeights[CREST_SHADOW_INDEX_HARD],
						shadowContext,
						light
					);

					shadowThisFrame = (half2)1.0 - saturate(shadowThisFrame);

					shadow = lerp(shadow, shadowThisFrame, _JitterDiameters_CurrentFrameWeights.zw * _SimDeltaTime * 60.0);
				}

				return shadow;
			}
			ENDHLSL
		}
	}
	Fallback Off
}
