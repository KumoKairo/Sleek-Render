using System;
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;

namespace SleekRender
{
    public static class EditorGUIHelper
    {
        static EditorGUIHelper()
        {
            s_GUIContentCache = new Dictionary<string, GUIContent>();
        }

        #region GUIContent caching

        static Dictionary<string, GUIContent> s_GUIContentCache;

        public static GUIContent GetContent(string textAndTooltip)
        {
            if (string.IsNullOrEmpty(textAndTooltip))
                return GUIContent.none;

            GUIContent content;

            if (!s_GUIContentCache.TryGetValue(textAndTooltip, out content))
            {
                var s = textAndTooltip.Split('|');
                content = new GUIContent(s[0]);

                if (s.Length > 1 && !string.IsNullOrEmpty(s[1]))
                    content.tooltip = s[1];

                s_GUIContentCache.Add(textAndTooltip, content);
            }

            return content;
        }

        #endregion

        public static bool Header(string title, SerializedProperty group, Action resetAction)
        {
            var rect = GUILayoutUtility.GetRect(16f, 22f, FxStyles.header);
            GUI.Box(rect, title, FxStyles.header);

            var display = group == null || group.isExpanded;

            var foldoutRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            var e = Event.current;

            if (e.type == EventType.Repaint)
                FxStyles.headerFoldout.Draw(foldoutRect, false, false, display, false);

            if (e.type == EventType.MouseDown)
            {
                if (rect.Contains(e.mousePosition) && group != null)
                {
                    display = !display;

                    if (group != null)
                        group.isExpanded = !group.isExpanded;

                    e.Use();
                }
            }

            return display;
        }

        public static bool Header(string title, SerializedProperty group, 
            SerializedProperty enabledField)
        {
            var display = group == null || group.isExpanded;
            var enabled = enabledField.boolValue;

            var rect = GUILayoutUtility.GetRect(16f, 22f, FxStyles.header);
            GUI.Box(rect, title, FxStyles.header);

            var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            var e = Event.current;

            if (e.type == EventType.Repaint)
                FxStyles.headerCheckbox.Draw(toggleRect, false, false, enabled, false);

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

                    // TODO DISABLE PROPERTY

                    e.Use();
                }
                else if (rect.Contains(e.mousePosition) && group != null)
                {
                    display = !display;
                    group.isExpanded = !group.isExpanded;
                    e.Use();
                }
            }

            return display;
        }
    }
}
