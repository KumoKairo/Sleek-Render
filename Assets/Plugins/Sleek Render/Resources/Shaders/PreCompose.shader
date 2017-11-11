Shader "Sleek Render/Post Process/PreCompose"
{
	Properties
	{
		_BloomTex("Bloom", 2D) = "black" {}
		_VignetteForm("Vignette Form", vector) = (1.0, 1.0, 1.0, 1.0)
		_VignetteColor("Vignette Color", color) = (0.0, 0.0, 0.0, 1.0)
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
			
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				half2 uv : TEXCOORD0;
			};

			struct v2f
			{
				half2 uv : TEXCOORD0;
				half4 vertex : SV_POSITION;
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
			
			sampler2D _BloomTex;
			half4 _VignetteForm, _VignetteColor;

			half4 frag (v2f i) : SV_Target
			{
				half4 mainColor = tex2D(_BloomTex, i.uv);
				return mainColor;
			}
			ENDCG
		}
	}
}
