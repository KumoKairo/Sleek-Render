Shader "Sleek Render/Post Process/Brightpass"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv_0 : TEXCOORD0;
				float2 uv_1 : TEXCOORD1;
				float2 uv_2 : TEXCOORD2;
				float2 uv_3 : TEXCOORD3;
			};
			
			sampler2D _MainTex, _PrevBrightPass;
			float4 _MainTex_TexelSize, _LuminanceConst;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				half2 texelSize = _MainTex_TexelSize.xy;
				o.uv_0 = v.uv.xy - texelSize;
				o.uv_1 = v.uv + texelSize * half2(2, -2);
				o.uv_2 = v.uv + texelSize * half2(-2, 2);
				o.uv_3 = v.uv + texelSize;
				

				if (_ProjectionParams.x < 0)
				{
					o.uv_0.y = 1-o.uv_0.y;
					o.uv_1.y = 1-o.uv_1.y;
					o.uv_2.y = 1-o.uv_2.y;
					o.uv_3.y = 1-o.uv_3.y;
				}

				return o;
			}

			half4 frag (v2f i) : SV_Target
			{
				half4 col_0 = tex2D(_MainTex, i.uv_0);
				half4 col_1 = tex2D(_MainTex, i.uv_1);
				half4 col_2 = tex2D(_MainTex, i.uv_2);
				half4 col_3 = tex2D(_MainTex, i.uv_3);

				half3 final_col = 
					(col_0.rgb * col_0.a 
					+ col_1.rgb * col_1.a 
					+ col_2.rgb * col_2.a 
					+ col_3.rgb * col_3.a) * 0.25h;

				half4 col = half4(final_col, 1.0h);
				return col;
			}
			ENDCG
		}
	}
}
