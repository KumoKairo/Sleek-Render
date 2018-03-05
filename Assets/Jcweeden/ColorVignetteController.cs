using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class ColorVignetteController : MonoBehaviour 
{
	public float vignetteBeginRadius = 0.166f;
	public float vignetteExpandRadius = 1.34f;
	public Color vignetteColor = Color.black;

	public Material targetMaterial;

	private static class Uniforms
	{
		public static readonly int _VignetteShape = Shader.PropertyToID("_VignetteShape");
		public static readonly int _VignetteColor = Shader.PropertyToID("_VignetteColor");
	}

	public void Start()
	{
		targetMaterial = GetComponent<Renderer>().sharedMaterial;
	}

	public void Update()
	{
		float squareVignetteBeginRaduis = vignetteBeginRadius * vignetteBeginRadius;
		float vignetteRadii = vignetteBeginRadius + vignetteExpandRadius;
		float oneOverVignetteRadiusDistance = 1f / (vignetteRadii - squareVignetteBeginRaduis);

		targetMaterial.SetVector(Uniforms._VignetteShape, new Vector4(
			4f * oneOverVignetteRadiusDistance * oneOverVignetteRadiusDistance,
			-oneOverVignetteRadiusDistance * squareVignetteBeginRaduis));

		// Premultiplying Alpha of vignette color
		targetMaterial.SetColor(Uniforms._VignetteColor, new Color(
				vignetteColor.r * vignetteColor.a,
				vignetteColor.g * vignetteColor.a,
				vignetteColor.b * vignetteColor.a,
				vignetteColor.a));
	}
}
