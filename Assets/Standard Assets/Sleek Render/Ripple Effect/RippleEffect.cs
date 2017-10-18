using UnityEngine;

[AddComponentMenu("Effects/Sleek Render/Ripple Effect")]
[RequireComponent(typeof(Camera))]
public class RippleEffect : MonoBehaviour
{
    public Material material;

    private Camera _camera;
    private Camera _targetCamera;
    private Mesh _quadMesh;
    private RenderTexture _renderTexture;

    private int _cullingMask;

    private Coroutine _waitForEndOfFrameCoroutine;

    private void Start()
    {
        _quadMesh = CreateQuadMesh();
        _renderTexture = CreateRenderTexture();

        _camera = GetComponent<Camera>();
        
        var copyCamera = new GameObject("Camera");
        _targetCamera = copyCamera.AddComponent<Camera>();
        _targetCamera.CopyFrom(_camera);
        _targetCamera.targetTexture = _renderTexture;
        _targetCamera.enabled = false;
    }

    private void LateUpdate()
    {
        _targetCamera.Render();
    }

    private void OnRenderObject()
    {
        
    }

    private void OnPreCull()
    {
        _cullingMask = _camera.cullingMask;
        _camera.cullingMask = 0;
    }

    private void OnPostRender()
    {
        _camera.cullingMask = _cullingMask;

        int instanceId = Camera.current.GetInstanceID();
        if (instanceId == this._camera.GetInstanceID())
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
            0, 1, 2,
            0, 2, 3
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.UploadMeshData(true);

        return mesh;
    }

    private RenderTexture CreateRenderTexture()
    {
        var renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = QualitySettings.antiAliasing;
        renderTexture.depth = 16;
        return renderTexture;
    }
}
