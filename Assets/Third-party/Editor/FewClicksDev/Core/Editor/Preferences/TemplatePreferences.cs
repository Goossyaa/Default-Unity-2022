namespace FewClicksDev.Core
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class TemplatePreferences
    {
        private const string PREFS_PREFIX = "FewClicksDev.ToolName.";
        private const string PREFS_PATH = "Few Clicks Dev/Tool Name";
        private const SettingsScope SETTINGS_SCOPE = SettingsScope.User;

        private const string LABEL = "Tool Name";
        private static readonly string[] KEYWORDS = new string[] { "Few Clicks Dev", LABEL, "Some Options" };
        private const float LABEL_WIDTH = 250f;

        private const string TEMPLATE_INT_LABEL = "Template integer";
        private const string TEMPLATE_FLOAT_LABEL = "Template float";
        private const string TEMPLATE_BOOL_LABEL = "Template bool";
        private const string TEMPLATE_STRING_LABEL = "Template string";
        private const string TEMPLATE_COLOR_LABEL = "Template color";

        private const int DEFAULT_INT = 0;
        private const float DEFAULT_FLOAT = 1f;
        private const bool DEFAULT_BOOL = false;
        private const string DEFAULT_STRING = "string";
        private static readonly Color DEFAULT_COLOR = new Color(0.5f, 0.5f, 0.5f, 1f);

        public static int TemplateInt = DEFAULT_INT;
        public static float TemplateFloat = DEFAULT_FLOAT;
        public static bool TemplateBool = DEFAULT_BOOL;
        public static string TemplateString = DEFAULT_STRING;
        public static Color TemplateColor = DEFAULT_COLOR;

        private static bool arePrefsLoaded = false;

        static TemplatePreferences()
        {
            loadPreferences();
        }

        //[SettingsProvider] //Uncomment to make it work
        public static SettingsProvider PreferencesSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(PREFS_PATH, SETTINGS_SCOPE)
            {
                label = LABEL,
                guiHandler = (searchContext) =>
                {
                    OnGUI();
                },

                keywords = new HashSet<string>(KEYWORDS)
            };

            return provider;
        }

        public static void OnGUI()
        {
            float _labelWith = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = LABEL_WIDTH;

            if (arePrefsLoaded == false)
            {
                loadPreferences();
            }

            EditorDrawer.DrawHeader("Settings");
            TemplateInt = EditorGUILayout.IntField(TEMPLATE_INT_LABEL, TemplateInt);
            TemplateFloat = EditorGUILayout.FloatField(TEMPLATE_FLOAT_LABEL, TemplateFloat);
            TemplateBool = EditorGUILayout.Toggle(TEMPLATE_BOOL_LABEL, TemplateBool);
            TemplateString = EditorGUILayout.TextField(TEMPLATE_BOOL_LABEL, TemplateString);
            TemplateColor = EditorGUILayout.ColorField(TEMPLATE_COLOR_LABEL, TemplateColor);

            EditorDrawer.LargeSpace();

            if (GUILayout.Button("Reset to defaults"))
            {
                resetToDefaults();
            }

            if (GUI.changed == true)
            {
                savePreferences();
            }

            EditorGUIUtility.labelWidth = _labelWith;
        }

        private static void loadPreferences()
        {
            TemplateInt = EditorPrefs.GetInt(PREFS_PREFIX + TEMPLATE_INT_LABEL, DEFAULT_INT);
            TemplateFloat = EditorPrefs.GetFloat(PREFS_PREFIX + TEMPLATE_FLOAT_LABEL, DEFAULT_FLOAT);
            TemplateBool = EditorPrefs.GetBool(PREFS_PREFIX + TEMPLATE_BOOL_LABEL, DEFAULT_BOOL);
            TemplateString = EditorPrefs.GetString(PREFS_PREFIX + TEMPLATE_STRING_LABEL, DEFAULT_STRING);
            TemplateColor = PreferencesExtensions.LoadColor(PREFS_PREFIX + TEMPLATE_COLOR_LABEL, DEFAULT_COLOR);

            arePrefsLoaded = true;
        }

        private static void savePreferences()
        {
            EditorPrefs.SetInt(PREFS_PREFIX + TEMPLATE_INT_LABEL, TemplateInt);
            EditorPrefs.SetFloat(PREFS_PREFIX + TEMPLATE_FLOAT_LABEL, TemplateFloat);
            EditorPrefs.SetBool(PREFS_PREFIX + TEMPLATE_BOOL_LABEL, TemplateBool);
            EditorPrefs.SetString(PREFS_PREFIX + TEMPLATE_STRING_LABEL, TemplateString);
            EditorPrefs.SetString(PREFS_PREFIX + TEMPLATE_COLOR_LABEL, PreferencesExtensions.GetStringFromColor(TemplateColor));
        }

        private static void resetToDefaults()
        {
            TemplateInt = DEFAULT_INT;
            TemplateFloat = DEFAULT_FLOAT;
            TemplateBool = DEFAULT_BOOL;
            TemplateString = DEFAULT_STRING;
            TemplateColor = DEFAULT_COLOR;

            savePreferences();
        }
    }
}