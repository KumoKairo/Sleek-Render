half _LensCurvatureBarrelPower;

half _LensCurvatureZoom;

float2 ApplyLensCurvature(float2 uv)
{
	float screenAspect = _ScreenParams.x / _ScreenParams.y;

	uv = uv * 2.0 - 1.0;

	uv.x *= screenAspect;
	
	float theta = atan2(uv.y, uv.x);

	float radius = length(uv);
	#ifdef LENS_CURVATURE_PRECISE
	radius = pow(radius, _LensCurvatureBarrelPower);
	#else
	radius = lerp(radius, radius * radius, saturate(_LensCurvatureBarrelPower - 1.0));
	#endif

	uv.x = radius * cos(theta);
	uv.y = radius * sin(theta);

	uv *= _LensCurvatureZoom;

	uv.x /= screenAspect;

	return 0.5 * (uv + 1.0);
}

fixed4 fragLensCurvature(v2f_img_aa i) : COLOR{
	fixed4 mainTex = tex2D( _MainTex, ApplyLensCurvature(i.uv) );

	return mainTex;
}