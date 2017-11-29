Shader "Sleek Render/Post Process/Compose"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PreComposeTex("PreCompose Texture", 2D) = "black" {}
		_Colorize("Colorize", color) = (1.0, 1.0, 1.0, 0.0)
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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
			
			sampler2D _MainTex, _PreComposeTex;
			half4 _Colorize;

			half4 frag (v2f i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);
				half4 precompose = tex2D(_PreComposeTex, i.uv);
				half3 mainColor = col.rgb * precompose.a + precompose.rgb;

				half3 result = mainColor * (1.0h - _Colorize.a) + _Colorize.a * _Colorize.rgb * dot(half3(0.2126h, 0.7152h, 0.0722h), mainColor);

				return half4(result, 1.0h);
			}
			ENDCG
		}
	}
}
