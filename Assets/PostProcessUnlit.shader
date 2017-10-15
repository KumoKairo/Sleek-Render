Shader "Unlit/PostProcessUnlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white"
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Cull Off
		ZTest Always

		Pass
		{
			CGPROGRAM
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
			
			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
