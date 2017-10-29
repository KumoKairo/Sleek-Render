Shader "Weasel Trust/Downsample Brightpass"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LuminanceConst("Luminance", Vector) = (1.0, 1.0, 1.0, 1.0)
		_TexelSize("_TexelSize", Vector) = (1.0, 1.0, 1.0, 1.0)
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
				half2 uv_0 : TEXCOORD0;
				half2 uv_1 : TEXCOORD1;
				half2 uv_2 : TEXCOORD2;
				half2 uv_3 : TEXCOORD3;
				half2 uv_4 : TEXCOORD4;
			};

			sampler2D _MainTex;
			float4 _TexelSize, _MainTex_TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				half2 texelSize = _TexelSize.xy;

				o.uv_4 = v.uv;
				o.uv_0 = v.uv - texelSize;
				o.uv_1 = v.uv + texelSize * half2(1, -1);
				o.uv_2 = v.uv + texelSize * half2(-1, 1);
				o.uv_3 = v.uv + texelSize;

				if (_ProjectionParams.x < 0)
				{
					o.uv_0.y = 1-o.uv_0.y;
					o.uv_1.y = 1-o.uv_1.y;
					o.uv_2.y = 1-o.uv_2.y;
					o.uv_3.y = 1-o.uv_3.y;
					o.uv_4.y = 1-o.uv_4.y;
				}

				return o;
			}
			
			half4 _LuminanceConst;

			half4 frag (v2f i) : SV_Target
			{
				half4 col_0 = tex2D(_MainTex, i.uv_0);
				half luma_0 = saturate(dot(half4(col_0.rgb, 1.0h), _LuminanceConst));

				half4 col_1 = tex2D(_MainTex, i.uv_1);
				half luma_1 = saturate(dot(half4(col_1.rgb, 1.0h), _LuminanceConst));

				half4 col_2 = tex2D(_MainTex, i.uv_2);
				half luma_2 = saturate(dot(half4(col_2.rgb, 1.0h), _LuminanceConst));

				half4 col_3 = tex2D(_MainTex, i.uv_3);
				half luma_3 = saturate(dot(half4(col_3.rgb, 1.0h), _LuminanceConst));

				half4 col_4 = tex2D(_MainTex, i.uv_4);
				half luma_4 = saturate(dot(half4(col_4.rgb, 1.0h), _LuminanceConst));

				half4 col = (col_0 + col_1 + col_2 + col_3 + col_4) * 0.2;
				col.a = (luma_0 + luma_1 + luma_2 + luma_3 + luma_4) * 0.2h;

				return col;
				return half4(col.a, col.a, col.a, col.a);
			}
			ENDCG
		}
	}
}
