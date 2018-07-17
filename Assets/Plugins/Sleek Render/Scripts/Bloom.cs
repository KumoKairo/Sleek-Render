using UnityEngine;

namespace SleekRender
{
    public class Bloom
    {
        private RenderTexture[] _blurTextures;
        private Material _brightpassBlurMaterial;
        private Material _downsampleBlurMaterial;
        private PassRenderer _renderer;

        public Bloom(PassRenderer renderer)
        {
            _renderer = renderer;
        }

        public RenderTexture ApplyToAndReturn(RenderTexture source, SleekRenderSettings settings)
        {
            float oneOverOneMinusBloomThreshold = 1f / (1f - settings.bloomThreshold);
            var luma = settings.bloomLumaVector;
            Vector4 luminanceThreshold = new Vector4(
                luma.x * oneOverOneMinusBloomThreshold,
                luma.y * oneOverOneMinusBloomThreshold,
                luma.z * oneOverOneMinusBloomThreshold, -settings.bloomThreshold * oneOverOneMinusBloomThreshold);

            // Changing current Luminance Const value just to make sure that we have the latest settings in our Uniforms
            _brightpassBlurMaterial.SetVector(Uniforms._LuminanceThreshold, luminanceThreshold);

            var currentTargetRenderTexture = _blurTextures[0];
            var previousTargetRenderTexture = _blurTextures[0];
            for (int i = 0; i < _blurTextures.Length; i++)
            {
                currentTargetRenderTexture = _blurTextures[i];

                // We use a different material for the first blur pass
                if (i == 0)
                {
                    // Applying downsample + brightpass (stored in Alpha)
                    _renderer.Blit(source, currentTargetRenderTexture, _brightpassBlurMaterial);
                }
                else
                {
                    // Applying only blur to our already brightpassed texture
                    _renderer.Blit(previousTargetRenderTexture, currentTargetRenderTexture, _brightpassBlurMaterial);
                }

                previousTargetRenderTexture = currentTargetRenderTexture;
            }

            return currentTargetRenderTexture;
        }

        public void CreateResources(SleekRenderSettings settings)
        {
            _blurTextures = new RenderTexture[2];
            _blurTextures[0] = HelperExtensions.CreateTransientRenderTexture("Brightpass Blur 0", 128, 128);
            _blurTextures[1] = HelperExtensions.CreateTransientRenderTexture("Downsample Blur 1", 128, 128);
        }

        public void ReleaseResources()
        {

        }
    }
}
