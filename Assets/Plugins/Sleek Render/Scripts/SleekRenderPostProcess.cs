using UnityEngine;
using UnityEngine.Rendering;

namespace SleekRender
{
    // Custom component editor view definition
    [AddComponentMenu("Effects/Sleek Render Post Process")]
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode, DisallowMultipleComponent]
    public class SleekRenderPostProcess : MonoBehaviour
    {
        // Keywords for shader variants
        private static class Keywords
        {
            public const string COLORIZE_ON = "COLORIZE_ON";
            public const string BLOOM_ON = "BLOOM_ON";
            public const string VIGNETTE_ON = "VIGNETTE_ON";
            public const string BRIGHTNESS_CONTRAST_ON = "BRIGHTNESS_CONTRAST_ON";
        }

        // Currently linked settings in the inspector
        public SleekRenderSettings settings;

        // Various Material cached objects
        // Created dynamically from found and loaded shaders
        private Material _preComposeMaterial;
        private Material _composeMaterial;

        private PassRenderer _passRenderer;
        private BloomRenderer _bloomRenderer;

        private RenderTexture _preComposeTexture;
        private RenderTexture _bloomResultTexture;

        // Currenly cached camera on which Post Processing stack is applied
        private Camera _mainCamera;

        // Cached camera width and height. Used in editor code for checking updated size for recreating resources
        private int _currentCameraPixelWidth;
        private int _currentCameraPixelHeight;

        // Various cached variables needed to avoid excessive shader enabling / disabling
        private bool _isColorizeAlreadyEnabled = false;
        private bool _isBloomAlreadyEnabled = false;
        private bool _isVignetteAlreadyEnabled = false;
        private bool _isContrastAndBrightnessAlreadyEnabled = false;

        private void OnEnable()
        {
            // If we are adding a component from scratch, we should supply fake settings with default values 
            // (until normal ones are linked)
            CreateDefaultSettingsIfNoneLinked();
            CreateResources();
        }

        private void OnDisable()
        {
            ReleaseResources();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture target)
        {
            // Editor only behaviour needed to recreate resources if viewport size changes (resizing editor window)
#if UNITY_EDITOR
            CreateDefaultSettingsIfNoneLinked();
            CheckScreenSizeAndRecreateTexturesIfNeeded(_mainCamera);
#endif
            // Applying post processing steps
            ApplyPostProcess(source);
            // Last step as separate pass
            Compose(source, target);
        }

        private void ApplyPostProcess(RenderTexture source)
        {
            var isBloomEnabled = settings.bloomEnabled;
            Bloom(source, isBloomEnabled);
            Precompose(source, isBloomEnabled);
        }

        private void Bloom(RenderTexture source, bool isBloomEnabled)
        {
            if (isBloomEnabled)
            {
                _bloomResultTexture = _bloomRenderer.ApplyToAndReturn(source, settings);
            }
        }

        private void Precompose(RenderTexture source, bool isBloomEnabled)
        {
            // Setting up vignette effect
            var isVignetteEnabledInSettings = settings.vignetteEnabled;
            if (isVignetteEnabledInSettings && !_isVignetteAlreadyEnabled)
            {
                _preComposeMaterial.EnableKeyword(Keywords.VIGNETTE_ON);
                _isVignetteAlreadyEnabled = true;
            }
            else if (!isVignetteEnabledInSettings && _isVignetteAlreadyEnabled)
            {
                _preComposeMaterial.DisableKeyword(Keywords.VIGNETTE_ON);
                _isVignetteAlreadyEnabled = false;
            }

            if (isVignetteEnabledInSettings)
            {
                // Calculating Vignette parameters once per frame rather than once per pixel
                float vignetteBeginRadius = settings.vignetteBeginRadius;
                float squareVignetteBeginRaduis = vignetteBeginRadius * vignetteBeginRadius;
                float vignetteRadii = vignetteBeginRadius + settings.vignetteExpandRadius;
                float oneOverVignetteRadiusDistance = 1f / (vignetteRadii - squareVignetteBeginRaduis);

                var vignetteColor = settings.vignetteColor;

                _preComposeMaterial.SetVector(Uniforms._VignetteShape, new Vector4(
                    4f * oneOverVignetteRadiusDistance * oneOverVignetteRadiusDistance,
                    -oneOverVignetteRadiusDistance * squareVignetteBeginRaduis));

                // Premultiplying Alpha of vignette color
                _preComposeMaterial.SetColor(Uniforms._VignetteColor, new Color(
                    vignetteColor.r * vignetteColor.a,
                    vignetteColor.g * vignetteColor.a,
                    vignetteColor.b * vignetteColor.a,
                    vignetteColor.a));
            }

            // Bloom is handled in two different passes (two blurring bloom passes and one precompose pass)
            // So we need to check for whether it's enabled in precompose step too (shader has variants without bloom)
            if (isBloomEnabled)
            {
                _preComposeMaterial.SetTexture(Uniforms._BloomTex, _bloomResultTexture);
                _preComposeMaterial.SetFloat(Uniforms._BloomIntencity, settings.bloomIntensity);
                _preComposeMaterial.SetColor(Uniforms._BloomTint, settings.bloomTint);

                if (!_isBloomAlreadyEnabled)
                {
                    _preComposeMaterial.EnableKeyword(Keywords.BLOOM_ON);
                    _isBloomAlreadyEnabled = true;
                }
            }
            else if (_isBloomAlreadyEnabled)
            {
                _preComposeMaterial.DisableKeyword(Keywords.BLOOM_ON);
                _isBloomAlreadyEnabled = false;
            }

            var sourceRenderTexture = isBloomEnabled ? _bloomResultTexture : source;
            // Finally applying precompose step. It slaps bloom and vignette together
            _passRenderer.Blit(sourceRenderTexture, _preComposeTexture, _preComposeMaterial);
        }

        private void Compose(RenderTexture source, RenderTexture target)
        {
            // Composing pass includes using full size main render texture + precompose texture
            // Precompose texture contains valuable info in its Alpha channel (whether to apply it on the final image or not)
            // Compose step also includes uniform colorizing which is calculated and enabled / disabled separately
            Color colorize = settings.colorize;
            var a = colorize.a;
            var colorizeConstant = new Color(colorize.r * a, colorize.g * a, colorize.b * a, 1f - a);
            _composeMaterial.SetColor(Uniforms._Colorize, colorizeConstant);

            if (settings.colorizeEnabled && !_isColorizeAlreadyEnabled)
            {
                _composeMaterial.EnableKeyword(Keywords.COLORIZE_ON);
                _isColorizeAlreadyEnabled = true;
            }
            else if (!settings.colorizeEnabled && _isColorizeAlreadyEnabled)
            {
                _composeMaterial.DisableKeyword(Keywords.COLORIZE_ON);
                _isColorizeAlreadyEnabled = false;
            }

            float normalizedContrast = settings.contrast + 1f;
            float normalizedBrightness = (settings.brightness + 1f) / 2f;
            var brightnessContrastPrecomputed = (-0.5f) * (normalizedContrast + 1f) + (normalizedBrightness * 2f); // optimization
            _composeMaterial.SetVector(Uniforms._BrightnessContrast, new Vector4(normalizedContrast, normalizedBrightness, brightnessContrastPrecomputed));

            if (settings.brightnessContrastEnabled && !_isContrastAndBrightnessAlreadyEnabled)
            {
                _composeMaterial.EnableKeyword(Keywords.BRIGHTNESS_CONTRAST_ON);
                _isContrastAndBrightnessAlreadyEnabled = true;
            }
            else if (!settings.brightnessContrastEnabled && _isContrastAndBrightnessAlreadyEnabled)
            {
                _composeMaterial.DisableKeyword(Keywords.BRIGHTNESS_CONTRAST_ON);
                _isContrastAndBrightnessAlreadyEnabled = false;
            }

            _passRenderer.Blit(source, target, _composeMaterial);
        }

        private void CreateResources()
        {
            _mainCamera = GetComponent<Camera>();

            _preComposeMaterial = HelperExtensions.CreateMaterialFromShader("Sleek Render/Post Process/PreCompose");
            _composeMaterial = HelperExtensions.CreateMaterialFromShader("Sleek Render/Post Process/Compose");

            _currentCameraPixelWidth = Mathf.RoundToInt(_mainCamera.pixelWidth);
            _currentCameraPixelHeight = Mathf.RoundToInt(_mainCamera.pixelHeight);

            // Point for future main render target size changing
            int width = _currentCameraPixelWidth;
            int height = _currentCameraPixelHeight;

            // Capping max base texture height in pixels
            // We usually don't need extra pixels for precompose and blur passes
            var maxHeight = Mathf.Min(height, 720);
            var ratio = (float)maxHeight / height;

            int precomposeWidth = Mathf.RoundToInt((width * ratio) / 5f);
            int precomposeHeight = Mathf.RoundToInt((height * ratio) / 5f);

            _preComposeTexture = HelperExtensions.CreateTransientRenderTexture("Pre Compose", precomposeWidth, precomposeHeight);

            _composeMaterial.SetTexture(Uniforms._PreComposeTex, _preComposeTexture);
            _composeMaterial.SetVector(Uniforms._LuminanceConst, new Vector4(0.2126f, 0.7152f, 0.0722f, 0f));

            _isColorizeAlreadyEnabled = false;
            _isBloomAlreadyEnabled = false;
            _isVignetteAlreadyEnabled = false;
            _isContrastAndBrightnessAlreadyEnabled = false;

            _passRenderer = _passRenderer ?? new PassRenderer();
            _bloomRenderer = _bloomRenderer ?? new BloomRenderer(_passRenderer);
            _bloomRenderer.CreateResources(settings);
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
            _preComposeMaterial.DestroyImmediateIfNotNull();
            _composeMaterial.DestroyImmediateIfNotNull();

            _preComposeTexture.DestroyImmediateIfNotNull();

            _bloomRenderer.ReleaseResources();
        }

        private void CheckScreenSizeAndRecreateTexturesIfNeeded(Camera mainCamera)
        {
            var cameraSizeHasChanged = mainCamera.pixelWidth != _currentCameraPixelWidth ||
                mainCamera.pixelHeight != _currentCameraPixelHeight;

            if (cameraSizeHasChanged)
            {
                ReleaseResources();
                CreateResources();
            }
        }

        private float GetCurrentAspect(Camera mainCamera)
        {
            const float SQUARE_ASPECT_CORRECTION = 0.7f;
            return mainCamera.aspect * SQUARE_ASPECT_CORRECTION;
        }

        private void CreateDefaultSettingsIfNoneLinked()
        {
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<SleekRenderSettings>();
                settings.name = "Default Settings";
            }
        }
    }
}