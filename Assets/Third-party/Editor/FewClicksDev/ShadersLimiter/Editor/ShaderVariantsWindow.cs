namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;

    public class ShaderVariantsWindow : CustomEditorWindow
    {
        private static readonly Color FILTER_COLOR = new Color(0.2216981f, 0.8124145f, 0.8867924f, 1f);
        private static readonly Vector2 WINDOW_SIZE = new Vector2(1600f, 400f);

        private const float SINGLE_LINE_HEIGHT = 34f;
        private const float INDEX_WIDTH = 30f;
        private const float PASS_WIDTH = 200f;

        private static CompiledShaderVariant currentVariant = null;
        private static Vector2 variantsScrollPosition = Vector2.zero;

        protected override string windowName => "Shader Variants";
        protected override string version => "1.0.0";
        protected override Vector2 minWindowSize => new Vector2(900f, 600f);
        protected override Color mainColor => ShadersLimiter.MAIN_COLOR;

        private string filterString = string.Empty;
        private ShaderToStrip shaderToStrip = null;
        private ShaderVariantCollection collection = null;

        public void Initialize(ShaderToStrip _shader, ShaderVariantCollection _collection)
        {
            minSize = WINDOW_SIZE;
            titleContent = new GUIContent("Shader Variants");

            shaderToStrip = _shader;
            collection = _collection;
        }

        protected override void drawWindowGUI()
        {
            if (shaderToStrip == null)
            {
                return;
            }

            LargeSpace();
            DrawBoldLabel("Shader");

            using (new HorizontalScope())
            {
                GUILayout.Label("Shader Reference", FixedWidth(120f));
                EditorGUILayout.ObjectField(shaderToStrip.ShaderReference, typeof(Shader), false);
            }

            drawWindow();
        }

        private void drawWindow()
        {
            DrawShaderVariants(collection, shaderToStrip.ShaderReference, shaderToStrip.AvailableVariants, windowWidthWithPaddings, ref filterString);
            NormalSpace();
        }

        public static void DrawShaderVariants(ShaderVariantCollection _collection, Shader _shader, List<CompiledShaderVariant> _variants, float _windowWidth, ref string _keywordsFilter)
        {
            SmallSpace();
            DrawLine();
            NormalSpace();

            _keywordsFilter = EditorGUILayout.TextField("Filter", _keywordsFilter);

            NormalSpace();
            DrawBoldLabel("Current Variants");
            SmallSpace();

            _keywordsFilter = _keywordsFilter.ToLower();

            GUIStyle _labelStyle = Styles.CustomizedButton(SINGLE_LINE_HEIGHT, TextAnchor.MiddleCenter, new RectOffset(0, 0, 0, 0));
            GUIStyle _buttonStyle = Styles.CustomizedButton(SINGLE_LINE_HEIGHT, TextAnchor.MiddleLeft, new RectOffset(5, 0, 0, 0));

            float _scrollWidth = _windowWidth - NORMAL_SPACE;
            float _keywordsWidth = _scrollWidth - (INDEX_WIDTH * 2f) - PASS_WIDTH - LARGE_SPACE;

            using (var _scrollScope = new ScrollViewScope(variantsScrollPosition, false, true))
            {
                using (ColorScope.Background(BLUE))
                {
                    using (new HorizontalScope())
                    {
                        GUILayout.Label($"ID", _labelStyle, FixedWidth(INDEX_WIDTH));
                        GUILayout.Label($"Shader Pass", _buttonStyle, FixedWidth(PASS_WIDTH));
                        GUILayout.Label($"No.", _labelStyle, FixedWidth(INDEX_WIDTH));
                        GUILayout.Label($"Keywords", _buttonStyle, FixedWidth(_keywordsWidth));
                    }
                }

                bool _checkForFilter = _keywordsFilter.IsNullEmptyOrWhitespace() == false;
                int _index = 1;

                GUIStyle _buttonStyleWithSmallFont = new GUIStyle(_buttonStyle);
                _buttonStyleWithSmallFont.fontSize = 9;
                _buttonStyleWithSmallFont.richText = true;
                _buttonStyleWithSmallFont.wordWrap = true;

                for (int i = 0; i < _variants.Count; i++)
                {
                    currentVariant = _variants[i];

                    if (_checkForFilter && currentVariant.KeywordsSetAsSingleString.ToLower().Contains(_keywordsFilter) == false)
                    {
                        continue;
                    }

                    string _keywords = _checkForFilter ?
                        currentVariant.KeywordsSetAsSingleString.Replace(_keywordsFilter.ToUpper(), $"<color={FILTER_COLOR.GetHexString()}>{_keywordsFilter.ToUpper()}</color>") :
                        currentVariant.KeywordsSetAsSingleString;

                    using (new HorizontalScope())
                    {
                        float _widthWithDeleteButton = _collection == null ? _keywordsWidth : _keywordsWidth - SINGLE_LINE_HEIGHT - 1f;
                        string _shaderPassLabel = currentVariant.ShaderPass.ToString().InsertSpaceBeforeUpperCaseAndNumeric();

                        GUILayout.Label($"{_index}", _labelStyle, FixedWidth(INDEX_WIDTH));
                        GUILayout.Label($"{_shaderPassLabel}", _buttonStyle, FixedWidth(PASS_WIDTH));
                        GUILayout.Label($"{currentVariant.NumberOfKeywords}", _labelStyle, FixedWidth(INDEX_WIDTH));
                        GUILayout.Label($"{_keywords}", _buttonStyleWithSmallFont, FixedWidth(_widthWithDeleteButton));

                        if (_collection != null && GUILayout.Button(string.Empty, Styles.FixedClose(SINGLE_LINE_HEIGHT), FixedWidthAndHeight(SINGLE_LINE_HEIGHT)))
                        {
                            var _variant = new ShaderVariantCollection.ShaderVariant();
                            _variant.shader = _shader;
                            _variant.passType = currentVariant.ShaderPass;
                            _variant.keywords = currentVariant.KeywordsSetAsSingleString.Split(' ');

                            Undo.RecordObject(_collection, $"Removing variant of {_shader.name} from the collection.");
                            bool _removed = _collection.Remove(_variant);

                            if (_removed)
                            {
                                ShadersLimiter.Log($"Variant of {_shader.GetShaderName()} with keywords {currentVariant.KeywordsSetAsSingleString} was removed from the collection.");
                                _variants.RemoveAt(i);
                                break;
                            }
                        }
                    }

                    _index++;
                }

                variantsScrollPosition = _scrollScope.scrollPosition;
            }
        }

        public static void Show(ShaderToStrip _shader, ShaderVariantCollection _collection = null)
        {
            ShaderVariantsWindow _window = GetWindow<ShaderVariantsWindow>();
            _window.Initialize(_shader, _collection);
            _window.Show();
        }
    }
}
