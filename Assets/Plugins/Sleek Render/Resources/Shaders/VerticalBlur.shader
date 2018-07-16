﻿Shader "Sleek Render/Post Process/Vertical Blur"
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
			};

			half2 _TexelSize;
			sampler2D_half _MainTex;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;

				half4 stepVector = half4(0.0h, _TexelSize.y, 0.0h, 0.0h);
				half stepOne = 1.441h;
				half stepTwo = 3.361h;
				half stepThree = 5.04h;

				o.uv_0 = v.uv;
				o.uv_1 = v.uv + stepVector * stepOne;
				o.uv_2 = v.uv - stepVector * stepOne;
				o.uv_3 = v.uv + stepVector * stepTwo;
				o.uv_4 = v.uv - stepVector * stepTwo;
				o.uv_5 = v.uv + stepVector * stepThree;
				o.uv_6 = v.uv - stepVector * stepThree;

				if (_ProjectionParams.x < 0)
				{
					o.uv_0.y = 1-o.uv_0.y;
					o.uv_1.y = 1-o.uv_1.y;
					o.uv_2.y = 1-o.uv_2.y;
					o.uv_3.y = 1-o.uv_3.y;
					o.uv_4.y = 1-o.uv_4.y;
					o.uv_5.y = 1-o.uv_5.y;
					o.uv_6.y = 1-o.uv_6.y;
				}

				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half4 tap_0 = tex2D(_MainTex, i.uv_0);
				half4 tap_1 = tex2D(_MainTex, i.uv_1);
				half4 tap_2 = tex2D(_MainTex, i.uv_2);
				half4 tap_3 = tex2D(_MainTex, i.uv_3);
				half4 tap_4 = tex2D(_MainTex, i.uv_4);
				half4 tap_5 = tex2D(_MainTex, i.uv_5);
				half4 tap_6 = tex2D(_MainTex, i.uv_6);

				half4 result 
					= tap_0 * 0.159h
					+ (tap_1 + tap_2) * 0.263h
					+ (tap_3 + tap_4) * 0.122h
					+ (tap_5 + tap_6) * 0.023h;

				return result;
			}
			ENDCG
		}
	}
}
