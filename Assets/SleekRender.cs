using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SleekRender : MonoBehaviour 
{
	public Camera camera;
	public Material material;

	private RenderTexture _texture;
	private Mesh _onScreenMesh;

	void Start () 
	{
		_texture = PrepareRenderTexture ();
		_onScreenMesh = PrepareOnScreenMesh ();
		PrepareMaterial (ref material);
		camera.targetTexture = _texture;
		DisplaySupportedColorFormats ();
	}
	
	void OnPostRender () 
	{
		GL.PushMatrix();
		material.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(GL.QUADS);
		GL.Color(Color.red);
		GL.Vertex3(0, 0.5F, 0);
		GL.Vertex3(0.5F, 1, 0);
		GL.Vertex3(1, 0.5F, 0);
		GL.Vertex3(0.5F, 0, 0);
		GL.Color(Color.cyan);
		GL.Vertex3(0, 0, 0);
		GL.Vertex3(0, 0.25F, 0);
		GL.Vertex3(0.25F, 0.25F, 0);
		GL.Vertex3(0.25F, 0, 0);
		GL.End();
		GL.PopMatrix();

		return;

		GL.PushMatrix();
		GL.LoadOrtho();

		// activate the first shader pass (in this case we know it is the only pass)
		material.SetPass(0);
		// draw a quad over whole screen
		GL.Begin(GL.QUADS);
		GL.Vertex3(0f, 0f, 0f);
		GL.Vertex3(1f, 0f, 0f);
		GL.Vertex3(1f, 1f, 0f);
		GL.Vertex3(0f, 1f, 0f);
		GL.End();

		GL.PopMatrix();
	}

	void PrepareMaterial (ref Material material)
	{
		// Unity has a built-in shader that is useful for drawing
		// simple colored things. In this case, we just want to use
		// a blend mode that inverts destination colors.
		var shader = Shader.Find("Hidden/Internal-Colored");
		material = new Material(shader);
		material.hideFlags = HideFlags.HideAndDontSave;
		// Set blend mode to invert destination colors.
		material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
		material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
		// Turn off backface culling, depth writes, depth test.
		material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
		material.SetInt("_ZWrite", 0);
		material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
	}

	private RenderTexture PrepareRenderTexture ()
	{
		var supportsRBG565 = SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.RGB565);
		var renderTextureFormat = RenderTextureFormat.ARGB32;
		if (supportsRBG565) {
			renderTextureFormat = RenderTextureFormat.RGB565;
		}
		var texture = new RenderTexture (Screen.width, Screen.height, 0);
		texture.format = renderTextureFormat;

		return texture;
	}

	private Mesh PrepareOnScreenMesh ()
	{
		var mesh = new Mesh ();
		var vertices = new Vector3[4];
		var uvs = new Vector2[4];
		var triangles = new int[6];

		vertices [0] = new Vector3 (-1f, -1f, camera.nearClipPlane);
		vertices [1] = new Vector3 (-1f, 1f, camera.nearClipPlane);
		vertices [2] = new Vector3 (1f, 1f, camera.nearClipPlane);
		vertices [3] = new Vector3 (1f, -1f, camera.nearClipPlane);


		uvs [0] = new Vector2 (0f, 0f);
		uvs [1] = new Vector2 (0f, 1f);
		uvs [2] = new Vector2 (1f, 1f);
		uvs [3] = new Vector2 (1f, 0f);

		triangles [0] = 0;
		triangles [1] = 2;
		triangles [2] = 1;

		triangles [3] = 0;
		triangles [4] = 1;
		triangles [5] = 3;

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;

		return mesh;
	}

	private void DisplaySupportedColorFormats(){
		foreach (RenderTextureFormat textureFormat in Enum.GetValues(typeof(RenderTextureFormat))) {
			var isSupported = SystemInfo.SupportsRenderTextureFormat (textureFormat);
			var isNotSupportedString = isSupported ? " " : " NOT ";
			if (!isSupported) {
				Debug.Log (textureFormat.ToString () + " is" + isNotSupportedString + "supported");
			}
		}

		Debug.Log ("----------------------");

		foreach (RenderTextureFormat textureFormat in Enum.GetValues(typeof(RenderTextureFormat))) {
			var isSupported = SystemInfo.SupportsRenderTextureFormat (textureFormat);
			var isNotSupportedString = isSupported ? " " : " NOT ";
			if (isSupported) {
				Debug.Log (textureFormat.ToString () + " is" + isNotSupportedString + "supported");
			}
		}
	}
}
