using UnityEngine;
using UnityEngine.Rendering;

namespace SleekRender
{
    [AddComponentMenu("Effects/Sleek Render Post Process")]
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode, DisallowMultipleComponent]
    public class SleekRenderPostProcess : MonoBehaviour
    {
        private static class Uniforms
        {
            public static readonly int _LuminanceConst = Shader.PropertyToID("_LuminanceConst");
            public static readonly int _SpreadDirection = Shader.PropertyToID("_SpreadDirection");
            public static readonly int _BloomIntencity = Shader.PropertyToID("_BloomIntencity");
            public static readonly int _MainTex = Shader.PropertyToID("_MainTex");
            public static readonly int _BloomTex = Shader.PropertyToID("_BloomTex");
            public static readonly int _YSpread = Shader.PropertyToID("_YSpread");
            public static readonly int _XSpread = Shader.PropertyToID("_XSpread");
            public static readonly int _TexelSize = Shader.PropertyToID("_TexelSize");
        }

        public SleekRenderSettings settings;
        private Material _downsampleMaterial;
        private Material _brightpassMaterial;
        private Material _blurMaterial;
        private Material _verticalBlurGammaCorrectionMaterial;
        private Material _composeMaterial;
        private Material _displayMainTextureMaterial;

        private RenderTexture _mainRenderTexture;
        private RenderTexture _downsampledBrightpassTexture;
        private RenderTexture _preBloomTexture;
        private RenderTexture _horizontalBlurTexture;
        private RenderTexture _verticalBlurGammaCorrectedTexture;
        private RenderTexture _finalComposeTexture;

        private Camera _mainCamera;
        private Camera _renderCamera;
        private Mesh _fullscreenQuadMesh;
        private int _originalCullingMask;

        private int _currentCameraPixelWidth;
        private int _currentCameraPixelHeight;

        private void OnEnable()
        {
            CreateDefaultSettingsIfNoneLinked();
            CreateResources();
        }

        private void OnDisable()
        {
            ReleaseResources();
        }

        private void LateUpdate()
        {
            #if UNITY_EDITOR
            CheckScreenSizeAndRecreateTexturesIfNeeded(_mainCamera);
            #endif

            PrepareRenderCamera(_renderCamera, _mainCamera);

            _mainRenderTexture.DiscardContents(true, true);
            _renderCamera.Render();
            ApplyBloom();
        }

        private CameraClearFlags clearFlags;
        private void OnPreCull()
        {
            _originalCullingMask = _mainCamera.cullingMask;
            _mainCamera.cullingMask = 0;

            clearFlags = _mainCamera.clearFlags;
            _mainCamera.clearFlags = CameraClearFlags.SolidColor;
        }

        private void OnPostRender()
        {
            _mainCamera.cullingMask = _originalCullingMask;
            _mainCamera.clearFlags = clearFlags;

            _composeMaterial.SetPass(0);
            Graphics.DrawMeshNow(_fullscreenQuadMesh, Matrix4x4.identity);
        }

        private void ApplyBloom()
        {
            #if UNITY_EDITOR
            CreateDefaultSettingsIfNoneLinked();
            #endif

            float oneOverOneMinusBloomThreshold = 1f / (1f - settings.bloomThreshold);
            Vector4 luminanceConst = new Vector4(
                0.2126f * oneOverOneMinusBloomThreshold,
                0.7152f * oneOverOneMinusBloomThreshold,
                0.0722f * oneOverOneMinusBloomThreshold,
                -settings.bloomThreshold * oneOverOneMinusBloomThreshold);

            _downsampleMaterial.SetVector(Uniforms._LuminanceConst, luminanceConst);

            Blit(_mainRenderTexture, _downsampledBrightpassTexture, _downsampleMaterial);

            Blit(_downsampledBrightpassTexture, _preBloomTexture, _brightpassMaterial);

            _blurMaterial.SetVector(Uniforms._SpreadDirection, new Vector4(1f, 0f, 0f, 0f));
            Blit(_preBloomTexture, _horizontalBlurTexture, _blurMaterial);

            _verticalBlurGammaCorrectionMaterial.SetFloat(Uniforms._BloomIntencity, settings.bloomIntensity);
            _verticalBlurGammaCorrectionMaterial.SetPass(0);
            Blit(_horizontalBlurTexture, _verticalBlurGammaCorrectedTexture, _verticalBlurGammaCorrectionMaterial);
        }

        private void CreateResources()
        {
            _mainCamera = GetComponent<Camera>();

            var downsampleShader = Shader.Find("Sleek Render/Post Process/Downsample Brightpass");
            var brightPassShader = Shader.Find("Sleek Render/Post Process/Brightpass");
            var blurShader = Shader.Find("Sleek Render/Post Process/Horizontal Gaussian Blur");
            var verticalBlurGammaCorrectionShader = Shader.Find("Sleek Render/Post Process/Vertical Gaussian Blur Gamma Correction");
            var composeShader = Shader.Find("Sleek Render/Post Process/Compose");
            var displayMainTextureShader = Shader.Find("Sleek Render/Post Process/Display Main Texture");

            _downsampleMaterial = new Material(downsampleShader);
            _brightpassMaterial = new Material(brightPassShader);
            _blurMaterial = new Material(blurShader);
            _verticalBlurGammaCorrectionMaterial = new Material(verticalBlurGammaCorrectionShader);
            _composeMaterial = new Material(composeShader);
            _displayMainTextureMaterial = new Material(displayMainTextureShader);

            _currentCameraPixelWidth = _mainCamera.pixelWidth;
            _currentCameraPixelHeight = _mainCamera.pixelHeight;

            var width = _currentCameraPixelWidth;
            var height = _currentCameraPixelHeight;

            var maxHeight = Mathf.Min(height, 720);
            var ratio = (float) maxHeight / height;

            int blurWidth = 32;
            int blurHeight = 128;

            int downsampleWidth = Mathf.RoundToInt((width * ratio) / 4);
            int downsampleHeight = Mathf.RoundToInt((height * ratio) / 4);

            _downsampledBrightpassTexture = CreateTransientRenderTexture("Bloom Downsample Pass", downsampleWidth, downsampleHeight);
            _preBloomTexture = CreateTransientRenderTexture("Pre Bloom", blurWidth, blurHeight);
            _horizontalBlurTexture = CreateTransientRenderTexture("Pre Bloom", blurWidth, blurHeight);
            _verticalBlurGammaCorrectedTexture = CreateTransientRenderTexture("Pre Bloom", blurWidth, blurHeight);

            _mainRenderTexture = CreateMainRenderTexture(width, height);
            _finalComposeTexture = CreateMainRenderTexture(width, height);

            _verticalBlurGammaCorrectionMaterial.SetTexture(Uniforms._MainTex, _downsampledBrightpassTexture);
            _verticalBlurGammaCorrectionMaterial.SetTexture(Uniforms._BloomTex, _horizontalBlurTexture);
            var ySpread = 1 / (float) blurHeight;
            _verticalBlurGammaCorrectionMaterial.SetFloat(Uniforms._YSpread, ySpread);

            _blurMaterial.SetFloat(Uniforms._XSpread, 1.0f / (width * 0.055f));
            _downsampleMaterial.SetVector(Uniforms._TexelSize,
                new Vector4(1f / _downsampledBrightpassTexture.width, 1f / _downsampledBrightpassTexture.height, 
                0f, 0f));

            _composeMaterial.SetTexture(Uniforms._MainTex, _mainRenderTexture);
            _composeMaterial.SetTexture(Uniforms._BloomTex, _verticalBlurGammaCorrectedTexture);

            var renderCameraGameObject = new GameObject("Bloom Render Camera");
            renderCameraGameObject.hideFlags = HideFlags.HideAndDontSave;
            _renderCamera = renderCameraGameObject.AddComponent<Camera>();
            _renderCamera.CopyFrom(_mainCamera);
            _renderCamera.enabled = false;

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
            var rgb565NotSupported = !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB565);

            var textureFormat = RenderTextureFormat.RGB565;
            if (isMetal || isTegra || rgb565NotSupported)
            {
                textureFormat = RenderTextureFormat.ARGB32;
            }

            var renderTexture = new RenderTexture(width, height, 16, textureFormat);
            var antialiasingSamples = QualitySettings.antiAliasing;
            renderTexture.antiAliasing = antialiasingSamples == 0 ? 1 : antialiasingSamples;
            return renderTexture;
        }

        private void ReleaseResources()
        {
            DestroyImmediateIfNotNull(_downsampleMaterial);
            DestroyImmediateIfNotNull(_brightpassMaterial);
            DestroyImmediateIfNotNull(_blurMaterial);
            DestroyImmediateIfNotNull(_verticalBlurGammaCorrectionMaterial);
            DestroyImmediateIfNotNull(_composeMaterial);
            DestroyImmediateIfNotNull(_displayMainTextureMaterial);

            DestroyImmediateIfNotNull(_mainRenderTexture);
            DestroyImmediateIfNotNull(_downsampledBrightpassTexture);
            DestroyImmediateIfNotNull(_preBloomTexture);
            DestroyImmediateIfNotNull(_horizontalBlurTexture);
            DestroyImmediateIfNotNull(_verticalBlurGammaCorrectedTexture);
            DestroyImmediateIfNotNull(_finalComposeTexture);

            DestroyImmediateIfNotNull(_fullscreenQuadMesh);

            if(_renderCamera != null)
            {
                DestroyImmediateIfNotNull(_renderCamera.gameObject);
            }
        }

        private void DestroyImmediateIfNotNull(Object obj)
        {
            if(obj != null)
            {
                DestroyImmediate(obj);
            }
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
            material.SetTexture(Uniforms._MainTex, source);
            material.SetPass(materialPass);
            Graphics.DrawMeshNow(_fullscreenQuadMesh, Matrix4x4.identity);
        }

        private void PrepareRenderCamera(Camera renderCamera, Camera mainCamera)
        {
            renderCamera.CopyFrom(mainCamera);
            renderCamera.targetTexture = _mainRenderTexture;
        }

        private void CheckScreenSizeAndRecreateTexturesIfNeeded(Camera mainCamera)
        {
            if(mainCamera.pixelWidth != _currentCameraPixelWidth || mainCamera.pixelHeight != _currentCameraPixelHeight)
            {
                ReleaseResources();
                CreateResources();
            }
        }

        private void CreateDefaultSettingsIfNoneLinked()
        {
            if(settings == null)
            {
                settings = ScriptableObject.CreateInstance<SleekRenderSettings>();
                settings.name = "Default Settings";
            }
        }

        private void DrawDebugTexture(Texture texture)
        {
            _displayMainTextureMaterial.SetTexture(Uniforms._MainTex, texture);
            _displayMainTextureMaterial.SetPass(0);
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