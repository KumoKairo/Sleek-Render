Shader "Sleek Render/Post Process/Compose"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PreComposeTex("PreCompose Texture", 2D) = "black" {}
		_FilmGrainTex("Film Grain Texture", 2D) = "white" {}
		_Colorize("Colorize", color) = (1.0, 1.0, 1.0, 0.0)
		_ContrastBrightness("Contrast And Brightness", vector) = (1.0, 0.5, 0, 0)
		_LuminanceConst("Luminance Const", vector) = (0.2126, 0.7152, 0.0722, 0.0)
		_FilmGrainIntensity("Film Grain Intensity", float) = 0.5
		_FilmGrainChannel("Film Grain Channel", vector) = (1.0, 0.0, 0.0, 0.0)
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ COLORIZE_ON
			#pragma multi_compile _ BRIGHTNESS_CONTRAST_ON
			#pragma multi_compile _ FILM_GRAIN_ON

			#pragma multi_compile _ FILM_GRAIN_OVERLAY
			#pragma multi_compile _ FILM_GRAIN_MULTIPLY
			#pragma multi_compile _ FILM_GRAIN_ADDITION
			
			struct appdata
			{
				half4 vertex : POSITION;
				half2 uv : TEXCOORD0;
				half2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv2 : TEXCOORD1;
			};

			half4 _Colorize, _LuminanceConst, _MainTex_TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				o.uv = v.uv;
				o.uv2 = v.uv2;

				if (_ProjectionParams.x < 0)
				{
					o.uv.y = 1 - v.uv.y;
					o.uv2.y = 1 - v.uv2.y;
				}

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
				{
					o.uv.y = 1 - v.uv.y;
					o.uv2.y = 1 - v.uv2.y;
				}
				#endif

				return o;
			}
			
			sampler2D_half _MainTex, _PreComposeTex, _FilmGrainTex;
			half _FilmGrainIntensity;
			half3 _BrightnessContrast;
			half4 _FilmGrainChannel;

			half3 Overlay(half3 a, half3 b) 
			{
				half3 screen = 1.0 - 2.0 * (1 - a) * (1 - b);
				half3 mult = 2 * a * b;
				return lerp(mult, screen, saturate((a - .5) * 10000));
			}

			half3 MultiplyOverlay(half3 a, half3 b)
			{
				return a * b;
			}

			half3 AdditionOverlay(half3 a, half3 b)
			{
				return a + b;
			}

			half4 frag (v2f i) : SV_Target
			{
				half4 precompose = tex2D(_PreComposeTex, i.uv);
				half4 col = tex2D(_MainTex, i.uv);
				half3 mainColor = col.rgb * precompose.a + precompose.rgb;

				#ifdef COLORIZE_ON
				half3 result = mainColor * _Colorize.a + _Colorize.rgb * dot(_LuminanceConst, mainColor);
				#else 
				half3 result = mainColor;
				#endif

				#ifdef BRIGHTNESS_CONTRAST_ON
				result = saturate((result * _BrightnessContrast.x) + _BrightnessContrast.z);
				#endif

				#ifdef FILM_GRAIN_ON
				half4 filmGrainTex = tex2D(_FilmGrainTex, i.uv2);
				half filmGrain = dot(filmGrainTex, _FilmGrainChannel);

				#ifdef FILM_GRAIN_OVERLAY
				half3 overlay = Overlay(result, half3(filmGrain, filmGrain, filmGrain));

				#elif FILM_GRAIN_MULTIPLY
				half3 overlay = MultiplyOverlay(result, half3(filmGrain, filmGrain, filmGrain));

				#elif FILM_GRAIN_ADDITION
				half3 overlay = AdditionOverlay(result, half3(filmGrain, filmGrain, filmGrain));
				#else
				half3 overlay = result;
				#endif
				result = lerp(result, overlay, _FilmGrainIntensity);
				#endif

				return half4(result, 1.0h);
			}
			ENDCG
		}
	}
}
