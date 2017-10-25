Shader "Unlit/ThreeChannelUnlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Luminance ("Luminance", Vector) = (1, 1, 1, 1)
		_TentColor ("Tent Light Color", Color) = (1, 1, 1, 1)
		_BonfireColor ("Bonfire Color", Color) = (1, 1, 1, 1)
		_FillLightColor ("Fill Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				half2 uv : TEXCOORD0;
			};

			struct v2f
			{
				half2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				half4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			half4 _MainTex_ST, _Luminance, _TentColor, _BonfireColor, _FillLightColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				// sample the texture
				half4 col = tex2D(_MainTex, i.uv);

				half4 tentColor = col.r * _BonfireColor * _BonfireColor.a;
				half4 bonfireColor = col.g * _TentColor * _TentColor.a;
				half4 fillColor = col.b * _FillLightColor * _FillLightColor.a;
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);

				return tentColor + bonfireColor + fillColor;
			}
			ENDCG
		}
	}
}
