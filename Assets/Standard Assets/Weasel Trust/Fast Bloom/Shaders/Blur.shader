Shader "Weasel Trust/Blur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Spread ("Blur Spread", float) = 0.1
		_SpreadDirection ("Spread Direction", Vector) = (1, 0, 0, 0)
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

			float _Spread;
			float4 _SpreadDirection;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;

				half step = _Spread;
				o.uv_0 = v.uv - 2 * half4(step, 0, 0, 0);
				o.uv_1 = v.uv - half4(step, 0, 0, 0);
				o.uv_2 = v.uv;
				o.uv_3 = v.uv + half4(step, 0, 0, 0);
				o.uv_4 = v.uv + 2 * half4(step, 0, 0, 0);

				#if UNITY_UV_STARTS_AT_TOP
				o.uv_0.y = 1-o.uv_0.y;
				o.uv_1.y = 1-o.uv_1.y;
				o.uv_2.y = 1-o.uv_2.y;
				o.uv_3.y = 1-o.uv_3.y;
				o.uv_4.y = 1-o.uv_4.y;
				#endif

				return o;
			}
			
			sampler2D _MainTex;

			half blurKernel[11] = {0.0009765625, 0.009765625, 0.04394531, 0.1171875, 0.2050781, 0.2460938, 0.2050781, 0.1171875, 0.04394531, 0.009765625, 0.0009765625};

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
