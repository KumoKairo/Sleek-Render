using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawMeshNow : MonoBehaviour {

	// When added to an object, draws colored rays from the
	// transform position.
	public int lineCount = 100;
	public float radius = 3.0f;

	public Mesh drawMeshNow;

	private Mesh quadMesh;

	private Material lineMaterial;
	private void CreateLineMaterial()
	{
		if (!lineMaterial)
		{
			// Unity has a built-in shader that is useful for drawing
			// simple colored things.
			Shader shader = Shader.Find("Unlit/PostProcessUnlit");
			lineMaterial = new Material(shader);
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			// Turn on alpha blending
			lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			// Turn backface culling off
			lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			// Turn off depth writes
			lineMaterial.SetInt("_ZWrite", 0);
		}
	}

	void CreateQuadMesh ()
	{
		if (quadMesh != null) {
			return;
		}

		var mesh = new Mesh ();

		var vertices = new [] {
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, 0f, 0f)
		};

		var uvs = new [] {
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};

		var triangles = new [] {
			0, 2, 1,
			0, 3, 2
		};

		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		quadMesh = mesh;
	}

	// Will be called after all regular rendering is done
	public void OnRenderObject()
	{
		CreateLineMaterial();
		CreateQuadMesh ();
		// Apply the line material
		lineMaterial.SetPass(0);

		lineMaterial.SetColor ("_Color", Color.white);

		GL.PushMatrix();

		var matrix = Matrix4x4.identity;
		Graphics.DrawMeshNow (quadMesh, matrix);
		/*GL.LoadOrtho();
		GL.Color(Color.red);
		GL.Begin(GL.QUADS);
		GL.Vertex3(0F, 0F, 0F);
		GL.Vertex3(0F, 1F, 0F);
		GL.Vertex3(1F, 1F, 0F);
		GL.Vertex3(1F, 0F, 0F);
		GL.End();*/
		GL.PopMatrix();

		/*
		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		//GL.MultMatrix(transform.localToWorldMatrix);

		Graphics.DrawMeshNow (drawMeshNow, transform.localToWorldMatrix);
		// Draw lines
		/*GL.Begin(GL.LINES);
		for (int i = 0; i < lineCount; ++i)
		{
			float a = i / (float)lineCount;
			float angle = a * Mathf.PI * 2;
			// Vertex colors change from red to green
			GL.Color(new Color(a, 1 - a, 0, 0.8F));
			// One vertex at transform position
			GL.Vertex3(0, 0, 0);
			// Another vertex at edge of circle
			GL.Vertex3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
		}
		GL.End();
		GL.PopMatrix();*/
	}
}
