Shader "Hidden/DOFPro" {
Properties {
	[HideInInspector]_MainTex ("Base (RGB), Depth (A)", 2D) = "white" {}
    [HideInInspector]_COCTex ("COC Texture (RGBA)", 2D) = "white" {}
	[HideInInspector]_BlurOffsets ("Blur step size", Vector) = (1, 0, 0, 0)
	_FocalDist ("Focal Dist", Float) = .1
	_FocalLength ("Focal Length", Float) = 0.02

	//Bokeh-related properties
	_BokehThreshold("Bokeh Threshold", Float) = .5
	_BokehGain("Bokeh Gain", Float) = 2.0
	_BokehBias("Bokeh Bias", Float) = .5

}

SubShader {
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }

	CGINCLUDE	
		#pragma multi_compile BLUR_RADIUS_10 BLUR_RADIUS_5 BLUR_RADIUS_3

		//#pragma multi_compile FXPRO_HDR_ON FXPRO_HDR_OFF

		#pragma target 3.0
		#pragma glsl

		#include "FxProInclude.cginc"

		#define GAUSSIAN_KERNEL
	
		half _FocalDist;
		half _FocalLength;
		
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		half4 _MainTex_TexelSize;
		
		half _OneOverDepthScale;

		v2f_img vert_img_aa(appdata_img v)
		{
			v2f_img o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord;

			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv.y =  1 - o.uv.y;
			#endif
			return o;
		}
	ENDCG

	Pass { // pass 0
		name "make_coc_texture"
				
		CGPROGRAM
			#pragma vertex vert_img_aa
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			//#define DOF_CLOSE_ONLY
			//#define DOF_FAR_ONLY

			#pragma multi_compile USE_CAMERA_DEPTH_TEXTURE DONT_USE_CAMERA_DEPTH_TEXTURE

            #ifdef USE_CAMERA_DEPTH_TEXTURE
		    sampler2D _CameraDepthTexture;
            #else 
            //sampler2D _MainTex;
		    #endif
		
		    inline fixed4 CalculateCircleOfConfusion(half2 _uv) {
			    fixed focalDist = _FocalDist;
			 
			    #ifdef USE_CAMERA_DEPTH_TEXTURE
				    fixed depth = Linear01Depth( tex2D(_CameraDepthTexture, _uv).r ) * _OneOverDepthScale;
			    #else
				    fixed depth = tex2D(_MainTex, _uv).a;
			    #endif

				#ifdef DOF_FAR_ONLY
					depth = max(depth, focalDist);
				#elif defined(DOF_CLOSE_ONLY)
					depth = min(depth, focalDist);
				#endif

			    fixed coc = ( abs(depth - focalDist) / depth ) * _FocalLength / saturate(focalDist - _FocalLength);

				coc = saturate(coc);

				return fixed4(coc, depth, .0, .0);
		    }

			fixed4 frag (v2f_img input) : COLOR  {
				return fixed4( CalculateCircleOfConfusion( input.uv ) );
			}

		ENDCG
	}

	Pass { // pass 1
		name "dof_separable_simple"
				
		CGPROGRAM
			#pragma vertex vert_img_aa
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
            sampler2D _COCTex;

			#define OUTPUT_COC_TO_ALPHA
			#ifdef OUTPUT_COC_TO_ALPHA
				#define IGNORE_ALPHA_CHANNEL
			#endif

			#include "SeparableBlur.cginc"

			fixed4 frag (v2f_img input) : COLOR  {
				fixed curCOC = tex2D( _COCTex, input.uv ).r;

				fixed4 res = BlurTex(_MainTex, input, curCOC);

				/*#ifdef OUTPUT_COC_TO_ALPHA
				res.a = curCOC;
				#endif*/

				return res;
			}

		ENDCG
	}
	
	Pass {	//pass 2
		name "blur"
				
		CGPROGRAM
			#pragma vertex vert_img_aa
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			//Blur depth channel as well
			//#define BLUR_ALPHA_CHANNEL
			
			//#define BLUR_RED_ONLY
			
			#include "SeparableBlur.cginc"

			fixed4 frag (v2f_img i) : COLOR  {
				return BlurTex(_MainTex, i, 1.0);
			}
		ENDCG
	}

	Pass {	//pass 3
		name "red_to_all"
				
		CGPROGRAM
			#pragma vertex vert_img_aa
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			fixed4 frag (v2f_img input) : COLOR  {
				fixed redValue = tex2D(_MainTex, input.uv).r;

				return fixed4(redValue, redValue, redValue, redValue);
			}
		ENDCG
	}

	Pass { // pass 4
		name "dof_with_bokeh"
				
		CGPROGRAM
			#pragma vertex vert_img_aa
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
            sampler2D _COCTex;

			//#define PENTAGONAL_BOKEH

			#include "DOFProBokeh.cginc"

			fixed4 frag (v2f_img input) : COLOR  {
				fixed curCOC = tex2D( _COCTex, input.uv ).r;

				fixed4 res = fixed4(0.0, 0.0, 0.0, 0.0);
				
				res.rgb = DOFWithBokeh(_MainTex, input.uv, curCOC);

				//res.a = curCOC;

				return res;
			}

		ENDCG
	}
}

}