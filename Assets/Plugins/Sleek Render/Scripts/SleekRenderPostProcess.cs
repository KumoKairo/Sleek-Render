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
            public static readonly int _BloomIntencity = Shader.PropertyToID("_BloomIntencity");
            public static readonly int _MainTex = Shader.PropertyToID("_MainTex");
            public static readonly int _BloomTex = Shader.PropertyToID("_BloomTex");
            public static readonly int _PreComposeTex = Shader.PropertyToID("_PreComposeTex");
            public static readonly int _YSpread = Shader.PropertyToID("_YSpread");
            public static readonly int _XSpread = Shader.PropertyToID("_XSpread");
            public static readonly int _TexelSize = Shader.PropertyToID("_TexelSize");
            public static readonly int _Colorize = Shader.PropertyToID("_Colorize");
            public static readonly int _VignetteShape = Shader.PropertyToID("_VignetteShape");
            public static readonly int _VignetteColor = Shader.PropertyToID("_VignetteColor");
            public static readonly int _GammaCompressionPower = Shader.PropertyToID("_GammaCompressionPower");
            public static readonly int _GammaCompressionFactor = Shader.PropertyToID("_GammaCompressionFactor");
        }

        public SleekRenderSettings settings;
        private Material _downsampleMaterial;
        private Material _brightpassBlurMaterial;
        private Material _blurMaterial;
        private Material _verticalBlurGammaCorrectionMaterial;
        private Material _preComposeMaterial;
        private Material _composeMaterial;
        private Material _displayMainTextureMaterial;

        private RenderTexture _mainRenderTexture;
        private RenderTexture _downsampledBrightpassTexture;
        private RenderTexture _brightPassBlurTexture;
        private RenderTexture _horizontalBlurTexture;
        private RenderTexture _verticalBlurGammaCorrectedTexture;
        private RenderTexture _preComposeTexture;
        private RenderTexture _finalComposeTexture;

        private Camera _mainCamera;
        private Camera _renderCamera;
        private Mesh _fullscreenQuadMesh;
        private int _originalCullingMask;
        private CameraClearFlags _originalClearFlags;

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
            ApplyPostProcess();
        }

        private void OnPreCull()
        {
            _originalCullingMask = _mainCamera.cullingMask;
            _mainCamera.cullingMask = 0;

            _originalClearFlags = _mainCamera.clearFlags;
            _mainCamera.clearFlags = CameraClearFlags.SolidColor;
        }

        private void OnPostRender()
        {
            _mainCamera.cullingMask = _originalCullingMask;
            _mainCamera.clearFlags = _originalClearFlags;

            Compose();
        }

        private void ApplyPostProcess()
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

            float vignetteBeginRadius = settings.vignetteBeginRadius;
            float squareVignetteBeginRaduis = vignetteBeginRadius * vignetteBeginRadius;
            float vignetteRadii = vignetteBeginRadius + settings.vignetteExpandRadius;
            float oneOverVignetteRadiusDistance = 1f / (vignetteRadii - squareVignetteBeginRaduis);

            var vignetteColor = settings.vignetteColor;

            _preComposeMaterial.SetVector(Uniforms._VignetteShape, new Vector4(
                4f * oneOverVignetteRadiusDistance * oneOverVignetteRadiusDistance,
                -oneOverVignetteRadiusDistance * squareVignetteBeginRaduis));

            _preComposeMaterial.SetColor(Uniforms._VignetteColor, new Color(
                    vignetteColor.r * vignetteColor.a, 
                    vignetteColor.g * vignetteColor.a,
                    vignetteColor.b * vignetteColor.a,
                    vignetteColor.a));

            _verticalBlurGammaCorrectionMaterial.SetVector(Uniforms._VignetteShape, new Vector4(
                4f * oneOverVignetteRadiusDistance * oneOverVignetteRadiusDistance,
                -oneOverVignetteRadiusDistance * squareVignetteBeginRaduis));

            _verticalBlurGammaCorrectionMaterial.SetColor(Uniforms._VignetteColor, new Color(
                vignetteColor.r * vignetteColor.a,
                vignetteColor.g * vignetteColor.a,
                vignetteColor.b * vignetteColor.a,
                vignetteColor.a));

            _downsampleMaterial.SetVector(Uniforms._LuminanceConst, luminanceConst);

            Blit(_mainRenderTexture, _downsampledBrightpassTexture, _downsampleMaterial);

            Blit(_downsampledBrightpassTexture, _brightPassBlurTexture, _brightpassBlurMaterial);

            Blit(_brightPassBlurTexture, _verticalBlurGammaCorrectedTexture, _verticalBlurGammaCorrectionMaterial);

            _preComposeMaterial.SetFloat(Uniforms._BloomIntencity, settings.bloomIntensity);

            float gammaCompressionPower = settings.gammaCompressionPower;
            _preComposeMaterial.SetFloat(Uniforms._GammaCompressionPower, gammaCompressionPower);
            _preComposeMaterial.SetFloat(Uniforms._GammaCompressionFactor, Mathf.Pow(settings.hdrMaxIntensity, -gammaCompressionPower));

            Blit(_downsampledBrightpassTexture, _preComposeTexture, _preComposeMaterial);
        }

        private void Compose()
        {
            _composeMaterial.SetColor(Uniforms._Colorize, settings.colorize);
            _composeMaterial.SetPass(0);
            Graphics.DrawMeshNow(_fullscreenQuadMesh, Matrix4x4.identity);
        }

        private void CreateResources()
        {
            _mainCamera = GetComponent<Camera>();

            var downsampleShader = Shader.Find("Sleek Render/Post Process/Downsample Brightpass");
            var brightPassShader = Shader.Find("Sleek Render/Post Process/Brightpass Blur");
            var blurShader = Shader.Find("Sleek Render/Post Process/Horizontal Gaussian Blur");
            var verticalBlurGammaCorrectionShader = Shader.Find("Sleek Render/Post Process/Vertical Gaussian Blur Gamma Correction");
            var composeShader = Shader.Find("Sleek Render/Post Process/Compose");
            var displayMainTextureShader = Shader.Find("Sleek Render/Post Process/Display Main Texture");
            var preComposeShader = Shader.Find("Sleek Render/Post Process/PreCompose");

            _downsampleMaterial = new Material(downsampleShader);
            _brightpassBlurMaterial = new Material(brightPassShader);
            _blurMaterial = new Material(blurShader);
            _verticalBlurGammaCorrectionMaterial = new Material(verticalBlurGammaCorrectionShader);
            _preComposeMaterial = new Material(preComposeShader);
            _composeMaterial = new Material(composeShader);
            _displayMainTextureMaterial = new Material(displayMainTextureShader);

            _currentCameraPixelWidth = Mathf.RoundToInt(_mainCamera.pixelWidth);
            _currentCameraPixelHeight = Mathf.RoundToInt(_mainCamera.pixelHeight);

            int width = _currentCameraPixelWidth;
            int height = _currentCameraPixelHeight;

            var maxHeight = Mathf.Min(height, 720);
            var ratio = (float) maxHeight / height;

            int blurWidth = 32;
            int blurHeight = 128;

            int downsampleWidth = Mathf.RoundToInt((width * ratio) / 5);
            int downsampleHeight = Mathf.RoundToInt((height * ratio) / 5);

            _downsampledBrightpassTexture = CreateTransientRenderTexture("Bloom Downsample Pass", downsampleWidth, downsampleHeight);
            _brightPassBlurTexture = CreateTransientRenderTexture("Pre Bloom", blurWidth, blurHeight);
            _horizontalBlurTexture = CreateTransientRenderTexture("Horizontal Blur", blurWidth, blurHeight);
            _verticalBlurGammaCorrectedTexture = CreateTransientRenderTexture("Vertical Blur", blurWidth, blurHeight);
            _preComposeTexture = CreateTransientRenderTexture("Pre Compose", downsampleWidth, downsampleHeight);

            _mainRenderTexture = CreateMainRenderTexture(width, height);
            _finalComposeTexture = CreateMainRenderTexture(width, height);

            _verticalBlurGammaCorrectionMaterial.SetTexture(Uniforms._MainTex, _downsampledBrightpassTexture);
            _verticalBlurGammaCorrectionMaterial.SetTexture(Uniforms._BloomTex, _horizontalBlurTexture);

            var ySpread = 1 / (float) blurHeight;
            var xSpread = 1 / (float) blurWidth;
            var blurTexelSize = new Vector4(xSpread, ySpread);
            _verticalBlurGammaCorrectionMaterial.SetVector(Uniforms._TexelSize, blurTexelSize);
            _brightpassBlurMaterial.SetVector(Uniforms._TexelSize, blurTexelSize);

            _preComposeMaterial.SetTexture(Uniforms._BloomTex, _verticalBlurGammaCorrectedTexture);

            var downsampleTexelSize = new Vector4(1f / _downsampledBrightpassTexture.width, 1f / _downsampledBrightpassTexture.height);
            _downsampleMaterial.SetVector(Uniforms._TexelSize, downsampleTexelSize);

            _composeMaterial.SetTexture(Uniforms._MainTex, _mainRenderTexture);
            _composeMaterial.SetTexture(Uniforms._PreComposeTex, _preComposeTexture);

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

#if UNITY_EDITOR
            textureFormat = RenderTextureFormat.ARGB32;
#endif

            var renderTexture = new RenderTexture(width, height, 16, textureFormat);
            var antialiasingSamples = QualitySettings.antiAliasing;
            renderTexture.antiAliasing = antialiasingSamples == 0 ? 1 : antialiasingSamples;
            return renderTexture;
        }

        private void ReleaseResources()
        {
            DestroyImmediateIfNotNull(_downsampleMaterial);
            DestroyImmediateIfNotNull(_brightpassBlurMaterial);
            DestroyImmediateIfNotNull(_blurMaterial);
            DestroyImmediateIfNotNull(_verticalBlurGammaCorrectionMaterial);
            DestroyImmediateIfNotNull(_preComposeMaterial);
            DestroyImmediateIfNotNull(_composeMaterial);
            DestroyImmediateIfNotNull(_displayMainTextureMaterial);

            DestroyImmediateIfNotNull(_mainRenderTexture);
            DestroyImmediateIfNotNull(_downsampledBrightpassTexture);
            DestroyImmediateIfNotNull(_brightPassBlurTexture);
            DestroyImmediateIfNotNull(_horizontalBlurTexture);
            DestroyImmediateIfNotNull(_verticalBlurGammaCorrectedTexture);
            DestroyImmediateIfNotNull(_preComposeTexture);
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