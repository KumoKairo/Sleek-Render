﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/FxProTap" {

Properties {
	_MainTex ("", 2D) = "white" {}
}

Category {
	ZTest Always Cull Off ZWrite Off

	Subshader {
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct v2f {
					float4 pos : SV_POSITION;
					half4 uv[2] : TEXCOORD0;
				};

				float4 _MainTex_TexelSize;
				float4 _BlurOffsets;

				v2f vert (appdata_img v)
				{
					v2f o;
					float offX = _MainTex_TexelSize.x * _BlurOffsets.x;
					float offY = _MainTex_TexelSize.y * _BlurOffsets.y;

					o.pos = UnityObjectToClipPos (v.vertex);
					float2 uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord.xy-float2(offX, offY));
				
					o.uv[0].xy = uv + float2( offX, offY);
					o.uv[0].zw = uv + float2(-offX, offY);
					o.uv[1].xy = uv + float2( offX,-offY);
					o.uv[1].zw = uv + float2(-offX,-offY);
					return o;
				}
				
				sampler2D _MainTex;

				fixed4 frag( v2f i ) : SV_Target
				{
					fixed4 c;
					c  = tex2D( _MainTex, i.uv[0].xy );
					c += tex2D( _MainTex, i.uv[0].zw );
					c += tex2D( _MainTex, i.uv[1].xy );
					c += tex2D( _MainTex, i.uv[1].zw );
					c *= .25;
					return c;
				}
			ENDCG
		}
	}

	Subshader {
		Pass {
			SetTexture [_MainTex] {constantColor [_Color] combine texture * constant alpha}
			SetTexture [_MainTex] {constantColor [_Color] combine texture * constant + previous}
			SetTexture [_MainTex] {constantColor [_Color] combine texture * constant + previous}
			SetTexture [_MainTex] {constantColor [_Color] combine texture * constant + previous}		
		}

	}
}

Fallback off

}