
Shader "Sleek Render/Post Process/Upsample Dual filter" {
	Properties {
		_Intensity("Iteration", float) = 1
		_TexelSize("Texel Size", vector) = (0, 0, 0, 0)
		_MainTex("Texture", 2D) = "white" {}

	}
	
	SubShader{

			Pass{

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			half _Intensity;
			half4 _TexelSize;
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
				half2 uv_4 : TEXCOORD5;
				half2 uv_5 : TEXCOORD6;
				half2 uv_6 : TEXCOORD7;
				half2 uv_7 : TEXCOORD8;
				
			};

			v2f vert(appdata v) {
				v2f o;
				o.vertex = v.vertex;
				o.uv = v.uv;
				half2 duv = _Intensity * _TexelSize;
				o.uv_0 = v.uv - half2(-duv.x * 2.0, 0.0);
				o.uv_1 = v.uv + half2(-duv.x,duv.y);
				o.uv_2 = v.uv + half2(0.0, duv.y * 2.0);
				o.uv_3 = v.uv + half2(duv.x, duv.y);
				o.uv_4 = v.uv + half2(duv.x * 2.0,0.0);
				o.uv_5 = v.uv + half2(duv.x,-duv.y);
				o.uv_6 = v.uv + half2(0.0, -duv.y * 2.0);
				o.uv_7 = v.uv + half2(-duv.x, -duv.y);

				if (_ProjectionParams.x < 0)
				{
					o.uv.y = 1 - o.uv.y;
					o.uv_0.y = 1 - o.uv_0.y;
					o.uv_1.y = 1 - o.uv_1.y;
					o.uv_2.y = 1 - o.uv_2.y;
					o.uv_3.y = 1 - o.uv_3.y;
					o.uv_4.y = 1 - o.uv_4.y;
					o.uv_5.y = 1 - o.uv_5.y;
					o.uv_6.y = 1 - o.uv_6.y;
					o.uv_7.y = 1 - o.uv_7.y;
				}

				return o;

			}

			half4 frag(v2f i) : SV_TARGET{
				
				
				half4 tap_0 = tex2D(_MainTex, i.uv_0);
				half4 tap_1 = tex2D(_MainTex, i.uv_1) * 2.0;
				half4 tap_2 = tex2D(_MainTex, i.uv_2);
				half4 tap_3 = tex2D(_MainTex, i.uv_3)*2.0;
				half4 tap_4 = tex2D(_MainTex, i.uv_4);
				half4 tap_5 = tex2D(_MainTex, i.uv_5)*2.0;
				half4 tap_6 = tex2D(_MainTex, i.uv_6);
				half4 tap_7 = tex2D(_MainTex, i.uv_7)*2.0;

				half4 result = tap_0 + tap_1 + tap_2 + tap_3
				+ tap_4 + tap_5 + tap_6 + tap_7;
				
				result = result/12.0;
				return result;
			}



			ENDCG
			}
	}
	
}
