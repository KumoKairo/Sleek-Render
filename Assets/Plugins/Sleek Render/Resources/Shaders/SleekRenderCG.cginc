struct appdata
{
	half4 vertex : POSITION;
	half4 uv : TEXCOORD0;
};

// Texture fetch + brightpass through a predefined pixel luminance threshold
half4 getTapAndLumaFrom(sampler2D_half tex, half2 uv, half4 luminanceThreshold)
{
	half4 tap = tex2D(tex, uv.xy);
	// Calculating the pixel brightness
	half luma = saturate(dot(half4(tap.rgb, 1.0h), luminanceThreshold)); 
	// Makes dark pixels black, leaving only bright-enough pixels on the scene
	return tap *= luma;
}

// Using Markus-downsample formula for pixel value calculation
// Five-tap boxy blur
// Also automatically applies brightpass logic to all taps
half4 applyDownsampleBrightpassTapLogic(half2 uv[5], sampler2D_half tex, half4 luminanceThreshold)
{
	half4 result = getTapAndLumaFrom(tex, uv[0], luminanceThreshold) * 4.0;
	result += getTapAndLumaFrom(tex, uv[1], luminanceThreshold);
	result += getTapAndLumaFrom(tex, uv[2], luminanceThreshold);
	result += getTapAndLumaFrom(tex, uv[3], luminanceThreshold);
	result += getTapAndLumaFrom(tex, uv[4], luminanceThreshold);
	
	return result / 8.0h;
}

// Using Markus-downsample formula for pixel value calculation
// Five-tap boxy blur
// Version without brightpass
half4 applyDownsampleTapLogic(half2 uv[5], sampler2D_half tex)
{
	half4 result = tex2D(tex, uv[0]) * 4.0;
	result += tex2D(tex, uv[1]);
	result += tex2D(tex, uv[2]);
	result += tex2D(tex, uv[3]);
	result += tex2D(tex, uv[4]);
	
	return result / 8.0h;
}

// Using Markus-downsample formula for UV offset calculations in vertex shader
void calculateDownsampleTapPoints(appdata v, half2 halfpixel, out half2 uv[5])
{
	uv[0] = v.uv;
	uv[1] = v.uv + half2(-halfpixel.x, -halfpixel.y);
	uv[2] = v.uv + half2(halfpixel.x, halfpixel.y);
	uv[3] = v.uv + half2(-halfpixel.x, halfpixel.y);
	uv[4] = v.uv + half2(halfpixel.x, -halfpixel.y);
}