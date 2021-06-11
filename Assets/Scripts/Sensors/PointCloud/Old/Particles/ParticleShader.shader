Shader "Unlit/TestShader"
{
	Properties
	{
		HueScale("Hue scale", Range(5, 100)) = 10
		HueOffset("Hue Offset", Range(-5, 5)) = 0
	}
		SubShader
	{
		Pass
		{
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma target 5.0;


			#include "UnityCG.cginc"
			//uniform RWStructuredBuffer<float4> lines;
			float HueScale;
			float HueOffset;

            struct v2f
            {
                float4 vertex : SV_POSITION;
				fixed4 color : TEXCOORD0;
            };

			float3 HUEtoRGB(float H)
			{
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R, G, B));
			}

			// https://answers.unity.com/questions/576324/drawing-indexed-vertices-from-compute-shader.html
            v2f vert (uint id: SV_VertexID, float4 vertex : POSITION)
            {
                v2f o;

				o.vertex = UnityObjectToClipPos(vertex);//lines[id]);//lines[0]);
				float f = (float)id;
				o.color = half4(HUEtoRGB((vertex.y+HueOffset)/ HueScale), 1);
				//o.color = half4(sin(f / 10), sin(f / 100), sin(f / 1000), 0) * 0.5 + 0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample  the texture
				return i.color;

            }
            ENDCG
        }
    }
}
