#define PI  3.14159265

//------------------------------------------
//user variables

#define samples 3	//samples on the first ring

#ifdef BLUR_RADIUS_10
#define rings 5		//ring count
#elif BLUR_RADIUS_5
#define rings 3		//ring count
#elif BLUR_RADIUS_3
#define rings 2		//ring count
#endif


//#define maxblur 1.0

#define fringe 0.7	//bokeh chromatic aberration/fringing

half _BokehThreshold;
half _BokehGain;
half _BokehBias;

half _BlurIntensity;

//bool noise = true; //use noise instead of pattern for sample dithering
//float noiseAmount = 0.0001; //dither amount

/*
next part is experimental
not looking good with small sample and ring count
looks okay starting from samples = 4, rings = 4
*/

//#define PENTAGONAL_BOKEH

#ifdef PENTAGONAL_BOKEH
#define pentaFeather 0.4 //pentagon shape feather

float penta(float2 coords) //pentagonal shape
{
	float scale = float(rings) - 1.3;
	float4  HS0 = float4(1.0, 0.0, 0.0, 1.0);
	float4  HS1 = float4(0.309016994, 0.951056516, 0.0, 1.0);
	float4  HS2 = float4(-0.809016994, 0.587785252, 0.0, 1.0);
	float4  HS3 = float4(-0.809016994, -0.587785252, 0.0, 1.0);
	float4  HS4 = float4(0.309016994, -0.951056516, 0.0, 1.0);
	float4  HS5 = float4(0.0, 0.0, 1.0, 1.0);

	float4  one = float4(1.0);

	float4 P = float4( (coords), float2(scale, scale) );

	float4 dist = float4(0.0);
	float inorout = -4.0;

	dist.x = dot(P, HS0);
	dist.y = dot(P, HS1);
	dist.z = dot(P, HS2);
	dist.w = dot(P, HS3);

	dist = smoothstep(-pentaFeather, pentaFeather, dist);

	inorout += dot(dist, one);

	dist.x = dot(P, HS4);
	dist.y = HS5.w - abs(P.z);

	dist = smoothstep(-pentaFeather, pentaFeather, dist);
	inorout += dist.x;

	return clamp(inorout, 0.0, 1.0);
}
#endif

fixed3 color(sampler2D tex, float2 coords, fixed coc) //processing the sample
{
	//fixed3 col = tex2Dlod(tex, float4(coords + float2(0.0, 1.0) * _MainTex_TexelSize.xy * fringe * coc, 0.0, 0.0) ).rgb;
	fixed3 col = tex2Dlod(tex, float4(coords.xy, 0.0, 0.0) ).rgb;

	fixed lum = Luminance(col.rgb);
	float thresh = max((lum - _BokehThreshold) * _BokehGain, 0.0);

	return col + lerp( fixed3(0.0, 0.0, 0.0), col, thresh * coc );
}

fixed3 DOFWithBokeh(sampler2D mainTex, float2 uv, fixed coc){

	// calculation of pattern for ditering

	//float2 noise = float20.0;//rand(gl_TexCoord[0].xy) * noiseAmount * coc;

	// getting blur x and y step factor

	float w = _MainTex_TexelSize.x * coc * _BlurIntensity;// + noise.x;
	float h = _MainTex_TexelSize.y * coc * _BlurIntensity;// + noise.y;

	// calculation of final color

	fixed3 col = tex2Dlod(mainTex, float4(uv.xy, 0.0, 0.0) ).rgb;
	
	float s = 1.0;
	int ringsamples;

	for (int i = 1; i <= rings; i++)
	{
		ringsamples = i * samples;

		for (int j = 0; j < ringsamples; j++)
		{
			float step = PI * 2.0 / float(ringsamples);
			float pw = ( cos( float(j) * step ) * float(i) );
			float ph = ( sin( float(j) * step ) * float(i) );
			float p = 1.0;

			#ifdef PENTAGONAL_BOKEH
			p = penta( float2(pw, ph) );
			#endif

			col += color(mainTex, uv.xy + float2(pw*w, ph*h), coc) * lerp(1.0, (float(i)) / (float(rings)), _BokehBias) * p;
			s += 1.0 * lerp(1.0, float(i) / float(rings), _BokehBias) * p;
		}
	}

	col /= s; //divide by sample count

	return col;
}