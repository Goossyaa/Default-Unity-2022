namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using UnityEditor;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;

    [CustomEditor(typeof(KeywordDescription))]
    public class KeywordDescriptionEditor : Editor
    {
        private const float DEFAULT_BUTTON_WIDTH = 0.7f;
        private const float DEFAULT_BUTTON_HEIGHT = 24f;

        private KeywordDescription keywordDescription = null;

        private void OnEnable()
        {
            keywordDescription = target as KeywordDescription;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.DrawScriptProperty();

            SmallSpace();
            KeywordDescriptionWindow.DrawDefaultView(keywordDescription);

            LargeSpace();
            EditorGUILayout.HelpBox("Please use keyword description window to edit this asset", MessageType.Info);
            NormalSpace();

            using (ColorScope.Background(BLUE))
            {
                using (new HorizontalScope())
                {
                    FlexibleSpace();
                    float _buttonWidth = Screen.width * DEFAULT_BUTTON_WIDTH;

                    if (DrawBoxButton("Open description window", FixedWidthAndHeight(_buttonWidth, DEFAULT_BUTTON_HEIGHT)))
                    {
                        KeywordDescriptionWindow.Show(keywordDescription);
                    }

                    FlexibleSpace();
                }
            }
        }
    }
}