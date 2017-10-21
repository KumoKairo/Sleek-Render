Shader "Weasel Trust/Precompose"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BloomTex("Bloom", 2D) = "black" {}
		_BloomIntencity("Bloom Intensity", float) = 0.672
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
				half4 color = tex2D(_MainTex, i.uv);
				half4 bloom = tex2D(_BloomTex, i.uv) * _BloomIntencity;
				//half luma = dot(color.rgb + bloom.rgb, half3(0.2126h, 0.7152h, 0.0722h));
				//half gammaCorrectionFactor = 0.933033;
				//half gammaCorrectionPower = 0.05;
				//half gammaCompression = gammaCorrectionFactor * pow(luma, gammaCorrectionPower);

				//half4 finalColor = color + bloom;

				return color + bloom;
			}
			ENDCG
		}
	}
}
