Shader "Hidden/FxPro" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ChromAberrTex ("Chromatic Aberration (RGB)", 2D) = "black" {}
		_LensDirtTex ("Lens Dirt Texture", 2D) = "black" {}
		_DirtIntensity ("Lens Dirt Intensity", Float) = .1
		_ChromaticAberrationOffset("Chromatic Aberration Offset", Float) = 1

		_BloomTex("Bloom (RGBA)", 2D) = "black" {}
		_DOFTex("DOF (RGB), COC(A)", 2D) = "black" {}
		_COCTex("COC Texture (RGBA)", 2D) = "white" {}
//		_DOFStrength("DOF Strength", Float) = .5

		_SCurveIntensity ("S-Curve Intensity", Float) = .5

		_LensCurvatureBarrelPower("Lens Curvature Barrel Power", Float) = 1.1
		_LensCurvatureZoom("Lens Curvature Zoom", Float) = 1.0
			
		_FilmGrainTex ("Film Grain (RGB)", 2D) = "white" {}
		_FilmGrainIntensity("Film Grain Intensity", Float) = .5
		_FilmGrainTiling("Film Grain Tiling", Float) = 4
		_FilmGrainChannel("Film Grain Channel", Vector) = (1.0, .0, .0, .0)

		_VignettingIntensity("Vignetting Intensity", Float) = .5
		
		//Color effects
		_CloseTint ("Warm Tint Color", Color) = (1, .5, 0, 1)
		_FarTint ("Warm Tint Color", Color) = (0, 0, 1, 1)
	    
	    _CloseTintStrength("Close Tint Strength", Float) = .5
	    _FarTintStrength("Far Tint Strength", Float) = .5
	    
	    _DesaturateDarksStrength("Desaturate Darks Strength", Float) = 0.25
	    _DesaturateFarObjsStrength("Desaturate Far Objs Strength", Float) = .5
	    
	    _FogTint ("Fog Tint Color", Color) = (1, 1, 1, 1)
	    _FogStrength("Fog Strength", Float) = .5
	}
	
	
	CGINCLUDE
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma glsl
		#pragma fragmentoption ARB_precision_hint_fastest

		#pragma multi_compile FXPRO_HDR_ON FXPRO_HDR_OFF

		//+++++++++++++++++++++++++++
		//USER-DEFINED PARAMETERS
		//Those are performance-light, and are defined here to save some keywords.
		#define S_CURVE_ON
		#define VIGNETTING_ON
		#define VIGNETTING_POWER 1		//Larger values result in vignetting being closer to the screen corners
		
		#define FOG_DENSITY 1
		//+++++++++++++++++++++++++++

		sampler2D _MainTex;
		half4 _MainTex_TexelSize;

		#include "FxProInclude.cginc"

		struct v2f_img_aa {
			float4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			half2 uv2 : TEXCOORD1;	//Flipped uv on DirectX platforms to work correctly with AA
		};

		v2f_img_aa vert_img_aa(appdata_img v)
		{
			v2f_img_aa o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord;
			o.uv2 = v.texcoord;

			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv2.y = 1 - o.uv2.y;
			#endif

			return o;
		}
	ENDCG

	SubShader 
	{
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		Blend Off
		
		
		Pass {	//[Pass 0] Bloom/DOF final composite
			name "bloom_dof_composite"
			CGPROGRAM
			#pragma vertex vert_img_aa
			#pragma fragment frag
 	
 			#pragma multi_compile LENS_DIRT_ON LENS_DIRT_OFF
			#pragma multi_compile CHROMATIC_ABERRATION_ON CHROMATIC_ABERRATION_OFF
			#pragma multi_compile DOF_ENABLED DOF_DISABLED
			#pragma multi_compile BLOOM_ENABLED BLOOM_DISABLED
			//#pragma multi_compile S_CURVE_ON S_CURVE_OFF
			#pragma multi_compile FILM_GRAIN_ON FILM_GRAIN_OFF
			//#pragma multi_compile VIGNETTING_ON VIGNETTING_OFF
			#pragma multi_compile COLOR_FX_ON COLOR_FX_OFF


 			#ifdef CHROMATIC_ABERRATION_ON
 			sampler2D _ChromAberrTex;
 			half4 _ChromAberrTex_TexelSize;
			#endif

			#ifdef DOF_ENABLED
			sampler2D _DOFTex;
			sampler2D _COCTex;
			#endif
			
			#ifdef BLOOM_ENABLED
			sampler2D _BloomTex;
			#endif

			#ifdef LENS_DIRT_ON
			sampler2D _LensDirtTex;
			half _DirtIntensity;
			#endif
			
			#ifdef FILM_GRAIN_ON
			sampler2D _FilmGrainTex;
			fixed _FilmGrainIntensity;
			float _FilmGrainTiling;
			fixed4 _FilmGrainChannel;
			#endif

			#ifdef VIGNETTING_ON
			half _VignettingIntensity;
			#endif

			fixed4 frag ( v2f_img_aa i ) : COLOR
			{
				#ifdef COLOR_FX_ON
				fixed3 mainTex = tex2D(_MainTex, i.uv2).rgb;
				#else
				fixed3 mainTex = tex2D(_MainTex, i.uv).rgb;
				#endif

				#if defined(DOF_ENABLED) || defined(DEPTH_FX_ON)
					fixed3 cocTex = tex2D( _COCTex, i.uv2 ).rgb;
				#endif

				#ifdef DOF_ENABLED
					fixed3 dofTex = tex2D(_DOFTex, i.uv2).rgb;
					fixed3 srcTex = dofTex;

					srcTex = lerp(mainTex, srcTex, cocTex.r);
				#else
					fixed3 srcTex = mainTex;
				#endif

				#ifdef BLOOM_ENABLED
					fixed4 bloomTex = saturate( tex2D(_BloomTex, i.uv2) );

					fixed3 resColor = Screen(srcTex.rgb, bloomTex.rgb);
				#else
					fixed4 bloomTex = fixed4(0, 0, 0, 0);
					fixed3 resColor = srcTex.rgb;
				#endif

				//Convert to LDR
				resColor = saturate(resColor);
				
				#ifdef LENS_DIRT_ON
				fixed3 lensDirtTex = tex2D(_LensDirtTex, i.uv2).rgb;
				resColor = Screen(resColor, saturate(lensDirtTex * max(bloomTex.rgb, srcTex.rgb) * _DirtIntensity));
				//resColor = resColor + saturate(lensDirtTex * max(bloomTex.rgb, srcTex.rgb) * _DirtIntensity);
				#endif
				
				#ifdef CHROMATIC_ABERRATION_ON
				fixed3 chromaticAberration = tex2D(_ChromAberrTex, i.uv2).rgb;

				chromaticAberration = saturate(saturate(chromaticAberration) - (srcTex.rgb) );//Make sure not to make the overall image brighter - just add the abberation

				resColor = Screen(resColor, chromaticAberration);
				//resColor = resColor + chromaticAberration;
				#endif

				
				#ifdef FILM_GRAIN_ON
				fixed4 filmGrainTex = tex2D(_FilmGrainTex, i.uv2 * _FilmGrainTiling);
				
				fixed filmGrain = dot(filmGrainTex, _FilmGrainChannel);

				resColor = lerp(resColor, Overlay(resColor, fixed3(filmGrain, filmGrain, filmGrain)), _FilmGrainIntensity);
				#endif

				#ifdef VIGNETTING_ON
				float2 radius = i.uv2 - float2(.5, .5);

				float vignetting = 2 * dot(radius, radius);

				for (int i = 0; i < VIGNETTING_POWER; i++)
					vignetting *= vignetting;

				vignetting *= _VignettingIntensity;

				resColor *= 1 - vignetting;
				#endif

				return fixed4( resColor, 0 );
			} 
			ENDCG
		}
		
		Pass 	//[Pass 1] Downsample
		{ 	
			CGPROGRAM			
			#pragma vertex vert
			#pragma fragment frag
			
			struct v2f {
				float4 pos : SV_POSITION;
				float4 uv[4] : TEXCOORD0;
			};
						
			v2f vert (appdata_img v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				float4 uv;
				uv.xy = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
				uv.zw = 0;

				float offX = _MainTex_TexelSize.x;
				float offY = _MainTex_TexelSize.y;
				
				// Direct3D9 needs some texel offset!
				#ifdef UNITY_HALF_TEXEL_OFFSET
				uv.x += offX * 2.0f;
				uv.y += offY * 2.0f;
				#endif
				o.uv[0] = uv + float4(-offX,-offY,0,1);
				o.uv[1] = uv + float4( offX,-offY,0,1);
				o.uv[2] = uv + float4( offX, offY,0,1);
				o.uv[3] = uv + float4(-offX, offY,0,1);

				return o;
			}
			
			fixed4 frag( v2f i ) : SV_Target
			{
				fixed4 c;
				c  = tex2D( _MainTex, i.uv[0].xy );
				c += tex2D( _MainTex, i.uv[1].xy );
				c += tex2D( _MainTex, i.uv[2].xy );
				c += tex2D( _MainTex, i.uv[3].xy );
				c *= .25f;

				return c;
			}	
			ENDCG		 
		}
		
		Pass {	//[Pass 2]
			name "chromatic_aberration"
			CGPROGRAM
				#pragma vertex vert_img_aa
				#pragma fragment frag

				half _ChromaticAberrationOffset;
	
				inline fixed3 ChromaticAberration(sampler2D _tex, half2 _uv, half2 _texelSize, half _size) {
					fixed3 texOrig = tex2D(_tex, _uv).rgb;

					fixed3 texR = tex2D(_tex, _uv + half2(_texelSize.x, 0) * _size).rgb;
					fixed3 texG = tex2D(_tex, _uv + half2(-_texelSize.x, -_texelSize.y) * _size).rgb;
					fixed3 texB = tex2D(_tex, _uv + half2(-_texelSize.x, _texelSize.y) * _size).rgb;

					return fixed3(texR.r, texG.g, texB.b);
				}

				fixed4 frag (v2f_img_aa i) : COLOR  {
					fixed3 chromaticAberration = ChromaticAberration(_MainTex, i.uv, _MainTex_TexelSize.xy, _ChromaticAberrationOffset);
					return fixed4(chromaticAberration, 1);
				}
			ENDCG
		}

		Pass {	//[Pass 3]
			name "lens_curvature_precise"
			CGPROGRAM
				#pragma vertex vert_img_aa
				#pragma fragment fragLensCurvature

				#define LENS_CURVATURE_PRECISE
				#include "LensCurvature.cginc"
			ENDCG
		}

		Pass {	//[Pass 4]
			name "lens_curvature_optimized"
			CGPROGRAM
				#pragma vertex vert_img_aa
				#pragma fragment fragLensCurvature

				//#define LENS_CURVATURE_PRECISE
				#include "LensCurvature.cginc"
			ENDCG
		}
		
		Pass {	//[Pass 5]
			name "color_effects"
			CGPROGRAM
				#pragma vertex vert_img_aa
				#pragma fragment frag
				
				#pragma multi_compile USE_CAMERA_DEPTH_TEXTURE DONT_USE_CAMERA_DEPTH_TEXTURE
				
				#ifdef USE_CAMERA_DEPTH_TEXTURE
			    sampler2D _CameraDepthTexture;
			    half _OneOverDepthScale;
			    #endif
			    
			    fixed GetDepth(sampler2D mainTex, float2 uv) {
			    #ifdef USE_CAMERA_DEPTH_TEXTURE
				    return Linear01Depth( tex2D(_CameraDepthTexture, uv).r ) * _OneOverDepthScale;
			    #else
				    return tex2D(mainTex, uv).a;
			    #endif
			    }
			    
			    fixed4 _CloseTint;
			    fixed4 _FarTint;
			    
			    fixed _CloseTintStrength, _FarTintStrength;
			    fixed _DesaturateDarksStrength;
			    fixed _DesaturateFarObjsStrength;
			    
			    
			    fixed4 _FogTint;
			    fixed _FogStrength;

				fixed _SCurveIntensity;

				fixed4 frag (v2f_img_aa i) : COLOR  {
					fixed4 mainTex = tex2D(_MainTex, i.uv);
					fixed depth = saturate(GetDepth(_MainTex, i.uv2));
					
					fixed lum = Luminance( mainTex.rgb );
					
					fixed3 resColor = mainTex.rgb;
					
					//Apply distance color grading
					//Closer = warmer, further = colder
					fixed3 closeColor = lerp( resColor, resColor * _CloseTint, _CloseTintStrength );
					fixed3 farColor = lerp( resColor, resColor * _FarTint, _FarTintStrength );
					
					resColor = lerp( closeColor, farColor, depth );
					
					//Desaturate darks
					resColor = lerp( resColor, fixed3(lum, lum, lum), saturate( lum * _DesaturateDarksStrength ) );
										
					//Desaturate far away objects
					resColor = lerp( resColor, fixed3(lum, lum, lum), saturate( depth * _DesaturateFarObjsStrength ) );
					
					//Fog
					fixed fogAmount = 1 - depth;
					
					for (int i = 0; i < FOG_DENSITY; i ++)
						fogAmount *= fogAmount;
					
					fogAmount = 1 - fogAmount;
					
					fixed3 fogColor = lerp( resColor, _FogTint, _FogStrength );
					resColor = lerp( resColor, fogColor, fogAmount );
					
					#ifdef S_CURVE_ON
					resColor = saturate(resColor);
					
					fixed3 adjColor = Overlay(resColor, resColor);

					resColor = lerp(resColor, adjColor, _SCurveIntensity);
					#endif

					return fixed4(resColor, mainTex.a);
				}
			ENDCG
		}
	}
	
	fallback off
}