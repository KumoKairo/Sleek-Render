Shader "Unlit/IDTR Unlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv0 = v.uv+ float2(0.1, 0.1);
				o.uv1 = v.uv+ float2(0.2, 0.2);
				o.uv2 = v.uv+ float2(0.3, 0.3);
				o.uv3 = v.uv+ float2(0.4, 0.4);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col0 = tex2D(_MainTex, i.uv0);
				fixed4 col1 = tex2D(_MainTex, i.uv1);
				fixed4 col2 = tex2D(_MainTex, i.uv2);
				fixed4 col3 = tex2D(_MainTex, i.uv3);
				return (col0 + col1 + col2 + col3);
			}
			ENDCG
		}
	}
}
