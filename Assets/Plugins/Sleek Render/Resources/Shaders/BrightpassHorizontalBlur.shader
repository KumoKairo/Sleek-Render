﻿Shader "Sleek Render/Post Process/Brightpass Horizontal Blur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LuminanceConst("Luminance", Vector) = (1.0, 1.0, 1.0, 1.0)
		_TexelSize("_TexelSize", Vector) = (1.0, 1.0, 1.0, 1.0)
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

			v2f vert (appdata v)
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
					o.uv_0.y = 1-o.uv_0.y;
					o.uv_1.y = 1-o.uv_1.y;
					o.uv_2.y = 1-o.uv_2.y;
					o.uv_3.y = 1-o.uv_3.y;
					o.uv_4.y = 1-o.uv_4.y;
					o.uv_5.y = 1-o.uv_5.y;
					o.uv_6.y = 1-o.uv_6.y;
					o.uv_7.y = 1-o.uv_7.y;
				}

				return o;
			}
			
			half4 _LuminanceConst;

			void getTapAndLumaFrom(half2 uv, out half4 tap, out half luma)
			{
				tap = tex2D(_MainTex, uv);
				luma = saturate(dot(half4(tap.rgb, 1.0h), _LuminanceConst)); 
				tap *= luma;
			}

			half4 frag (v2f i) : SV_Target
			{
				half4 tap_0, tap_1, tap_2, tap_3, tap_4, tap_5, tap_6, tap_7; 
				half luma_0, luma_1, luma_2, luma_3, luma_4, luma_5, luma_6, luma_7; 

				getTapAndLumaFrom(i.uv_0, tap_0, luma_0);
				getTapAndLumaFrom(i.uv_1, tap_1, luma_1);
				getTapAndLumaFrom(i.uv_2, tap_2, luma_2);
				getTapAndLumaFrom(i.uv_3, tap_3, luma_3);
				getTapAndLumaFrom(i.uv_4, tap_4, luma_4);
				getTapAndLumaFrom(i.uv_5, tap_5, luma_5);
				getTapAndLumaFrom(i.uv_6, tap_6, luma_6);
				getTapAndLumaFrom(i.uv_7, tap_7, luma_7);

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
