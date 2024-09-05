namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;
    using static FewClicksDev.ShadersLimiter.VariantsCollectionEditorWindow;

    public class NewVariantsWindow : CustomEditorWindow
    {
        private const float SINGLE_LINE_HEIGHT = 22f;
        private const float INDEX_WIDTH = 30f;
        private const float COUNT_WIDTH = 50f;
        private const float CLEAR_CACHE_BUTTON_WIDTH = 100f;

        protected override string windowName => "New Shader Variants";
        protected override string version => ShadersLimiter.VERSION;
        protected override Vector2 minWindowSize => new Vector2(520f, 614f);
        protected override Color mainColor => ShadersLimiter.MAIN_COLOR;

        private List<NewVariant> newVariants = new List<NewVariant>();
        private Vector2 shadersScrollPosition = Vector2.zero;

        protected override void drawWindowGUI()
        {
            LargeSpace();
            EditorGUILayout.HelpBox("This is the list of shaders with new variants found, you can choose to delete shaders cache for all of them or one by one, forcing Unity to recompile them during the next build.", MessageType.Info);
            NormalSpace();

            using (new HorizontalScope())
            {
                if (DrawBoxButton(new GUIContent("Delete all shaders cache"), FixedWidthAndHeight(halfSizeButtonWidth, SINGLE_LINE_HEIGHT)))
                {
                    if (EditorUtility.DisplayDialog("Clear Cache", "Clear whole shader cache? Next build will take longer to complete.", "Yes", "No"))
                    {
                        ShadersLimiterDatabase.DeleteBuildCache(true);
                        ShadersLimiter.Log("Deleted all shaders cache. The next build will take longer to complete.");
                    }
                }

                Space(NORMAL_SPACE - 1f);

                if (DrawBoxButton(new GUIContent("Delete cache of shaders below"), FixedWidthAndHeight(halfSizeButtonWidth, SINGLE_LINE_HEIGHT)))
                {
                    foreach (var _variant in newVariants)
                    {
                        if (_variant == null || _variant.ShaderReference == null)
                        {
                            continue;
                        }

                        _variant.ShaderReference.DeleteCache();
                    }
                }
            }

            NormalSpace();
            GUIStyle _labelStyle = Styles.CustomizedButton(SINGLE_LINE_HEIGHT, TextAnchor.MiddleCenter, new RectOffset(0, 0, 0, 0));

            using (var _scrollScope = new ScrollViewScope(shadersScrollPosition))
            {
                using (new HorizontalScope())
                {
                    GUILayout.Label("ID", _labelStyle, FixedWidth(INDEX_WIDTH));
                    GUILayout.Label("Shader", _labelStyle);
                    GUILayout.Label("Count", _labelStyle, FixedWidth(COUNT_WIDTH));
                    GUILayout.Label(string.Empty, _labelStyle, FixedWidthAndHeight(CLEAR_CACHE_BUTTON_WIDTH, SINGLE_LINE_HEIGHT));
                    Space(4f);
                }

                int _index = 0;

                foreach (var _variant in newVariants)
                {
                    if (_variant == null || _variant.ShaderReference == null)
                    {
                        continue;
                    }

                    using (new HorizontalScope())
                    {
                        GUILayout.Label($"{_index + 1}", _labelStyle, FixedWidth(INDEX_WIDTH));

                        using (new HorizontalScope(Styles.BoxButton, FixedHeight(SINGLE_LINE_HEIGHT)))
                        {
                            Space(4f);
                            EditorGUILayout.ObjectField(_variant.ShaderReference, typeof(Shader), false);
                        }

                        GUILayout.Label(_variant.Count.ToString(), _labelStyle, FixedWidth(COUNT_WIDTH));

                        if (GUILayout.Button("Delete cache", _labelStyle, FixedWidthAndHeight(CLEAR_CACHE_BUTTON_WIDTH, SINGLE_LINE_HEIGHT)))
                        {
                            _variant.ShaderReference.DeleteCache();
                        }

                        Space(4f);
                    }

                    _index++;
                }

                shadersScrollPosition = _scrollScope.scrollPosition;
            }
        }

        public static void OpenWindow(List<NewVariant> _newVariants)
        {
            var _window = GetWindow<NewVariantsWindow>();
            _window.Show();
            _window.newVariants = _newVariants;
        }
    }
}
