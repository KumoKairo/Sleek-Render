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

        [Header("Color overlay (alpha sets intensity)")]
        public Color32 colorize = Color.clear;

        [HideInInspector]
        [Header("Vignette")]
        [Range(0f, 1f)]
        public float vignetteBeginRadius = 0f;

        [HideInInspector]
        [Range(0f, 1f)]
        public float vignetteEndRadius = 0f;

        [HideInInspector]
        public Color32 vignetteColor = Color.black;
    }
}