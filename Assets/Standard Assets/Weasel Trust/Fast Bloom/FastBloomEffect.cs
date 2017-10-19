using UnityEngine;
using UnityEngine.Rendering;

namespace WeaselTrust
{
    [AddComponentMenu("Effects/Weasel Trust/Fast Bloom")]
    [RequireComponent(typeof(Camera))]
    public class FastBloomEffect : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float bloomThreshold = 0.8f;

        [Range(0f, 3f)]
        public float bloomIntensity = 0.8f;

        [Range(0f, 0.5f)]
        public float spread = 0.1f;

        [SerializeField]
        private Shader _downsampleShader;

        [SerializeField]
        private Shader _blurShader;

        [SerializeField]
        private Shader _finalPassShader;

        private Material _downsampleMaterial;
        private Material _horizontalBlurMaterial;
        private Material _finalPassMaterial;

        private RenderTexture _downsampledBrightpassTexture;
        private RenderTexture _finalComposeTexture;
        private RenderTexture _bloomHorizontalBlurTexture;
        private RenderTexture _bloomVerticalBlurTexture;

        private Camera _mainCamera;
        private Camera _renderCamera;
        private RenderTexture _mainRenderTexture;
        private Mesh _fullscreenQuadMesh;
        private int _originalCullingMask;

        private void OnEnable()
        {
            CreateResources();
        }

        private void OnDisable()
        {
            ReleaseResources();
        }

        private void LateUpdate()
        {
            _renderCamera.Render();
            ApplyBloom();
        }

        private void ApplyBloom()
        {
            float oneOverOneMinusBloomThreshold = 1f / (1f - bloomThreshold);
            Vector4 luminanceConst = new Vector4(
                0.2126f * oneOverOneMinusBloomThreshold,
                0.7152f * oneOverOneMinusBloomThreshold,
                0.0722f * oneOverOneMinusBloomThreshold, 
                -bloomThreshold * oneOverOneMinusBloomThreshold);

            _downsampleMaterial.SetVector("_LuminanceConst", luminanceConst);
            Blit(_mainRenderTexture, _downsampledBrightpassTexture, _downsampleMaterial, 0);

            //_horizontalBlurMaterial.SetFloat("_Spread", spread);
            //Blit(_downsampledBrightpassTexture, _bloomHorizontalBlurTexture, _horizontalBlurMaterial, 0);

            //_verticalBlurMaterial.SetFloat("_Spread", spread);
            //Blit(_bloomHorizontalBlurTexture, _bloomVerticalBlurTexture, _verticalBlurMaterial, 0);

            //_finalPassMaterial.SetTexture("_BloomTex", _bloomVerticalBlurTexture);
            //_finalPassMaterial.SetFloat("_BloomIntencity", bloomIntensity);
            //Blit(_mainRenderTexture, _finalComposeTexture, _finalPassMaterial, 0);
        }

        private void OnRenderObject()
        {
            int instanceId = Camera.current.GetInstanceID();
            if (instanceId == this._mainCamera.GetInstanceID())
            {
                _finalPassMaterial.SetTexture("_MainTex", _downsampledBrightpassTexture);
                _finalPassMaterial.SetPass(0);
                Graphics.DrawMeshNow(_fullscreenQuadMesh, Matrix4x4.identity);
            }
            else
            {
                _mainRenderTexture.DiscardContents(false, true);
            }
        }

        private void OnPreCull()
        {
            _originalCullingMask = _mainCamera.cullingMask;
            _mainCamera.cullingMask = 0;
        }

        private void OnPostRender()
        {
            _mainCamera.cullingMask = _originalCullingMask;
        }

        private void CreateResources()
        {
            Debug.Log("Create resources");

            _mainCamera = GetComponent<Camera>();

            LoadShaderIfNotPresent(ref _downsampleShader, "Weasel Trust/Downsample Brightpass");
            LoadShaderIfNotPresent(ref _blurShader, "Weasel Trust/Blur");
            LoadShaderIfNotPresent(ref _finalPassShader, "Weasel Trust/Final Pass");

            _downsampleMaterial = new Material(_downsampleShader);
            _horizontalBlurMaterial = new Material(_blurShader);
            _finalPassMaterial = new Material(_finalPassShader);

            var width = Screen.width;
            var height = Screen.height;

            _downsampledBrightpassTexture = CreateTransientRenderTexture("Bloom Downsample Pass", width / 16, height / 16);
            _bloomHorizontalBlurTexture = CreateTransientRenderTexture("Bloom Horizontal Blur Pass", 32, 128);
            _bloomVerticalBlurTexture = CreateTransientRenderTexture("Bloom Vertical Blur Pass", 32, 128);

            Debug.Log(Screen.width + " " + Screen.height);

            _mainRenderTexture = CreateMainRenderTexture(width, height);
            _finalComposeTexture = CreateMainRenderTexture(width, height);

            var renderCameraGameObject = new GameObject("Bloom Render Camera");
            renderCameraGameObject.hideFlags = HideFlags.HideAndDontSave;
            _renderCamera = renderCameraGameObject.AddComponent<Camera>();
            _renderCamera.CopyFrom(_mainCamera);
            _renderCamera.enabled = false;
            _renderCamera.targetTexture = _mainRenderTexture;

            _fullscreenQuadMesh = CreateScreenSpaceQuadMesh();
        }

        private RenderTexture CreateTransientRenderTexture(string textureName, int width, int height)
        {
            var renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            renderTexture.name = textureName;
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            return renderTexture;
        }

        private RenderTexture CreateMainRenderTexture(int width, int height)
        {
            var isMetal = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal;
            var isTegra = SystemInfo.graphicsDeviceName.Contains("NVIDIA");
            var doesntSupportRgb565 = !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB565);

            var textureFormat = RenderTextureFormat.RGB565;
            if (isMetal || isTegra || doesntSupportRgb565)
            {
                textureFormat = RenderTextureFormat.ARGB32;
            }

            var renderTexture = new RenderTexture(width, height, 16, textureFormat);
            renderTexture.antiAliasing = QualitySettings.antiAliasing;
            return renderTexture;
        }

        private void LoadShaderIfNotPresent(ref Shader shaderVariable, string shaderName)
        {
            if (shaderVariable == null)
            {
                shaderVariable = Shader.Find(shaderName);
            }
        }

        private void ReleaseResources()
        {
            Debug.Log("Release resources");

            DestroyImmediate(_downsampleMaterial);
            DestroyImmediate(_horizontalBlurMaterial);

            DestroyImmediate(_downsampledBrightpassTexture);
            DestroyImmediate(_bloomHorizontalBlurTexture);
            DestroyImmediate(_bloomVerticalBlurTexture);

            DestroyImmediate(_renderCamera.gameObject);

            DestroyImmediate(_mainRenderTexture);
            DestroyImmediate(_finalComposeTexture);

            DestroyImmediate(_fullscreenQuadMesh);
        }

        public void Blit(Texture source, RenderTexture destination, Material material, int materialPass = 0)
        {
            SetActiveRenderTextureAndClear(destination);
            this.DrawFullscreenQuad(source, material, materialPass);
        }

        private static void SetActiveRenderTextureAndClear(RenderTexture destination)
        {
            RenderTexture.active = destination;
            GL.Clear(true, true, new Color(0.5f, 0.5f, 0.5f, 1f));
        }

        private void DrawFullscreenQuad(Texture source, Material material, int materialPass = 0)
        {
            material.SetTexture("_MainTex", source);
            material.SetPass(materialPass);
            Graphics.DrawMeshNow(_fullscreenQuadMesh, Matrix4x4.identity);
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
}