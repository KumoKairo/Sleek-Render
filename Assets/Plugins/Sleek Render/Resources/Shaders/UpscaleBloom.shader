Shader "Sleek Render/Post Process/Upscale Bloom"
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
			};

			half2 _MainTex_TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				half2 offset = half2(0.229h, 0.48h);
				o.uv_0 = v.uv + _MainTex_TexelSize.xy * offset;
				o.uv_1 = v.uv - _MainTex_TexelSize.xy * offset;
				o.uv_2 = v.uv + _MainTex_TexelSize.xy * half2(-offset.x, offset.y);
				o.uv_3 = v.uv + _MainTex_TexelSize.xy * half2(offset.x, -offset.y);
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col_0 = tex2D(_MainTex, i.uv_0);
				//fixed4 col_1 = tex2D(_MainTex, i.uv_1);
				//fixed4 col_2 = tex2D(_MainTex, i.uv_2);
				//fixed4 col_3 = tex2D(_MainTex, i.uv_3);
				return col_0;
				//return (col_0 + col_1 + col_2 + col_3) * 0.25h;
			}
			ENDCG
		}
	}
}
