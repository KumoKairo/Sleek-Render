Shader "Sleek Render/Post Process/Kawase"
{
	Properties
	{
		_Iteration("Iteration", float) = 1
		_TexelSize("Texel Size", vector) = (0, 0, 0, 0)
		_MainTex("Texture", 2D) = "white" {}

	}
		SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
	{
		CGPROGRAM

#pragma vertex vert
#pragma fragment frag

		half _Iteration;
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
		half2 uv_0 : TEXCOORD0;
		half2 uv_1 : TEXCOORD1;
		half2 uv_2 : TEXCOORD2;
		half2 uv_3 : TEXCOORD3;
	};

	v2f vert(appdata v) {
		v2f o;
		o.vertex = v.vertex;
		float2 halfTexelSize = _TexelSize / 2; 				//Check if possible _TexelSize/2
		float2 duv = (_TexelSize * (_Iteration - 1)) + halfTexelSize;

		//top left
		o.uv_0.x = v.uv.x - duv.x;
		o.uv_0.y = v.uv.y + duv.y;

		//bottom left
		o.uv_1.x = v.uv.x - duv.x;
		o.uv_1.y = v.uv.y - duv.y;

		//top right
		o.uv_2.x = v.uv.x + duv.x;
		o.uv_2.y = v.uv.y + duv.y;

		//bottom right
		o.uv_3.x = v.uv.x + duv.x;
		o.uv_3.y = v.uv.y - duv.y;

		if (_ProjectionParams.x < 0)
		{
			o.uv_0.y = 1 - o.uv_0.y;
			o.uv_1.y = 1 - o.uv_1.y;
			o.uv_2.y = 1 - o.uv_2.y;
			o.uv_3.y = 1 - o.uv_3.y;
		}


		return o;
	}

	half4 frag(v2f i) : SV_TARGET{
		half4 tap_0 = tex2D(_MainTex, i.uv_0);
		half4 tap_1 = tex2D(_MainTex, i.uv_1);
		half4 tap_2 = tex2D(_MainTex, i.uv_2);
		half4 tap_3 = tex2D(_MainTex, i.uv_3);

		half4 result = tap_0 + tap_1 + tap_2 + tap_3;
		result = 0.25 * result;
		return result;
	}


		ENDCG

	}
	}
}
