using UnityEngine;

namespace SleekRender
{
    public class Bloom
    {
        private static class Uniforms
        {
            public static readonly int _LuminanceThreshold = Shader.PropertyToID("_LuminanceThreshold");
        }

        private RenderTexture[] _blurTextures;
    }
}
