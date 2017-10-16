using System.Collections;
using UnityEngine;
using WeaselTrust;

[AddComponentMenu("Effects/Sleek Render/Ripple Effect")]
[RequireComponent(typeof(Camera))]
public class RippleEffect : MonoBehaviour
{
    public Material material;

    private Camera _camera;
    private Mesh _quadMesh;
    private RenderTexture _renderTexture;

    private Coroutine _waitForEndOfFrameCoroutine;

    private void Start()
    {
        _quadMesh = CreateQuadMesh();
        _renderTexture = CreateRenderTexture();

        material.SetTexture("_MainTex", _renderTexture);

        _camera = GetComponent<Camera>();
        _camera.targetTexture = _renderTexture;
        _camera.enabled = false;
    }

    private Mesh CreateQuadMesh()
    {
        var mesh = new Mesh();

        var vertices = new[] {
            new Vector3(-1f, -1f, 0f),
            new Vector3(-1f, 1f, 0f),
            new Vector3(1f, 1f, 0f),
            new Vector3(1f, -1f, 0f)
        };

        var uvs = new[] {
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

        var triangles = new[] {
            0, 2, 1,
            0, 3, 2
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.colors = colors;

        return mesh;
    }

    private RenderTexture CreateRenderTexture()
    {
        var renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = QualitySettings.antiAliasing;
        return renderTexture;
    }

    public void LateUpdate()
    {
        _camera.Render();
    }

    public void OnRenderObject()
    {
        int cameraInstanceId = Camera.current.GetInstanceID();
        if (cameraInstanceId == _camera.GetInstanceID())
        {
            material.SetTexture("_MainTex", _renderTexture);
            material.SetPass(0);
            Graphics.DrawMeshNow(_quadMesh, Matrix4x4.identity);
        }
        else
        {
            _renderTexture.DiscardContents(false, true);
        }
    }
}
