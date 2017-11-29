Shader "Sleek Render/Post Process/PreCompose"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "black" {}
		_BloomTex("Bloom", 2D) = "black" {}
		_GammaCompressionPower("Gamma Compression Power", float) = 0.05
		_GammaCompressionFactor("Gamma Compression Factor", float) = 0.933033
		_BloomIntencity("Bloom Intensity", float) = 0.672
		_VignetteShape("Vignette Form", vector) = (1.0, 1.0, 1.0, 1.0)
		_VignetteColor("Vignette Color", color) = (0.0, 0.0, 0.0, 1.0)
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				half2 uv : TEXCOORD0;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				o.uv = v.uv;

				if (_ProjectionParams.x < 0)
				{
					o.uv.y = 1 - o.uv.y;
				}

				return o;
			}
			
			sampler2D _BloomTex, _MainTex;
			half4 _VignetteShape, _VignetteColor;
			half _GammaCompressionFactor, _GammaCompressionPower, _BloomIntencity;

			half4 frag (v2f i) : SV_Target
			{
				half2 vignetteCenter = i.uv - half2(0.5h, 0.5h);
				half vignetteShape = saturate(dot(vignetteCenter, vignetteCenter) * _VignetteShape.x + _VignetteShape.y);
				half4 rawBloom = tex2D(_BloomTex, i.uv);

				half4 vignette = half4(_VignetteColor.rgb * vignetteShape, 1.0h - _VignetteColor.a * vignetteShape);
				half3 mainColor = tex2D(_MainTex, i.uv).rgb;
				half3 bloom = rawBloom * _BloomIntencity;

				half gammaCorrection = _GammaCompressionFactor * pow(dot(mainColor + bloom, half3(0.2126h, 0.7152h, 0.0722h)), _GammaCompressionPower);
				half4 result = half4(bloom * gammaCorrection * vignette.a + vignette.rgb, vignette.a);
				return result;
			}
			ENDCG
		}
	}
}
