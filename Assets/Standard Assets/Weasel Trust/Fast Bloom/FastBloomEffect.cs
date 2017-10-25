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

        [SerializeField]
        private Shader _downsampleShader;

        [SerializeField]
        private Shader _brightPassShader;

        [SerializeField]
        private Shader _blurShader;

        [SerializeField]
        private Shader _verticalBlurGammaCorrectionShader;

        [SerializeField]
        private Shader _composeShader;

        [SerializeField]
        private Shader _displayMainTextureShader;

        [SerializeField]
        private Shader _upscaleBloomShader;

        private Material _downsampleMaterial;
        private Material _brightpassMaterial;
        private Material _blurMaterial;
        private Material _verticalBlurGammaCorrectionMaterial;
        private Material _composeMaterial;
        private Material _displayMainTextureMaterial;
        private Material _upscaleBloomMaterial;

        private RenderTexture _downsampledBrightpassTexture;
        private RenderTexture _preBloomTexture;
        private RenderTexture _horizontalBlurTexture;
        private RenderTexture _verticalBlurGammaCorrectedTexture;

        private RenderTexture _finalComposeTexture;

        private Camera _mainCamera;
        private Camera _renderCamera;
        private RenderTexture _mainRenderTexture;
        private Mesh _fullscreenQuadMesh;
        private int _originalCullingMask;

        private void OnEnable()
        {
            Application.targetFrameRate = 60;
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
            Blit(_mainRenderTexture, _downsampledBrightpassTexture, _downsampleMaterial);

            Blit(_downsampledBrightpassTexture, _preBloomTexture, _brightpassMaterial);

            _blurMaterial.SetVector("_SpreadDirection", new Vector4(1f, 0f, 0f, 0f));
            Blit(_preBloomTexture, _horizontalBlurTexture, _blurMaterial);

            _verticalBlurGammaCorrectionMaterial.SetFloat("_BloomIntencity", bloomIntensity);
            _verticalBlurGammaCorrectionMaterial.SetPass(0);
            Blit(_horizontalBlurTexture, _verticalBlurGammaCorrectedTexture, _verticalBlurGammaCorrectionMaterial);
        }

        private void OnRenderObject()
        {
            int instanceId = Camera.current.GetInstanceID();
            if (instanceId == this._mainCamera.GetInstanceID())
            {
                _composeMaterial.SetPass(0);
                Graphics.DrawMeshNow(_fullscreenQuadMesh, Matrix4x4.identity);

                //_displayMainTextureMaterial.SetPass(0);
                //Graphics.DrawMeshNow(_fullscreenQuadMesh, Matrix4x4.identity);
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
            LoadShaderIfNotPresent(ref _brightPassShader, "Weasel Trust/Brightpass");
            LoadShaderIfNotPresent(ref _blurShader, "Weasel Trust/Horizontal Blur");
            LoadShaderIfNotPresent(ref _verticalBlurGammaCorrectionShader, "Weasel Trust/Vertical Blur Gamma Correction");
            LoadShaderIfNotPresent(ref _composeShader, "Weasel Trust/Compose");
            LoadShaderIfNotPresent(ref _upscaleBloomShader, "Weasel Trust/Upscale Bloom");

            LoadShaderIfNotPresent(ref _displayMainTextureShader, "Weasel Trust/Display Main Texture");

            _downsampleMaterial = new Material(_downsampleShader);
            _brightpassMaterial = new Material(_brightPassShader);
            _blurMaterial = new Material(_blurShader);
            _verticalBlurGammaCorrectionMaterial = new Material(_verticalBlurGammaCorrectionShader);
            _composeMaterial = new Material(_composeShader);
            _upscaleBloomMaterial = new Material(_upscaleBloomShader);

            _displayMainTextureMaterial = new Material(_displayMainTextureShader);

            var width = Screen.width;
            var height = Screen.height;

            int blurWidth = 32;
            int blurHeight = 128;

            int downsampleWidth = width / 4;
            int downsampleHeight = height / 4;

            _downsampledBrightpassTexture = CreateTransientRenderTexture("Bloom Downsample Pass", downsampleWidth, downsampleHeight);
            _preBloomTexture = CreateTransientRenderTexture("Pre Bloom", blurWidth, blurHeight);
            _horizontalBlurTexture = CreateTransientRenderTexture("Pre Bloom", blurWidth, blurHeight);
            _verticalBlurGammaCorrectedTexture = CreateTransientRenderTexture("Pre Bloom", blurWidth, blurHeight);

            _mainRenderTexture = CreateMainRenderTexture(width, height);
            _finalComposeTexture = CreateMainRenderTexture(width, height);

            _verticalBlurGammaCorrectionMaterial.SetTexture("_MainTex", _downsampledBrightpassTexture);
            _verticalBlurGammaCorrectionMaterial.SetTexture("_BloomTex", _horizontalBlurTexture);
            var ySpread = 1 / (float) blurHeight;
            _verticalBlurGammaCorrectionMaterial.SetFloat("_YSpread", ySpread);

            _blurMaterial.SetFloat("_XSpread", 1 / (float) blurWidth);
            _downsampleMaterial.SetVector("_TexelSize",
                new Vector4(1f / _downsampledBrightpassTexture.width, 1f / _downsampledBrightpassTexture.height, 
                0f, 0f));

            _composeMaterial.SetTexture("_MainTex", _mainRenderTexture);
            _composeMaterial.SetTexture("_BloomTex", _verticalBlurGammaCorrectedTexture);

            //===============
            _displayMainTextureMaterial.SetTexture("_MainTex", _verticalBlurGammaCorrectedTexture);
            _upscaleBloomMaterial.SetTexture("_BloomTex", _verticalBlurGammaCorrectedTexture);
            //===============

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
            Debug.Log("NAME: " + SystemInfo.graphicsDeviceName);
            var isTegra = SystemInfo.graphicsDeviceName.Contains("NVIDIA");
            var doesntSupportRgb565 = !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB565);

            var textureFormat = RenderTextureFormat.RGB565;
            if (isMetal || isTegra || doesntSupportRgb565)
            {
                textureFormat = RenderTextureFormat.ARGB32;
            }
            Debug.Log("FORMAT: " + textureFormat);

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

            DestroyImmediate(_downsampledBrightpassTexture);

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