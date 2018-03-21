#ifndef SEPARABLE_BLUR_CGINC_INCLUDED
#define SEPARABLE_BLUR_CGINC_INCLUDED

#include "UnityCG.cginc"

//#define BOX_KERNEL

//#ifndef BOX_KERNEL
//	#define GAUSSIAN_KERNEL
//#endif

#ifdef RGBM_DECODE
	#undef BLUR_ALPHA_CHANNEL
#endif

//No radius defined?
#if !defined(BLUR_RADIUS_10) && !defined(BLUR_RADIUS_5) && !defined(BLUR_RADIUS_3) && !defined(BLUR_RADIUS_2) && !defined(BLUR_RADIUS_1)
	#define BLUR_RADIUS_5
#endif

half4 _SeparableBlurOffsets;

//struct v2f {
//	half4 pos : SV_POSITION;
//	
//	half2 uv : TEXCOORD0;
//};	

//v2f vert (appdata_img v)
//{
//	v2f o;
//	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
//	o.uv = v.texcoord;
//	
//	return o;
//}

//inline fixed4 ApplyBokeh(fixed4 _input) {
//	fixed lum = Luminance( _input.rgb );
//	return _input * (1 + saturate( lum - _BokehThreshold ) * _BokehGain);
//}

inline fixed4 SampleTex(sampler2D _tex, half2 _uv) {
	fixed4 tex = tex2Dlod(_tex, half4(_uv.x, _uv.y, 0, 0) );
	
	#ifdef RGBM_DECODE
	return fixed4(tex.rgb * tex.a * 8, tex.a);
	#else
	return tex;
	#endif
}

inline fixed4 BlurTex(sampler2D _tex, v2f_img input, half _stepSizeScale) {
	#ifdef GAUSSIAN_KERNEL
		#ifdef BLUR_RADIUS_10
			#ifndef SQRT_KERNEL
			half blurKernel[21] = {0.0000009536743, 0.00001907349, 0.0001811981, 0.001087189, 0.004620552, 0.01478577, 0.03696442, 0.07392883, 0.1201344, 0.1601791, 0.1761971,
										0.1601791, 0.1201344, 0.07392883, 0.03696442, 0.01478577, 0.004620552, 0.001087189, 0.0001811981, 0.00001907349, 0.0000009536743};
			#else
			half blurKernel[21] = {0.00029375321, 0.00131370447, 0.00404910851, 0.00991825158, 0.02044699669, 0.03657670408, 0.0578328432, 0.08178798567, 0.10425965597, 0.1203886433, 0.12626470656,
										0.1203886433, 0.10425965597, 0.08178798567, 0.0578328432, 0.03657670408, 0.02044699669, 0.00991825158, 0.00404910851, 0.00131370447, 0.00029375321};
			#endif
		#endif
		
		#ifdef BLUR_RADIUS_5
		half blurKernel[11] = {0.0009765625, 0.009765625, 0.04394531, 0.1171875, 0.2050781, 0.2460938, 0.2050781, 0.1171875, 0.04394531, 0.009765625, 0.0009765625};
		#endif
	
		#ifdef BLUR_RADIUS_3
		half blurKernel[7] = {0.015625, 0.09375, 0.234375, 0.3125, 0.234375, 0.09375, 0.015625};
		#endif
		
		#ifdef BLUR_RADIUS_2
		half blurKernel[5] = {0.0625, 0.25, 0.375, 0.25, 0.0625};
		#endif
		
		#ifdef BLUR_RADIUS_1
		half blurKernel[3] = {0.25, 0.5, 0.25};
		#endif
	#endif

	#ifdef BLUR_RADIUS_10
	const int blurRadius = 10;
	#endif
	
	#ifdef BLUR_RADIUS_5
	const int blurRadius = 5;
	#endif
	
	#ifdef BLUR_RADIUS_3
	const int blurRadius = 3;
	#endif
	
	#ifdef BLUR_RADIUS_2
	const int blurRadius = 2;
	#endif
	
	#ifdef BLUR_RADIUS_1
	const int blurRadius = 1;
	#endif

	half2 finalStepSize = _SeparableBlurOffsets.xy * _stepSizeScale;
	
	half4 res = half4(0, 0, 0, 0);
	
	#ifdef BOX_KERNEL
	half boxWeight = 1.0 / half(blurRadius * 2 + 1);
	#endif
	
	for (int i = 0; i <= blurRadius * 2; i++) {
		half2 curUV = input.uv + _MainTex_TexelSize.xy * finalStepSize * half(i - blurRadius);
		
		#ifdef BLUR_ALPHA_CHANNEL
			#ifdef GAUSSIAN_KERNEL
			res += SampleTex(_tex, curUV) * blurKernel[i];
			#elif defined(BOX_KERNEL)
			res += SampleTex(_tex, curUV) * boxWeight;
			#endif
		#else
			#ifdef GAUSSIAN_KERNEL
			res.rgb += SampleTex(_tex, curUV).rgb * blurKernel[i];
			#elif defined(BOX_KERNEL)
			res.rgb += SampleTex(_tex, curUV).rgb * boxWeight;
			#endif
		#endif
	}
	
#ifndef IGNORE_ALPHA_CHANNEL
	#ifndef BLUR_ALPHA_CHANNEL
	fixed4 centralPixel = SampleTex(_tex, input.uv);
	
		//!!!!!Discards all black pixels!!!!!
		#ifdef COLORIZE_WITH_CENTRAL_PIXEL
		res.rgb *= centralPixel.rgb;
		#endif
		
	res.a = centralPixel.a;
	#endif
#endif
	
	return res;//fixed4(res.rgb, centralPixel.a);
}

#endif