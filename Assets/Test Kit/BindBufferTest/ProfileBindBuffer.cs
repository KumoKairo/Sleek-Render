using System.Diagnostics;
using SleekRender;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ProfileBindBuffer : MonoBehaviour
{
    public Material identityMaterial;

    public Material horizontalBlurMaterial;
    public Material verticalBlurMaterial;

    private RenderTexture _rt1;
    private RenderTexture _rt2;

    private string[] profilerKeys;
    private Mesh _fullscreenQuadMesh;
    private Matrix4x4 _identityMatrix;

    private const int passes = 20;

    private Stopwatch _sw;

    private void Awake()
    {
        _rt1 = new RenderTexture(256, 256, 0);
        _rt2 = new RenderTexture(256, 256, 0);
        _fullscreenQuadMesh = CreateScreenSpaceQuadMesh();

        profilerKeys = new string[passes];

        for (int i = 0; i < passes; i++)
        {
            profilerKeys[i] = (i + 2) + " passes ";
        }

        _identityMatrix = Matrix4x4.identity;

        _sw = new Stopwatch();
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, _rt1, identityMaterial);

        for (int i = 0; i < passes; i++)
        {
            var profilerKey = profilerKeys[i];

            _sw.Reset();
            _sw.Start();
            for (int j = 0; j <= i; j++)
            {
                Graphics.Blit(_rt1, _rt2, identityMaterial);
                Graphics.Blit(_rt2, _rt1, identityMaterial);
            }

            _sw.Stop();
            Debug.Log(profilerKey + _sw.ElapsedMilliseconds);

        }

        Graphics.Blit(_rt1, dst, identityMaterial);
    }

    public void Blit(Texture source, RenderTexture destination, Material material, int materialPass = 0)
    {
        SetActiveRenderTextureAndClear(destination);
        this.DrawFullscreenQuad(source, material, materialPass);
    }

    private static void SetActiveRenderTextureAndClear(RenderTexture destination)
    {
        RenderTexture.active = destination;
        GL.Clear(true, true, new Color(1f, 0.75f, 0.5f, 0.8f));
    }

    private void DrawFullscreenQuad(Texture source, Material material, int materialPass = 0)
    {
        material.SetTexture(SleekRenderPostProcess.Uniforms._MainTex, source);
        material.SetPass(materialPass);
        Graphics.DrawMeshNow(_fullscreenQuadMesh, _identityMatrix);
    }

    private Mesh CreateScreenSpaceQuadMesh()
    {
        var mesh = new Mesh();

        var vertices = new[]
        {
            new Vector3(-1f, -1f, 0f), // BL
            new Vector3(-1f, 1f, 0f),  // TL
            new Vector3(1f, 1f, 0f),   // TR
            new Vector3(1f, -1f, 0f)   // BR
        };

        var uvs = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f)
        };

        var colors = new[]
        {
            new Color(0f, 0f, 1f),
            new Color(0f, 1f, 1f),
            new Color(1f, 1f, 1f),
            new Color(1f, 0f, 1f),
        };

        var triangles = new[]
        {
            0, 2, 1,
            0, 3, 2
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.UploadMeshData(true);

        return mesh;
    }
}
