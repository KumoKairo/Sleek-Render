using UnityEditor;
using UnityEngine;

namespace SleekRender
{
    [CustomEditor(typeof(SleekRenderSettings))]
    public class SleekRenderSettingsInspector : Editor
    {
        private SerializedProperty _bloomEnabledProperty;
        private SerializedProperty _bloomThresholdProperty;
        private SerializedProperty _bloomIntensityProperty;

        private SerializedProperty _hdrCompressionEnabledProperty;
        private SerializedProperty _gammaCompressionPowerProperty;
        private SerializedProperty _hdrMaxIntensityProperty;

        private SerializedProperty _colorizeEnabledProperty;
        private SerializedProperty _colorizeProperty;

        private SerializedProperty _vignetteEnabledProperty;
        private SerializedProperty _vignetteBeginRadiusProperty;
        private SerializedProperty _vignetteExpandRadiusProperty;
        private SerializedProperty _vignetteColorProperty;

        private void OnEnable()
        {
            _bloomEnabledProperty = serializedObject.FindProperty("bloomEnabled");
            _bloomThresholdProperty = serializedObject.FindProperty("bloomThreshold");
            _bloomIntensityProperty = serializedObject.FindProperty("bloomIntensity");

            _hdrCompressionEnabledProperty = serializedObject.FindProperty("hdrCompressionEnabled");
            _gammaCompressionPowerProperty = serializedObject.FindProperty("gammaCompressionPower");
            _hdrMaxIntensityProperty = serializedObject.FindProperty("hdrMaxIntensity");

            _colorizeEnabledProperty = serializedObject.FindProperty("colorizeEnabled");
            _colorizeProperty = serializedObject.FindProperty("colorize");

            _vignetteEnabledProperty = serializedObject.FindProperty("vignetteEnabled");
            _vignetteBeginRadiusProperty = serializedObject.FindProperty("vignetteBeginRadius");
            _vignetteExpandRadiusProperty = serializedObject.FindProperty("vignetteExpandRadius");
            _vignetteColorProperty = serializedObject.FindProperty("vignetteColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int indent = EditorGUI.indentLevel;

            DrawBloomEditor();
            EditorGUILayout.Space();

            DrawHdrCompressionEditor();
            EditorGUILayout.Space();

            DrawColorizeEditor();
            EditorGUILayout.Space();

            _vignetteEnabledProperty.boolValue = EditorGUIHelper.Header("Vignette",
                _vignetteEnabledProperty, _vignetteEnabledProperty);

            if (_vignetteEnabledProperty.boolValue)
            {
                EditorGUI.indentLevel += 1;

                EditorGUILayout.LabelField("Begin radius");
                EditorGUILayout.Slider(_vignetteBeginRadiusProperty, 0f, 1f, "");

                EditorGUILayout.LabelField("Expand radius");
                EditorGUILayout.Slider(_vignetteExpandRadiusProperty, 0f, 3f, "");

                EditorGUILayout.LabelField("Color");
                _vignetteColorProperty.colorValue = EditorGUILayout.ColorField("", _vignetteColorProperty.colorValue);

                EditorGUI.indentLevel -= 1;
            }

            EditorGUI.indentLevel = indent;
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawColorizeEditor()
        {
            _colorizeEnabledProperty.boolValue = EditorGUIHelper.Header("Colorize",
                _colorizeEnabledProperty, _colorizeEnabledProperty);

            if (_colorizeEnabledProperty.boolValue)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.LabelField("Color");
                _colorizeProperty.colorValue = EditorGUILayout.ColorField("", _colorizeProperty.colorValue);
                EditorGUI.indentLevel -= 2;
            }
        }

        private void DrawHdrCompressionEditor()
        {
            _hdrCompressionEnabledProperty.boolValue = EditorGUIHelper.Header("HDR Compression",
                _hdrCompressionEnabledProperty, _hdrCompressionEnabledProperty);

            if (_hdrCompressionEnabledProperty.boolValue)
            {
                EditorGUI.indentLevel += 2;

                EditorGUILayout.LabelField("Gamma compression power");
                EditorGUILayout.Slider(_gammaCompressionPowerProperty, 0f, 1f, "");
                EditorGUILayout.LabelField("HDR Max Intensity");
                EditorGUILayout.Slider(_hdrMaxIntensityProperty, 0f, 15f, "");

                EditorGUI.indentLevel -= 2;
            }
        }

        private void DrawBloomEditor()
        {
            _bloomEnabledProperty.boolValue = EditorGUIHelper.Header("Bloom",
                _bloomEnabledProperty, _bloomEnabledProperty);

            if (_bloomEnabledProperty.boolValue)
            {
                EditorGUI.indentLevel += 2;

                EditorGUILayout.LabelField("Bloom threshold");
                EditorGUILayout.Slider(_bloomThresholdProperty, 0f, 1f, "");
                EditorGUILayout.LabelField("Bloom intensity");
                EditorGUILayout.Slider(_bloomIntensityProperty, 0f, 15f, "");

                EditorGUI.indentLevel -= 2;
            }
        }
    }
}
