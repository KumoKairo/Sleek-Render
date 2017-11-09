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
        public Color32 colorize = Color.white;
    }
}