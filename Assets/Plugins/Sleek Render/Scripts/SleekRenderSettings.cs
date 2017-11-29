using UnityEngine;

namespace SleekRender
{
    [CreateAssetMenu(menuName = "Sleek Render Settings")]
    public class SleekRenderSettings : ScriptableObject
    {
        [Header("Bloom")]
        [Range(0f, 1f)]
        public float bloomThreshold = 0.6f;

        [Range(0f, 15f)]
        public float bloomIntensity = 2.5f;

        [Header("HDR Compression")]
        [Range(0f, 1f)]
        public float gammaCompressionPower = 0.05f;

        [Range(0.01f, 100f)]
        public float hdrMaxIntensity = 1f;

        [Header("Color overlay (alpha sets intensity)")]
        public Color32 colorize = Color.clear;

        [Header("Vignette")]
        [Range(0f, 1f)]
        public float vignetteBeginRadius = 0.166f;

        public float vignetteExpandRadius = 1.34f;

        public Color vignetteColor = Color.black;
    }
}