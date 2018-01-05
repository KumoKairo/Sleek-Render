using UnityEngine;

namespace SleekRender
{
    [CreateAssetMenu(menuName = "Sleek Render Settings")]
    public class SleekRenderSettings : ScriptableObject
    {
        [Header("Bloom")]
        public bool bloomExpanded = false;
        public bool bloomEnabled = true;

        public float bloomThreshold = 0.6f;

        [Range(0f, 15f)]
        public float bloomIntensity = 2.5f;


        [Header("HDR Compression")]
        public bool hdrExpanded = true;
        public bool hdrCompressionEnabled = true;

        [Range(0f, 1f)]
        public float gammaCompressionPower = 0.05f;

        [Range(0.01f, 100f)]
        public float hdrMaxIntensity = 1f;

        [Header("Color overlay (alpha sets intensity)")]
        public bool colorizeExpanded = true;
        public bool colorizeEnabled = true;

        public Color32 colorize = Color.clear;

        [Header("Vignette")]
        public bool vignetteExpanded = true;
        public bool vignetteEnabled = true;

        [Range(0f, 1f)]
        public float vignetteBeginRadius = 0.166f;

        public float vignetteExpandRadius = 1.34f;

        public Color vignetteColor = Color.black;
    }
}