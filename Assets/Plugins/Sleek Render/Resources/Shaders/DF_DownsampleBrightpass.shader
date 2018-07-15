Shader "Sleek Render/Post Process/Dual Filter Downsample Brightpass" 
{
	Properties 
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Intensity("Iteration", float) = 1
		_LuminanceConst("Luminance", Vector) = (1.0, 1.0, 1.0, 1.0)
		_TexelSize("Texel Size", vector) = (0, 0, 0, 0)

	}
	
	SubShader
	{
			Pass
			{

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			half _Intensity;
			half2 _TexelSize;
			sampler2D_half _MainTex;

			struct appdata
			{
				half4 vertex : POSITION;
				half2 uv : TEXCOORD0;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv_0 : TEXCOORD1;
				half2 uv_1 : TEXCOORD2;
				half2 uv_2 : TEXCOORD3;
				half2 uv_3 : TEXCOORD4;
			};

			v2f vert(appdata v) 
			{
				v2f o;
				o.vertex = v.vertex;
				o.uv = v.uv;
				float2 duv = _Intensity * _TexelSize;
				o.uv_0 = v.uv - duv;
				o.uv_1 = v.uv + duv;
				o.uv_2 = v.uv + half2(duv.x, -duv.y);
				o.uv_3 = v.uv + half2(-duv.x, duv.y);

				if (_ProjectionParams.x < 0)
 				{
 					o.uv_0.y = 1 - o.uv_0.y;
 					o.uv_1.y = 1 - o.uv_1.y;
 					o.uv_2.y = 1 - o.uv_2.y;
 					o.uv_3.y = 1 - o.uv_3.y;
 				}

				
				return o;
			}

			half4 _LuminanceConst;

			void getTapAndLumaFrom(half2 uv, out half4 tap, out half luma)
			{
				tap = tex2D(_MainTex, uv);
				luma = saturate(dot(half4(tap.rgb, 1.0h), _LuminanceConst)); 
				tap *= luma;
			}

			half4 frag(v2f i) : SV_TARGET
			{
				
				half4 tap, tap_0, tap_1, tap_2, tap_3; 
				half luma, luma_0, luma_1, luma_2, luma_3; 

				getTapAndLumaFrom(i.uv, tap, luma);
				getTapAndLumaFrom(i.uv_0, tap_0, luma_0);
				getTapAndLumaFrom(i.uv_1, tap_1, luma_1);
				getTapAndLumaFrom(i.uv_2, tap_2, luma_2);
				getTapAndLumaFrom(i.uv_3, tap_3, luma_3);

				half4 result = (tap * 4.0) + tap_0 + tap_1 + tap_2 + tap_3;
				result = result/8.0;
				return result;
			}



			ENDCG
			}
	}
	
}