Shader "Hidden/Shader/UnderwaterPP"
{
    Properties
    {
        // This property is necessary to make the CommandBuffer.Blit bind the source texture to _MainTex
        _MainTex("Main Texture", 2D) = "white" {}
        _NoiseScale("NoiseScale", float) = 1
        _NoiseFrequency("NoiseFrequency", float) = 1
        _NoiseSpeed("NoiseSpeed", float) = 1
        _PixelOffset("Pixel Offset", float) = 1
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"
    #include "noiseSimplex.cginc"
    //#include "UnityCG.cginc"

    #define M_PI 3.1415926533589
    

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        float4 scrPos : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    float4 ComputeScreenPos(float4 pos) {
        float4 o = pos * 0.5f;
        #if defined(UNITY_HALF_TEXEL_OFFSET)
                o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w * _ScreenParams.zw;
        #else
                o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
        #endif

        o.zw = pos.zw;
        return o;
    }

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        output.scrPos = ComputeScreenPos(output.positionCS);
        return output;
    }

    

    float GetDepth(uint2 positionSS) 
    {
        return LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS).x;
    }

    float3 GetNormalWorldSpace(uint2 positionSS) 
    {
        float3 normalWS = 0.0f;
        if (GetDepth(positionSS) > 0) 
        { 
            NormalData normalData;
            const float4 normalBuffer = LOAD_TEXTURE2D_X(_NormalBufferTexture, positionSS);
            DecodeFromNormalBuffer(normalBuffer, positionSS, normalData);
            normalWS = normalData.normalWS;
        }

        return normalWS;
    }

    sampler2D _MainTexSampler;
    SamplerState sampler_MainTex; // "sampler" + �_MainTex�

    // List of properties to control your post process effect
    float _Intensity;
    uniform float _NoiseScale, _NoiseFrequency, _NoiseSpeed, _PixelOffset;
    TEXTURE2D_X(_MainTex);
    

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // Note that if HDUtils.DrawFullScreen is used to render the post process, use ClampAndScaleUVForBilinearPostProcessTexture(input.texcoord.xy) to get the correct UVs

        //uint2 uv = ClampAndScaleUVForBilinearPostProcessTexture(input.texcoord.xy)

        uint2 positionSS = input.texcoord * _ScreenSize.xy;

        //float3 sourceColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, input.texcoord).xyz;

        float depth = GetDepth(positionSS);

        float3 normal = GetNormalWorldSpace(positionSS);
  
        float3 spos = float3(input.texcoord.x, input.texcoord.y, 0) * _NoiseFrequency;

        spos.z += _Time.x * _NoiseSpeed;
        float noise = _NoiseScale * ((snoise(spos) + 1) / 2);
        
        float4 noiseToDirection = float4(cos(noise * M_PI * 2), sin(noise * M_PI * 2), 0, 0);
        
        
        float3 sourceColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, input.texcoord + (normalize(noiseToDirection) * _PixelOffset)).xyz;
        float4 col = float4(sourceColor, 1);

        return col;
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "UnderwaterPP"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
