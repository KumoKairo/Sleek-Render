Shader "Sleek Render/Post Process/Downsample Blur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BloomTex("Bloom", 2D) = "black" {}
		_TexelSize("Texel Size", vector) = (0, 0, 0, 0)
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                half2 halfpixel = _TexelSize.xy;

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

            half4 frag(v2f i) : SV_Target
            {
                return applyUpsampleTapLogic(i.uv, _MainTex);
            }
            ENDCG
		}
	}
}
