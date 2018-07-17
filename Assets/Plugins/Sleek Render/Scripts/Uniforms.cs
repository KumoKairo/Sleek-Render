
using UnityEngine;

namespace SleekRender
{
    public static class Uniforms
    {
        public static readonly int _LuminanceThreshold = Shader.PropertyToID("_LuminanceThreshold");
        public static readonly int _LuminanceConst = Shader.PropertyToID("_LuminanceConst");
        public static readonly int _BloomIntencity = Shader.PropertyToID("_BloomIntencity");
        public static readonly int _BloomTint = Shader.PropertyToID("_BloomTint");
        public static readonly int _MainTex = Shader.PropertyToID("_MainTex");
        public static readonly int _BloomTex = Shader.PropertyToID("_BloomTex");
        public static readonly int _PreComposeTex = Shader.PropertyToID("_PreComposeTex");
        public static readonly int _TexelSize = Shader.PropertyToID("_TexelSize");
        public static readonly int _Colorize = Shader.PropertyToID("_Colorize");
        public static readonly int _VignetteShape = Shader.PropertyToID("_VignetteShape");
        public static readonly int _VignetteColor = Shader.PropertyToID("_VignetteColor");
        public static readonly int _BrightnessContrast = Shader.PropertyToID("_BrightnessContrast");
    }
}