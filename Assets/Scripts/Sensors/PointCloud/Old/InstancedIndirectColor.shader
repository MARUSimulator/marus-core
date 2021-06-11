
// https://toqoz.fyi/thousands-of-meshes.html
Shader "Custom/InstancedIndirectColor" {
	Properties
	{
		HueScale("Hue scale", Range(5, 100)) = 10
		HueOffset("Hue Offset", Range(-5, 5)) = 0
		ParticleSize("Particle Size", Range(0.1, 5)) = 1
		_MainTex("Texture Image", 2D) = "white" {}
	}
    SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1"}

        Pass {
			cull off
			Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
			float HueScale;
			float HueOffset;
			float ParticleSize;

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
            }; 

            struct MeshProperties {
                float4x4 mat;
                float4 color;
            };

			float3 HUEtoRGB(float H)
			{
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R, G, B));
			}

			uniform sampler2D _MainTex;
            StructuredBuffer<MeshProperties> _Properties;

		    // https://www.reddit.com/r/Unity3D/comments/flwreg/how_do_i_make_a_trs_matrix_manually/
			// https://stackoverflow.com/questions/57204343/can-a-shader-rotate-shapes-to-face-camera
			// https://www.youtube.com/watch?v=qGppGvgw7Dg
            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                v2f o;

				// Particle positions
				float4x4 transPos = float4x4(1, 0, 0, _Properties[instanceID].mat[0][3],
												0, 1, 0, _Properties[instanceID].mat[1][3],
												0, 0, 1, _Properties[instanceID].mat[2][3],
												0, 0, 0, 1);
				
				// Make every particle face camera
				float4x4 transRot = float4x4(UNITY_MATRIX_V[0][0], UNITY_MATRIX_V[1][0], UNITY_MATRIX_V[2][0], 0,
											UNITY_MATRIX_V[0][1], UNITY_MATRIX_V[1][1], UNITY_MATRIX_V[2][1], 0,
											UNITY_MATRIX_V[0][2], UNITY_MATRIX_V[1][2], UNITY_MATRIX_V[2][2], 0,
											0, 0, 0, 1);
				float4x4 transSca = float4x4(ParticleSize, 0, 0, 0,
					0, ParticleSize, 0, 0,
					0, 0, ParticleSize, 0,
					0, 0, 0, 1);

				
				float4x4 transMat = mul(transPos, transRot);
				transMat = mul(transMat, transSca);
				float4 pos = mul(transMat, i.vertex);				
				o.vertex = UnityObjectToClipPos(pos);

				//o.color = _Properties[instanceID].color;
				o.color = half4(HUEtoRGB((pos.y + HueOffset) / HueScale), 1);

                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                return i.color;
            }
            
            ENDCG
        }
    }
}