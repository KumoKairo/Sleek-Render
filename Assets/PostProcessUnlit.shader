Shader "Unlit/PostProcessUnlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "red"
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Cull Off
		ZTest Always
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			
			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			float4 _Color;
			sampler2D _MainTex, _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				o.color = v.color;
				o.uv = v.uv;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				float luma = dot(col.rgb, float3(0.3, 0.3, 0.3));
				return i.color * luma;
				return float4(luma, luma, luma, 1.0);
			}
			ENDCG
		}
	}
}
