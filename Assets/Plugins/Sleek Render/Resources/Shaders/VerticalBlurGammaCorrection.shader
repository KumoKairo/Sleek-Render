Shader "Sleek Render/Post Process/Vertical Gaussian Blur Gamma Correction"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BloomTex("Bloom", 2D) = "black" {}
		_BloomIntencity("Bloom Intensity", float) = 0.672
		_YSpread("Y Spread", float) = 0
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
				float2 uv_0 : TEXCOORD0;
				float2 uv_1 : TEXCOORD1;
				float2 uv_2 : TEXCOORD2;
				float2 uv_3 : TEXCOORD3;
				float2 uv_4 : TEXCOORD4;
				float2 uv_5 : TEXCOORD5;
				float2 uv_6 : TEXCOORD6;
			};

			float _BloomIntencity, _YSpread;
			sampler2D _MainTex, _BloomTex;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;

				half4 stepVector = half4(0.0h, _YSpread, 0.0h, 0.0h);
				half stepOne = 1.5h;
				half stepTwo = 3.0h;
				half stepThree = 4.2h;
				o.uv_0 = v.uv + stepVector * stepOne;
				o.uv_1 = v.uv - stepVector * stepOne;
				o.uv_2 = v.uv + stepVector * stepTwo;
				o.uv_3 = v.uv - stepVector * stepTwo;
				o.uv_4 = v.uv + stepVector * stepThree;
				o.uv_5 = v.uv - stepVector * stepThree;
				o.uv_6 = v.uv;

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
				half4 tap_0 = tex2D(_BloomTex, i.uv_6);
				half4 tap_1 = tex2D(_BloomTex, i.uv_1);
				half4 tap_2 = tex2D(_BloomTex, i.uv_2);
				half4 tap_3 = tex2D(_BloomTex, i.uv_3);
				half4 tap_4 = tex2D(_BloomTex, i.uv_4);
				half4 tap_5 = tex2D(_BloomTex, i.uv_5);
				half4 tap_6 = tex2D(_BloomTex, i.uv_0);

				half4 bloomColor = 
					tap_4 * 0.015625 + tap_5 * 0.015625
					+ tap_3 * 0.0937 + tap_2 * 0.0937
					+ tap_6 * 0.234375 + tap_1 * 0.234375
					+ tap_0 * 0.3125;

				half4 color = tex2D(_MainTex, i.uv_6);
				half luma = dot(color.rgb + bloomColor.rgb * _BloomIntencity, half3(0.2126h, 0.7152h, 0.0722h));
				half gammaCorrectionFactor = 0.933033;
				half gammaCorrectionPower = 0.05;
				half gammaCompression = gammaCorrectionFactor * pow(luma, gammaCorrectionPower);
				half4 bloom = bloomColor * gammaCompression * _BloomIntencity;

				return bloom;
			}
			ENDCG
		}
	}
}
