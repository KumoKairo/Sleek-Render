#ifdef FXPRO_HDR_ON
#define fixed  half
#define fixed2 half2
#define fixed3 half3
#define fixed4 half4
#endif

inline fixed Screen1(fixed _a, fixed _b) {
	fixed res = 1 - (1 - _a) * (1 - _b);

	return res;
}

//Make sure that the input is in LDR (0..1)
inline fixed3 Screen(fixed3 _a, fixed3 _b) {
	return fixed3(Screen1(_a.r, _b.r), Screen1(_a.g, _b.g), Screen1(_a.b, _b.b));
}

//Make sure that the input is in LDR (0..1)
inline fixed4 Screen(fixed4 _a, fixed4 _b) {
	return fixed4(Screen1(_a.r, _b.r), Screen1(_a.g, _b.g), Screen1(_a.b, _b.b), Screen1(_a.a, _b.a));
}

//Make sure that the input is in LDR (0..1)
inline fixed Overlay1(fixed _a, fixed _b) {
	//(Target > 0.5) * (1 - (1-2*(Target-0.5)) * (1-Blend)) +
	//(Target <= 0.5) * ((2*Target) * Blend)

	//Convert to LDR first
	/*#ifndef FXPRO_HDR_OFF
	_a = saturate(_a);
	_b = saturate(_b);
	#endif*/

	fixed screen = (1 - 2 * (1 - _a) * (1 - _b));
	fixed mult = 2 * _a * _b;

	return lerp(mult, screen, saturate((_a - .5) * 10000));
}

inline fixed3 Overlay(fixed3 _a, fixed3 _b) {
	return fixed3(Overlay1(_a.r, _b.r), Overlay1(_a.g, _b.g), Overlay1(_a.b, _b.b));
}