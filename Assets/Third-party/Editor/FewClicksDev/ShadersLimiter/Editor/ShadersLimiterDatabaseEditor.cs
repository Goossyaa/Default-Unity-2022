namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using UnityEditor;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;

    [CustomEditor(typeof(ShadersLimiterDatabase))]
    public class ShadersLimiterDatabaseEditor : Editor
    {
        private const float BUTTON_HEIGHT = 22f;

        public override void OnInspectorGUI()
        {
            serializedObject.DrawScriptProperty();

            NormalSpace();

            EditorGUILayout.HelpBox("All variables should be set using Shader Stripper Window", MessageType.Info);

            NormalSpace();

            float _buttonWidth = Screen.width * 0.75f;

            using (new HorizontalScope())
            {
                FlexibleSpace();

                if (DrawBoxButton("Open Window", FixedWidthAndHeight(_buttonWidth, BUTTON_HEIGHT)))
                {
                    ShadersLimiterWindow.OpenWindow();
                }

                FlexibleSpace();
            }
        }
    }
}