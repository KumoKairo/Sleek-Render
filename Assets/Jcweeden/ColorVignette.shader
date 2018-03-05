Shader "Screen Space/ColorVignette"
{
	Properties
	{
		_Color ("Color", Color) = (0.204, 0.596, 0.859, 1)
		_VignetteShape("Vignette Form", vector) = (1.0, 1.0, 1.0, 1.0)
		_VignetteColor("Vignette Color", color) = (0.0, 0.0, 0.0, 1.0)
	}
	SubShader
	{
		Tags { "Queue" = "Geometry+1" }
		Cull off
		ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				half4 vertex : POSITION;
				half2 uv : TEXCOORD0;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
			};

			half4 _Color, _VignetteShape, _VignetteColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				o.uv = v.uv;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half2 vignetteCenter =  i.uv - half2(0.5h, 0.5h);
				half vignetteShape = saturate(dot(vignetteCenter, vignetteCenter) * _VignetteShape.x + _VignetteShape.y);
				half4 vignette = half4(_VignetteColor.rgb * vignetteShape, 1.0h - _VignetteColor.a * vignetteShape);
				half vignetteAlpha = vignette.a;
				half3 vignetteRGB = vignette.rgb;
				half3 alphaMultiplier = half3(vignette.a, vignette.a, vignette.a);

				half4 result = half4(_Color * vignetteAlpha + alphaMultiplier * vignetteRGB, vignetteAlpha);
				return result;
			}
			ENDCG
		}
	}
}
