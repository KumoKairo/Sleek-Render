﻿Shader "Weasel Trust/HorizontalBlur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv_0 : TEXCOORD0;
				float2 uv_1 : TEXCOORD1;
				float2 uv_2 : TEXCOORD2;
				float2 uv_3 : TEXCOORD3;
				float2 uv_4 : TEXCOORD4;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;

				half step = 0.05h;
				o.uv_0 = v.uv - 2 * half4(step, 0, 0, 0);
				o.uv_1 = v.uv - half4(step, 0, 0, 0);
				o.uv_2 = v.uv;
				o.uv_3 = v.uv + half4(step, 0, 0, 0);
				o.uv_4 = v.uv + 2 * half4(step, 0, 0, 0);
				return o;
			}
			
			sampler2D _MainTex;

			half4 frag (v2f i) : SV_Target
			{
				half4 tap_0 = tex2D(_MainTex, i.uv_0);
				half4 tap_1 = tex2D(_MainTex, i.uv_1);
				half4 tap_2 = tex2D(_MainTex, i.uv_2);
				half4 tap_3 = tex2D(_MainTex, i.uv_3);
				half4 tap_4 = tex2D(_MainTex, i.uv_4);

				return 0.06136 * tap_0 + 0.24477 * tap_1 + 0.38774 * tap_2 + 0.24477 * tap_3 + 0.06136 * tap_4;
			}
			ENDCG
		}
	}
}
