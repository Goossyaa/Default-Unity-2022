namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using UnityEditor;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;

    public class KeywordDescriptionWindow : CustomEditorWindow
    {
        private static readonly Color LABELS_COLOR = new Color(0.5206479f, 0.9433962f, 0.9176638f, 1f);
        private static readonly Vector2 WINDOW_SIZE = new Vector2(450f, 400f);

        private const float SINGLE_LINE_HEIGHT = 24f;
        private const float SEPARATOR_WIDTH = 0.7f;
        private const float LABELS_WIDTH = 0.9f;

        protected override string windowName => "Keyword Description";
        protected override string version => "1.0.0";
        protected override Vector2 minWindowSize => WINDOW_SIZE;
        protected override Color mainColor => ShadersLimiter.MAIN_COLOR;
        protected override float leftPadding => 0f;
        protected override float rightPadding => 0f;

        private KeywordDescription description = null;
        private SerializedObject descriptionObject = null;
        private bool isInEditMode = false;

        public void Initialize(KeywordDescription _description)
        {
            minSize = minWindowSize;
            maxSize = minWindowSize;
            titleContent = new GUIContent("Keyword Description");

            description = _description;
            descriptionObject = new SerializedObject(description);
        }

        protected override void drawWindowGUI()
        {
            if (isInEditMode == false)
            {
                DrawDefaultView(description);
            }
            else
            {
                drawEditMode();
            }

            NormalSpace();

            using (new HorizontalScope())
            {
                float _buttonWidth = (minWindowSize.x / 2f) - (LARGE_SPACE / 2f);
                FlexibleSpace();

                string _editModeText = isInEditMode ? "Save" : "Edit";

                if (DrawBoxButton(_editModeText, FixedWidthAndHeight(_buttonWidth, SINGLE_LINE_HEIGHT)))
                {
                    if (isInEditMode)
                    {
                        description.SetDirtyAndSave();
                    }

                    isInEditMode = !isInEditMode;
                }

                if (DrawBoxButton("Close", FixedWidthAndHeight(_buttonWidth, SINGLE_LINE_HEIGHT)))
                {
                    Close();
                }

                FlexibleSpace();
            }

            SmallSpace();
        }

        

        private void drawEditMode()
        {
            if (descriptionObject == null)
            {
                descriptionObject = new SerializedObject(description);
                return;
            }

            EditorGUILayout.PropertyField(descriptionObject.FindProperty("keywordName"));
            EditorGUILayout.PropertyField(descriptionObject.FindProperty("description"));

            SerializedProperty _showAdditionalMessageProperty = descriptionObject.FindProperty("showAdditionalMessage");
            EditorGUILayout.PropertyField(_showAdditionalMessageProperty);

            if (_showAdditionalMessageProperty.boolValue)
            {
                EditorGUILayout.PropertyField(descriptionObject.FindProperty("additionalMessageType"));
                EditorGUILayout.PropertyField(descriptionObject.FindProperty("additionalMessage"));
            }

            EditorGUILayout.PropertyField(descriptionObject.FindProperty("documentationLinks"), true);
            descriptionObject.ApplyModifiedProperties();
        }

        public static void DrawDefaultView(KeywordDescription _description)
        {
            SmallSpace();
            DrawCenteredBoldLabel(_description.KeywordName, Screen.width * LABELS_WIDTH, LABELS_COLOR);
            DrawLine(1f, Screen.width * SEPARATOR_WIDTH);
            SmallSpace();

            GUIStyle _style = new GUIStyle(EditorStyles.wordWrappedLabel);
            _style.richText = true;
            _style.alignment = TextAnchor.MiddleCenter;
            _style.normal.textColor = Color.white;

            using (new HorizontalScope())
            {
                FlexibleSpace();
                EditorGUILayout.LabelField(_description.Description, _style, FixedWidth(Screen.width * LABELS_WIDTH));
                FlexibleSpace();
            }

            if (_description.ShowAdditionalMessage)
            {
                LargeSpace();

                using (new HorizontalScope())
                {
                    LargeSpace();
                    EditorGUILayout.HelpBox(_description.AdditionalMessage, _description.AdditionalMessageType);
                    LargeSpace();
                }
            }

            if (_description.DocumentationLinks.IsNullOrEmpty())
            {
                return;
            }

            LargeSpace();
            DrawCenteredBoldLabel("Documentation", Screen.width * LABELS_WIDTH, LABELS_COLOR);
            DrawLine(1f, Screen.width * SEPARATOR_WIDTH);
            SmallSpace();

            using (new HorizontalScope())
            {
                FlexibleSpace();

                using (new VerticalScope())
                {
                    foreach (KeywordDescription.LinkWithTitle _link in _description.DocumentationLinks)
                    {
                        if (DrawBoxButton(_link.Title, FixedWidthAndHeight(Screen.width * 0.85f, SINGLE_LINE_HEIGHT)))
                        {
                            Application.OpenURL(_link.DocumentationLink);
                        }
                    }
                }

                FlexibleSpace();
            }
        }

        public static void Show(KeywordDescription _description)
        {
            Vector2 _screenPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
            _screenPosition.x += Random.Range(0f, 200f);
            _screenPosition.y += Random.Range(0f, 100f);

            KeywordDescriptionWindow _window = CreateInstance<KeywordDescriptionWindow>();
            _window.Initialize(_description);
            _window.ShowUtility();

            _window.position = new Rect(_screenPosition.x, _screenPosition.y, WINDOW_SIZE.x, WINDOW_SIZE.y);
        }
    }
}