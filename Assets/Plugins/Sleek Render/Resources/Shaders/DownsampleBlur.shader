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

            struct appdata
            {
                half4 vertex : POSITION;
                half4 uv : TEXCOORD0;
            };

            struct v2f
            {
                half4 vertex : SV_POSITION;
                half2 uv_0 : TEXCOORD0;
                half2 uv_1 : TEXCOORD1;
                half2 uv_2 : TEXCOORD2;
                half2 uv_3 : TEXCOORD3;
                half2 uv_4 : TEXCOORD4;
                half2 uv_5 : TEXCOORD5;
                half2 uv_6 : TEXCOORD6;
                half2 uv_7 : TEXCOORD7;
            };

            sampler2D_half _MainTex;
            float4 _TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                half2 halfpixel = _TexelSize.xy;

                o.uv_0 = v.uv + half2(-halfpixel.x * 2.0, 0.0);
                o.uv_1 = v.uv + half2(-halfpixel.x, halfpixel.y);
                o.uv_2 = v.uv + half2(0.0, halfpixel.y * 2.0);
                o.uv_3 = v.uv + half2(halfpixel.x, halfpixel.y);
                o.uv_4 = v.uv + half2(halfpixel.x * 2.0, 0.0);
                o.uv_5 = v.uv + half2(halfpixel.x, -halfpixel.y);
                o.uv_6 = v.uv + half2(0.0, -halfpixel.y * 2.0);
                o.uv_7 = v.uv + half2(-halfpixel.x, -halfpixel.y);

                if (_ProjectionParams.x < 0)
                {
                    o.uv_0.y = 1 - o.uv_0.y;
                    o.uv_1.y = 1 - o.uv_1.y;
                    o.uv_2.y = 1 - o.uv_2.y;
                    o.uv_3.y = 1 - o.uv_3.y;
                    o.uv_4.y = 1 - o.uv_4.y;
                    o.uv_5.y = 1 - o.uv_5.y;
                    o.uv_6.y = 1 - o.uv_6.y;
                    o.uv_7.y = 1 - o.uv_7.y;
                }

                return o;
            }

            half4 _LuminanceConst;

            void getTapAndLumaFrom(half2 uv, out half4 tap)
            {
                tap = tex2D(_MainTex, uv);
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 tap_0, tap_1, tap_2, tap_3, tap_4, tap_5, tap_6, tap_7;

                tap_0 = tex2D(_MainTex, i.uv_0);
                tap_1 = tex2D(_MainTex, i.uv_1);
                tap_2 = tex2D(_MainTex, i.uv_2);
                tap_3 = tex2D(_MainTex, i.uv_3);
                tap_4 = tex2D(_MainTex, i.uv_4);
                tap_5 = tex2D(_MainTex, i.uv_5);
                tap_6 = tex2D(_MainTex, i.uv_6);
                tap_7 = tex2D(_MainTex, i.uv_7);

                half4 result
                    = tap_0
                    + tap_1 * 2.0h
                    + tap_2
                    + tap_3 * 2.0h
                    + tap_4
                    + tap_5 * 2.0h
                    + tap_6
                    + tap_7 * 2.0h;

                return result / 12.0h;
            }
            ENDCG
		}
	}
}
