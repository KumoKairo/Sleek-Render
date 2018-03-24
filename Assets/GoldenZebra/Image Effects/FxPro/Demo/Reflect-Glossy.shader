Shader "Hidden/BloomProDemo/Reflective/Specular" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
	_MainTex ("Base (RGB) Emission (A)", 2D) = "white" {}
	_BumpMap ("Normal Map", 2D) = "bump" {}
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
	_EmissionColor ("Emission Color", Color) = (1,1,1,1)
	_NormalStrength ("Normal Strength", Range (0.01, 1)) = 1
}
SubShader {
	LOD 300
	Tags { "RenderType"="Opaque" }


CGPROGRAM
#pragma surface surf BlinnPhong
#pragma target 3.0

sampler2D _MainTex;
samplerCUBE _Cube;

sampler2D _BumpMap;

fixed4 _Color;
fixed4 _ReflectColor;
half _Shininess;

half _NormalStrength;

fixed4 _EmissionColor;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float3 worldRefl;
	INTERNAL_DATA
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 c = tex * _Color;
	o.Albedo = c.rgb;
	o.Gloss = _Color.a;
	o.Specular = _Shininess;
	
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	o.Normal.z /= _NormalStrength;
	o.Normal = normalize( o.Normal );
	
	float3 worldRefl = WorldReflectionVector (IN, o.Normal);
	fixed4 reflcol = texCUBE (_Cube, worldRefl);
	
	reflcol = reflcol * reflcol;
	reflcol *= tex.a;
	o.Emission = reflcol.rgb * _ReflectColor.rgb + _EmissionColor * tex.rgb * tex.a * 3;
	o.Alpha = reflcol.a * _ReflectColor.a;
	
//	o.Emission = tex.a;
//	o.Albedo = 0;
}
ENDCG
}

FallBack "Reflective/VertexLit"
//Fall
}
