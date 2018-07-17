Shader "Sleek Render/Post Process/Brightpass Blur"
{
	Properties
	{
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
				half4 uv : TEXCOORD0;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half2 uv[5];
			};

			sampler2D_half _MainTex;
			float4 _TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				half2 halfpixel = _TexelSize.xy;

				o.uv[0] = v.uv;
				o.uv[1] = v.uv + half2(-halfpixel.x, -halfpixel.y);
				o.uv[2] = v.uv + half2(halfpixel.x, halfpixel.y);
				o.uv[3] = v.uv + half2(-halfpixel.x, halfpixel.y);
				o.uv[4] = v.uv + half2(halfpixel.x, -halfpixel.y);

				if (_ProjectionParams.x < 0)
				{
					for(int i = 0; i < 5; i++)
					{
						o.uv[i].y = 1.0h - o.uv[i].y;
					}
				}

				return o;
			}
			
			half4 _LuminanceThreshold;

			half4 getTapAndLumaFrom(half2 uv)
			{
				half4 tap = tex2D(_MainTex, uv);
				half luma = saturate(dot(half4(tap.rgb, 1.0h), _LuminanceThreshold)); 
				return tap *= luma;
			}

			half4 frag (v2f i) : SV_Target
			{
				half4 result = getTapAndLumaFrom(i.uv[0]) * 4.0;

				for(int i = 1; i < 5; i++)
				{
					result += getTapAndLumaFrom(i.uv[i]);
				}
				
				return result / 8.0h;
			}
			ENDCG
		}
	}
}
