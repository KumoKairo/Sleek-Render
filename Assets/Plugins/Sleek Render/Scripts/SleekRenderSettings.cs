using UnityEngine;

[CreateAssetMenu(menuName = "Sleek Render/Settings")]
public class SleekRenderSettings : ScriptableObject
{
    [Range(0f, 1f)]
    public float bloomThreshold = 0.8f;

    [Range(0f, 3f)]
    public float bloomIntensity = 2.5f;
}
