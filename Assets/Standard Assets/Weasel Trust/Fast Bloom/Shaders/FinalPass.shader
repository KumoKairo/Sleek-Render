Shader "Weasel Trust/Final Pass"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BloomTex("Bloom", 2D) = "black" {}
		_BloomIntencity("Bloom Intensity", float) = 1
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
				half4 vertex : POSITION;
				half4 uv : TEXCOORD0;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half4 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				o.uv = v.uv;

				#if UNITY_UV_STARTS_AT_TOP
				o.uv.y = 1-o.uv.y;
				#endif

				return o;
			}
			
			sampler2D _MainTex, _BloomTex;
			float _BloomIntencity;

			half4 frag (v2f i) : SV_Target
			{
				half4 bloom = tex2D(_BloomTex, i.uv);
				half4 color = tex2D(_MainTex, i.uv);
				return color + bloom * bloom.a * _BloomIntencity;
			}
			ENDCG
		}
	}
}
