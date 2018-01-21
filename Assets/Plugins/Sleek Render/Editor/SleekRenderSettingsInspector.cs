using UnityEditor;
using UnityEngine;

namespace SleekRender
{
    [CustomEditor(typeof(SleekRenderSettings))]
    public class SleekRenderSettingsInspector : Editor
    {
        private SerializedProperty _isBloomGroupExpandedProperty;
        private SerializedProperty _bloomEnabledProperty;
        private SerializedProperty _bloomThresholdProperty;
        private SerializedProperty _bloomIntensityProperty;

        private SerializedProperty _isHdrGroupExpandedProperty;
        private SerializedProperty _hdrCompressionEnabledProperty;
        private SerializedProperty _gammaCompressionPowerProperty;
        private SerializedProperty _hdrMaxIntensityProperty;

        private SerializedProperty _isColorizeGroupExpandedProperty;
        private SerializedProperty _colorizeEnabledProperty;
        private SerializedProperty _colorizeProperty;

        private SerializedProperty _isVignetteExpandedProperty;
        private SerializedProperty _vignetteEnabledProperty;
        private SerializedProperty _vignetteBeginRadiusProperty;
        private SerializedProperty _vignetteExpandRadiusProperty;
        private SerializedProperty _vignetteColorProperty;

        private void OnEnable()
        {
            _isBloomGroupExpandedProperty = serializedObject.FindProperty("bloomExpanded");
            _bloomEnabledProperty = serializedObject.FindProperty("bloomEnabled");
            _bloomThresholdProperty = serializedObject.FindProperty("bloomThreshold");
            _bloomIntensityProperty = serializedObject.FindProperty("bloomIntensity");

            _isHdrGroupExpandedProperty = serializedObject.FindProperty("hdrExpanded");
            _hdrCompressionEnabledProperty = serializedObject.FindProperty("hdrCompressionEnabled");
            _gammaCompressionPowerProperty = serializedObject.FindProperty("gammaCompressionPower");
            _hdrMaxIntensityProperty = serializedObject.FindProperty("hdrMaxIntensity");

            _isColorizeGroupExpandedProperty = serializedObject.FindProperty("colorizeExpanded");
            _colorizeEnabledProperty = serializedObject.FindProperty("colorizeEnabled");
            _colorizeProperty = serializedObject.FindProperty("colorize");

            _isVignetteExpandedProperty = serializedObject.FindProperty("vignetteExpanded");
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

            DrawVignetteEditor();

            EditorGUI.indentLevel = indent;
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawVignetteEditor()
        {
            Header("Vignette",
                _isVignetteExpandedProperty, _vignetteEnabledProperty);

            if (_isVignetteExpandedProperty.boolValue)
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
        }

        private void DrawColorizeEditor()
        {
            Header("Colorize",
                _isColorizeGroupExpandedProperty, _colorizeEnabledProperty);

            if (_isColorizeGroupExpandedProperty.boolValue)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.LabelField("Color");
                _colorizeProperty.colorValue = EditorGUILayout.ColorField("", _colorizeProperty.colorValue);
                EditorGUI.indentLevel -= 2;
            }
        }

        private void DrawHdrCompressionEditor()
        {
            Header("HDR Compression",
                _isHdrGroupExpandedProperty, _hdrCompressionEnabledProperty);

            if (_isHdrGroupExpandedProperty.boolValue)
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
            Header("Bloom",
                _isBloomGroupExpandedProperty, _bloomEnabledProperty);

            if (_isBloomGroupExpandedProperty.boolValue)
            {
                EditorGUI.indentLevel += 2;

                EditorGUILayout.LabelField("Bloom threshold");
                EditorGUILayout.Slider(_bloomThresholdProperty, 0f, 1f, "");
                EditorGUILayout.LabelField("Bloom intensity");
                EditorGUILayout.Slider(_bloomIntensityProperty, 0f, 15f, "");

                EditorGUI.indentLevel -= 2;
            }
        }

        public static bool Header(string title, SerializedProperty isExpanded,
            SerializedProperty enabledField)
        {
            var display = isExpanded == null || isExpanded.boolValue;
            var enabled = enabledField.boolValue;

            var rect = GUILayoutUtility.GetRect(16f, 22f, FxStyles.header);
            GUI.Box(rect, title, FxStyles.header);

            var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            var e = Event.current;

            if (e.type == EventType.Repaint)
            {
                FxStyles.headerCheckbox.Draw(toggleRect, false, false, enabled, false);
            }

            if (e.type == EventType.MouseDown)
            {
                const float kOffset = 2f;
                toggleRect.x -= kOffset;
                toggleRect.y -= kOffset;
                toggleRect.width += kOffset * 2f;
                toggleRect.height += kOffset * 2f;

                if (toggleRect.Contains(e.mousePosition))
                {
                    enabledField.boolValue = !enabledField.boolValue;
                    e.Use();
                }
                else if (rect.Contains(e.mousePosition) && isExpanded != null)
                {
                    display = !display;
                    isExpanded.boolValue = !isExpanded.boolValue;
                    e.Use();
                }
            }

            return display;
        }
    }
}
