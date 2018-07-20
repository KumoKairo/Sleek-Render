Shader "Sleek Render/Post Process/Brightpass Blur"
{
	Properties
	{
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "SleekRenderCG.cginc"

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half2 uv[8] : TEXCOORD0;
			};

			sampler2D_half _MainTex;
			float4 _TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;

				calculateUpsampleTapPoints(v, _TexelSize.xy, o.uv);

				if (_ProjectionParams.x < 0)
				{
					for(int i = 0; i < 8; i++)
					{
						o.uv[i].y = 1.0h - o.uv[i].y;
					}
				}

				return o;
			}
			
			half4 _LuminanceThreshold;

			half4 frag (v2f i) : SV_Target
			{
				return applyUpsampleBrightpassTapLogic(i.uv, _MainTex, _LuminanceThreshold);
			}
			ENDCG
		}
	}
}
