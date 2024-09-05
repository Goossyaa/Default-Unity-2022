namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    using static FewClicksDev.Core.EditorDrawer;

    public class ShadersLimiterWindow : CustomEditorWindow
    {
        public enum WindowMode
        {
            Shaders = 0,
            StrippedGlobally = 1,
            Keywords = 2,
            Settings = 3
        }

        public enum KeywordsTable
        {
            Unique = 0,
            EnabledGlobal = 1,
            AllGlobal = 2
        }

        public const float SINGLE_LINE_HEIGHT = 24f;
        public const float BUTTON_HEIGHT = 22f;
        public const float KEYWORD_HEIGHT = 20f;

        private const int MAX_VISIBLE_SHADERS = 24;
        private const int MAX_VISIBLE_KEYWORDS = 30;
        private const int MAX_VISIBLE_KEYWORDS_GLOBAL = 28;

        private const float TOOLBAR_WIDTH = 0.925f;
        private const float KEYWORDS_TOOLBAR_WIDTH = 0.625f;
        private const int KEYWORDS_MARGIN = 20;
        private const float INDEX_WIDTH = 30f;
        private const float SHADER_TOGGLE_WIDTH = 24f;
        private const float SCROLL_WIDTH = 18f;
        private const float COLOR_INDICATOR_WIDTH = 8f;
        private const float COMPILE_SPECIFIED_LABEL_WIDTH = 200f;
        private const float SETTING_LABEL_WIDTH = 210f;

        private const string STRIP_ALL_SHADERS_INFO = "Please choose the way to initialize the database. You can delete the shader cache and start a new build. Be aware that the next build will be totally broken; all shaders will be stripped. Additionaly, you can just attach a shader variant collection.";
        private const string GLOBAL_SHADERS_INFO = "Shaders containing these keywords will be stripped from the build. You can add or remove shaders from this list using the controls below. Make sure that the keywords in this list are not used in the project; shaders will be pink or completely invisible in the build.";
        private const string KEYWORDS_INFO = "This is the list of all unique keywords that have been found during the build time.";
        private const string ENABLED_GLOBAL_KEYWORDS_INFO = "This is the list of all global keywords that are currently enabled in the project.";
        private const string ALL_GLOBAL_KEYWORDS_INFO = "This is the list of all global keywords in the project. Some of them are enabled, some are disabled.";
        private const string NOT_SUPPORTED_INFO = "This list preview is not supported on your Unity version.";

        private static readonly Color SELECT_COLOR = new Color(0.7f, 0.7f, 0.7f, 1f);

        private static readonly GUIContent SHADER_FILTER_CONTENT = new GUIContent("Shader filter", "Can be used to filter out shaders that you are not interested in.");
        private static readonly GUIContent COLLAPSE_ALL_SHADERS_CONTENT = new GUIContent("Collapse all Shaders", "Collapses all shaders in the list (disabling expanded view).");
        private static readonly GUIContent CLEAR_CACHE_CONTENT = new GUIContent("Clear Shaders Cache", "Clears the whole shader cache. The next build will take longer to complete.");
        private static readonly GUIContent STRIP_ALL_KEYWORDS_CONTENT = new GUIContent("Strip all", "Strip all keywords from the shader.");
        private static readonly GUIContent UNSTRIP_ALL_KEYWORDS_CONTENT = new GUIContent("Unstrip all", "Unstrip all keywords from the shader.");
        private const string SHOW_AVAILABLE_VARIANTS = "Show Available Variants (0)";
        private const string TOOLTIP_SHOW_AVAILABLE_VARIANTS = "It shows all available variants of this shader in a separate window.";
        private static readonly GUIContent STRIP_NOT_INCLUDED_KEYWORDS_CONTENT = new GUIContent("Strip not included Keywords", "Strip all keywords that are not included in any of the specified shader variant collections.");

        private static readonly GUIContent TOOLTIP_CLEAR_CACHE_AND_DATABASE_CONTENT = new GUIContent("Clear cache and database", "Clear all shader cache (forcing Unity to recompile all shaders during the next build) and clear the database, removing all setup.");
        private static readonly GUIContent TOOLTIP_STRIP_ALL_SHADERS_IN_THE_NEXT_BUILD_CONTENT = new GUIContent("Strip all shaders in the next build", "It will strip all shaders, causing the next build to be unplayable (all pink). This is useful when you have added tons of new shaders and want to strip them first to avoid a long compilation time.");
        private const string TOOLTIP_RESET_ALL_SETTINGS = "Resets all settings to default values.";
        private const string TOOLTIP_RESET_LOGS = "Resets all logs settings to default values.";
        private const string TOOLTIP_RESET_COLORS = "Resets all color indicators to default values.";
        private const string TOOLTIP_ENABLE_CONDITIONAL_COMPILATION = "Enables conditional compilation in all shaders. Keep in mind that only shaders with attached collections will be handled.";
        private const string TOOLTIP_DISABLE_CONDITIONAL_COMPILATION = "Disables conditional compilation in all shaders; from now on, they will be stripped based on keywords.";
        private const string TOOLTIP_REFRESH_SHADER_VARIANTS = "Refreshes shader variants in the whole database. You should use this button when you modify the shader variant collection previously attached to the shaders.";
        private const string TOOLTIP_UNSTRIP_ALL_KEYWORDS_IN_WHOLE_DATABASE = "Unstrips all keywords (except for globally stripped) in the whole database.";
        private const string TOOLTIP_STRIP_ALL_KEYWORDS_BASED_ON_COLLECTIONS = "Strip all keywords based on attached shader variant collections in the whole database.";

        private const string TOOLTIP_SET_TO_ALL_SHADERS = "Attach a shader variant collection to all shaders in the database.";
        private const string TOOLTIP_REMOVE_FROM_ALL_SHADERS = "Remove a shader variant collection from all shaders in the database.";
        private const string TOOLTIP_CLEAR_ALL_COLLECTIONS = "Clear all shader variant collections attached to shaders in the whole database.";

        protected override string windowName => "Shaders Limiter";
        protected override string version => ShadersLimiter.VERSION;
        protected override Vector2 minWindowSize => new Vector2(575f, 815f);
        protected override Color mainColor => ShadersLimiter.MAIN_COLOR;

        protected override bool hasDocumentation => true;
        protected override string documentationURL => "https://docs.google.com/document/d/1xnpjvoodYMXAGJUrKj5RcNjPOxcl_GRjJ94KOp9rK6U/edit?usp=sharing";

        protected override bool askForReview => true;
        protected override string reviewURL => "https://assetstore.unity.com/packages/tools/utilities/shaders-limiter-270827#reviews";

        private float shaderButtonWidth => windowWidthWithLeftPadding - INDEX_WIDTH - (2 * SHADER_TOGGLE_WIDTH) - COLOR_INDICATOR_WIDTH - (2 * LARGE_SPACE) - 8f;
        private float expandedShaderElementWidth => keywordButtonWidth + COLOR_INDICATOR_WIDTH + 2f + (2f * KEYWORD_HEIGHT);
        private float keywordButtonWidth => windowWidthWithLeftPadding - KEYWORDS_MARGIN - LARGE_SPACE - (KEYWORD_HEIGHT * 2) - 50f;

        private WindowMode windowMode = WindowMode.Shaders;
        private KeywordsTable keywordsTable = KeywordsTable.Unique;
        private string shaderFilter = string.Empty;

        private Vector2 shadersScrollPosition = Vector2.zero;
        private Vector2 keywordsScrollPosition = Vector2.zero;

        private ShadersLimiterDatabase database = null;
        private SerializedObject databaseSerializedObject = null;

        private List<ShaderToStrip> shadersInOrder = new List<ShaderToStrip>();
        private ShaderVariantCollection currentVariantsCollection = null;

        public GUIStyle SingleLineLabelStyle
        {
            get
            {
                if (singleLineLabelStyle == null)
                {
                    singleLineLabelStyle = Styles.CustomizedButton(SINGLE_LINE_HEIGHT, TextAnchor.MiddleCenter, new RectOffset(0, 0, 0, 0));
                }

                return singleLineLabelStyle;
            }
        }

        private GUIStyle singleLineLabelStyle = null;

        public GUIStyle SingleLineButtonStyle
        {
            get
            {
                if (singleLineButtonStyle == null)
                {
                    singleLineButtonStyle = Styles.CustomizedButton(SINGLE_LINE_HEIGHT, TextAnchor.MiddleLeft, new RectOffset(5, 0, 0, 0));
                }

                return singleLineButtonStyle;
            }
        }

        private GUIStyle singleLineButtonStyle = null;

        public GUIStyle BoldKeywordsLabelStyle
        {
            get
            {
                if (boldKeywordsLabelStyle == null)
                {
                    boldKeywordsLabelStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleRight, padding = new RectOffset(0, 5, 0, 0) };
                }

                return boldKeywordsLabelStyle;
            }
        }

        private GUIStyle boldKeywordsLabelStyle = null;

        public GUIStyle KeywordsLabelStyle
        {
            get
            {
                if (keywordsLabelStyle == null)
                {
                    keywordsLabelStyle = Styles.CustomizedButton(KEYWORD_HEIGHT, TextAnchor.MiddleCenter, new RectOffset(0, 0, 0, 0));
                }

                return keywordsLabelStyle;
            }
        }

        private GUIStyle keywordsLabelStyle = null;

#if UNITY_2021_2_OR_NEWER
        private GlobalKeyword[] enabledGlobalKeywords = null;
        private GlobalKeyword[] allGlobalKeywords = null;
#endif

        private string keywordToAdd = string.Empty;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (database == null)
            {
                database = AssetsUtilities.GetScriptableOfType<ShadersLimiterDatabase>();
            }

            regenerateShadersInOrder();
            registerDescriptionsToDatabase();

#if UNITY_2021_2_OR_NEWER
            enabledGlobalKeywords = Shader.enabledGlobalKeywords;
            allGlobalKeywords = Shader.globalKeywords;
#endif

            Undo.undoRedoPerformed -= refreshWindow;
            Undo.undoRedoPerformed += refreshWindow;

            ShaderToStrip.OnExpandStateChanged += refreshHeights;
        }

        private void OnDisable()
        {
            ShaderToStrip.OnExpandStateChanged -= refreshHeights;
        }

        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= refreshWindow;
        }

        public override void AddItemsToMenu(GenericMenu _menu)
        {
            base.AddItemsToMenu(_menu);

            GUIContent _additionalContent = new GUIContent("Open Variants Collection Editor");
            _menu.AddItem(_additionalContent, false, _openVariantsCollectionEditor);

            GUIContent _addVariantsFromCollectionContent = new GUIContent("Add Variants from Collection");
            _menu.AddItem(_addVariantsFromCollectionContent, false, askToAttackCollection);
            _menu.AddSeparator(string.Empty);

            GUIContent _disableCompilationLogsContent = new GUIContent("Disable Compilation Logs on all Shaders");
            _menu.AddItem(_disableCompilationLogsContent, false, _disableCompilationLogsOnAllShaders);

            GUIContent _disableStrippingLogsContent = new GUIContent("Disable Stripping Logs on all Shaders");
            _menu.AddItem(_disableStrippingLogsContent, false, _disableStrippingLogsOnAllShaders);

            void _openVariantsCollectionEditor()
            {
                VariantsCollectionEditorWindow.OpenWindow();
            }

            void _disableCompilationLogsOnAllShaders()
            {
                foreach (var _shader in database.AllShaders)
                {
                    _shader.SetPrintLogsOnCompile(false);
                }
            }

            void _disableStrippingLogsOnAllShaders()
            {
                foreach (var _shader in database.AllShaders)
                {
                    _shader.SetPrintLogsOnStrip(false);
                }
            }
        }

        protected override void drawWindowGUI()
        {
            findDatabaseIfNull();

            NormalSpace();
            windowMode = DrawEnumToolbar(windowMode, TOOLBAR_WIDTH, mainColor);
            SmallSpace();
            DrawLine();
            SmallSpace();

            if (database == null)
            {
                drawNullDatabase();
                return;
            }

            if (databaseSerializedObject == null)
            {
                databaseSerializedObject = new SerializedObject(database);
            }

            databaseSerializedObject.Update();

            switch (windowMode)
            {
                case WindowMode.Shaders:
                    drawShadersTab();
                    break;

                case WindowMode.StrippedGlobally:
                    drawGloballyStrippedKeywords();
                    break;

                case WindowMode.Keywords:
                    drawKeywords();
                    break;

                case WindowMode.Settings:
                    drawSettingsTab();
                    break;
            }

            databaseSerializedObject.ApplyModifiedProperties();
        }

        private void refreshWindow()
        {
            Repaint();
        }

        private void findDatabaseIfNull()
        {
            if (databaseSerializedObject == null && database != null)
            {
                databaseSerializedObject = new SerializedObject(database);
            }

            if (database != null)
            {
                return;
            }

            if (Time.frameCount % 10 == 0)
            {
                database = AssetsUtilities.GetScriptableOfType<ShadersLimiterDatabase>();
            }
        }

        private void drawShadersTab()
        {
            if (database.AllShaders.IsNullOrEmpty())
            {
                drawEmptyDatabase();
                return;
            }

            EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("shouldStripShaders"), new GUIContent("Strip shaders"));

            using (var _changeScope = new ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("displayOrder"), new GUIContent("Display order"));

                if (_changeScope.changed)
                {
                    regenerateShadersInOrder();
                }
            }

            EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("visibility"), new GUIContent("Visibility"));

            if (shadersInOrder == null || shadersInOrder.Count != database.AllShaders.Count)
            {
                regenerateShadersInOrder();
            }

            shaderFilter = EditorGUILayout.TextField(SHADER_FILTER_CONTENT, shaderFilter);
            SmallSpace();

            using (new HorizontalScope())
            {
                if (DrawBoxButton(COLLAPSE_ALL_SHADERS_CONTENT, FixedWidthAndHeight(halfSizeButtonWidth, BUTTON_HEIGHT)))
                {
                    foreach (var _shader in database.AllShaders)
                    {
                        _shader.IsExpanded = false;
                    }
                }

                Space(NORMAL_SPACE - 1f);

                if (DrawBoxButton(CLEAR_CACHE_CONTENT, FixedWidthAndHeight(halfSizeButtonWidth, BUTTON_HEIGHT)))
                {
                    if (EditorUtility.DisplayDialog("Clear Cache", "Clear whole shader cache? Next build will take longer to complete.", "Yes", "No"))
                    {
                        ShadersLimiterDatabase.DeleteBuildCache(true);
                        ShadersLimiter.Log("Deleted all shaders cache. The next build will take longer to complete.");
                    }
                }
            }

            SmallSpace();
            DrawLine();
            SmallSpace();

            float _width = windowWidthWithPaddings - 3f;
            float _height = SINGLE_LINE_HEIGHT * (MAX_VISIBLE_SHADERS + getExtraLines(SINGLE_LINE_HEIGHT));

            using (var _scrollScope = new ScrollViewScope(shadersScrollPosition, false, true, FixedWidthAndHeight(_width, _height)))
            {
                shadersScrollPosition = _scrollScope.scrollPosition;
                drawAllShaders(_height);
            }

            databaseSerializedObject.ApplyModifiedProperties();
        }

        private void drawAllShaders(float _visibleAreaHeight)
        {
            int _index = 0;
            bool _filerIsValid = shaderFilter.IsNullEmptyOrWhitespace() == false;
            string _filterToLower = shaderFilter.ToLower();

            foreach (var _shader in shadersInOrder)
            {
                if (_shader.IsVisible() == false || (_filerIsValid && _shader.ShaderPathToLower.Contains(_filterToLower) == false))
                {
                    continue;
                }

                drawSingleShaderToStrip(_shader, _index, _visibleAreaHeight);
                _index++;
            }
        }

        private void drawNullDatabase()
        {
            EditorGUILayout.HelpBox("Shaders database is null!", MessageType.Warning);
            SmallSpace();

            float _buttonWidth = windowWidth * 0.6f;

            using (new HorizontalScope())
            {
                FlexibleSpace();

                if (DrawBoxButton("Create database file", FixedWidthAndHeight(_buttonWidth, 24f)))
                {
                    string _folderPath = Application.dataPath;
                    string _assetPath = EditorUtility.SaveFolderPanel("Choose folder to create Shader Stripper Database", _folderPath, string.Empty);

                    if (_assetPath.Length > 0)
                    {
                        string _fileName = $"data_ShadersLimiter.asset";
                        string _fixedAssetPath = AssetsUtilities.ConvertAbsolutePathToDataPath(_assetPath, _fileName);
                        _fixedAssetPath = AssetDatabase.GenerateUniqueAssetPath(_fixedAssetPath);

                        ShadersLimiterDatabase _asset = ScriptableObject.CreateInstance<ShadersLimiterDatabase>();
                        AssetDatabase.CreateAsset(_asset, _fixedAssetPath);
                        AssetDatabase.Refresh();

                        EditorUtility.FocusProjectWindow();
                        EditorGUIUtility.PingObject(_asset);

                        OnEnable();
                    }
                }

                FlexibleSpace();
            }
        }

        private void drawEmptyDatabase()
        {
            NormalSpace();

            if (ShadersLimiterDatabase.StrippingEnabledForFirstBuild == false)
            {
                EditorGUILayout.HelpBox(STRIP_ALL_SHADERS_INFO, MessageType.Info);
                SmallSpace();

                using (new HorizontalScope())
                {
                    FlexibleSpace();

                    float _buttonWidth = windowWidth * 0.75f;

                    using (ColorScope.Background(RED))
                    {
                        if (DrawBoxButton("Delete Cache and Strip all shaders in the next Build", FixedWidthAndHeight(_buttonWidth, SINGLE_LINE_HEIGHT)))
                        {
                            ShadersLimiterDatabase.EnableFlagForTheFirstBuild();
                            ShadersLimiterDatabase.DeleteBuildCache(true);

                            ShadersLimiter.Log("Deleted the build cache. Please rebuild the game! Keep in mind that the game won't be playable.");
                        }
                    }

                    FlexibleSpace();
                }

                SmallSpace();
                DrawCenteredBoldLabel("OR");
                SmallSpace();

                using (new HorizontalScope())
                {
                    FlexibleSpace();

                    float _buttonWidth = windowWidth * 0.75f;

                    using (ColorScope.Background(ORANGE))
                    {
                        if (DrawBoxButton("Add Shaders from the Collection", FixedWidthAndHeight(_buttonWidth, SINGLE_LINE_HEIGHT)))
                        {
                            askToAttackCollection();
                        }
                    }

                    FlexibleSpace();
                }
            }
            else
            {
                float _margin = windowWidthWithPaddings / 6f;

                using (new HorizontalScope())
                {
                    Space(_margin);
                    EditorGUILayout.HelpBox("All is set! Now, please build the game.", MessageType.Info);
                    Space(_margin);
                }
            }
        }

        private void drawSingleShaderToStrip(ShaderToStrip _shaderToStrip, int _index, float _visibleAreaHeight)
        {
            if (_shaderToStrip == null)
            {
                return;
            }

            bool _shaderToStripIsBelowVisibleArea = _shaderToStrip.StartPositionAndHeight.x > shadersScrollPosition.y + _visibleAreaHeight + DEFAULT_LINE_HEIGHT;
            bool _shaderToStripIsAboveVisibleArea = _shaderToStrip.StartPositionAndHeight.x + _shaderToStrip.StartPositionAndHeight.y < shadersScrollPosition.y;

            if (_shaderToStripIsBelowVisibleArea || _shaderToStripIsAboveVisibleArea)
            {
                EditorGUILayout.LabelField(string.Empty, FixedHeight(_shaderToStrip.StartPositionAndHeight.y));
                return;
            }

            using (new HorizontalScope())
            {
                GUILayout.Label($"{_index + 1}", SingleLineLabelStyle, FixedWidth(INDEX_WIDTH));

                using (ColorScope.BackgroundAndContent(_shaderToStrip.IndicatorColor))
                {
                    GUILayout.Box(string.Empty, Styles.ClearBox, FixedWidthAndHeight(COLOR_INDICATOR_WIDTH, SINGLE_LINE_HEIGHT));
                }

                string _labelName = _shaderToStrip.Path + getStrippedString(_shaderToStrip.IsStripped, false);

                if (GUILayout.Button(_labelName, SingleLineButtonStyle, FixedWidth(shaderButtonWidth)))
                {
                    _shaderToStrip.IsExpanded = !_shaderToStrip.IsExpanded;

                    if (_shaderToStrip.IsExpanded)
                    {
                        _shaderToStrip.RemoveCollectionsNullReferences();
                    }
                }

                if (_shaderToStrip.AvailableKeywords.Count != 0)
                {
                    Rect _rect = GUILayoutUtility.GetLastRect();
                    GUI.Label(_rect, $"{_shaderToStrip.GetNumberOfStrippedKeywords()}/{_shaderToStrip.AvailableKeywords.Count}", BoldKeywordsLabelStyle);
                }

                bool _stripWholeShader = GUILayout.Toggle(_shaderToStrip.IsStripped, string.Empty, Styles.FixedToggle(SHADER_TOGGLE_WIDTH), FixedWidthAndHeight(SHADER_TOGGLE_WIDTH));

                if (_stripWholeShader != _shaderToStrip.IsStripped)
                {
                    Undo.RecordObject(database, $"Toggled stripping whole shader on {_shaderToStrip.ShortName}.");
                    _shaderToStrip.SetAsStripped(_stripWholeShader);
                }

                bool _useDifferentColor = _shaderToStrip.ForcePrintLogsOnCompile || _shaderToStrip.ForcePrintLogsOnStrip;

                using (ColorScope.Background(_useDifferentColor ? SELECT_COLOR : Color.white))
                {
                    if (GUILayout.Button(string.Empty, Styles.FixedSettings(SHADER_TOGGLE_WIDTH), FixedWidthAndHeight(SHADER_TOGGLE_WIDTH)))
                    {
                        Event _current = Event.current;
                        ShadersLimiterGenericMenu.ShowShaderMenu(database, _current, _shaderToStrip);
                    }
                }
            }

            if (_shaderToStrip.IsExpanded)
            {
                drawExpandedShader(_shaderToStrip);
            }
        }

        private void drawExpandedShader(ShaderToStrip _shaderToStrip)
        {
            float _width = windowWidthWithPaddings - 19f;

            using (new VerticalScope(Styles.LightButton, FixedWidth(_width)))
            {
                SmallSpace();

                using (new HorizontalScope())
                {
                    Space(KEYWORDS_MARGIN);
                    GUILayout.Label("Shader Reference", FixedWidth(120f)); //Label + 1px margin

                    float _objectFieldWidth = expandedShaderElementWidth - 121f;
                    EditorGUILayout.ObjectField(_shaderToStrip.ShaderReference, typeof(Shader), false, FixedWidth(_objectFieldWidth));
                }

                if (_shaderToStrip.AnyKeywordAvailable == false)
                {
                    SmallSpace();
                    return;
                }

                SmallSpace();
                GUILayout.Label("Helpers", EditorStyles.boldLabel.WithLeftMargin(KEYWORDS_MARGIN).WithColor(Color.white));
                SmallSpace();

                using (new HorizontalScope())
                {
                    Space(KEYWORDS_MARGIN);
                    float _buttonWidth = (expandedShaderElementWidth - NORMAL_SPACE) / 2f;

                    if (DrawBoxButton(STRIP_ALL_KEYWORDS_CONTENT, FixedWidthAndHeight(_buttonWidth, BUTTON_HEIGHT)))
                    {
                        _shaderToStrip.SetAllKeywordsAsStripped(true);
                    }

                    NormalSpace();

                    if (DrawBoxButton(UNSTRIP_ALL_KEYWORDS_CONTENT, FixedWidthAndHeight(_buttonWidth, BUTTON_HEIGHT)))
                    {
                        _shaderToStrip.SetAllKeywordsAsStripped(false);
                    }
                }

                drawExpandedShaderConditionalStripping(_shaderToStrip);

                NormalSpace();
                GUILayout.Label("Keywords", EditorStyles.boldLabel.WithLeftMargin(KEYWORDS_MARGIN).WithColor(Color.white));
                SmallSpace();

                if (_shaderToStrip.AvailableKeywords.Count > database.MaxVisibleKeywords)
                {
                    float _scrollWidth = expandedShaderElementWidth + KEYWORDS_MARGIN + SCROLL_WIDTH;
                    float _height = (KEYWORD_HEIGHT * database.MaxVisibleKeywords);

                    using (var _scrollScope = new ScrollViewScope(_shaderToStrip.ScrollPosition, false, true, FixedWidthAndHeight(_scrollWidth, _height)))
                    {
                        drawExpandedShaderKeywords(_shaderToStrip);
                        _shaderToStrip.ScrollPosition = _scrollScope.scrollPosition;
                    }
                }
                else
                {
                    drawExpandedShaderKeywords(_shaderToStrip);
                }

                LargeSpace();
            }
        }

        private void drawExpandedShaderKeywords(ShaderToStrip _shaderToStrip)
        {
            for (int i = 0; i < _shaderToStrip.AvailableKeywords.Count; i++)
            {
                using (new HorizontalScope())
                {
                    Space(KEYWORDS_MARGIN);
                    Keyword _keyword = _shaderToStrip.AvailableKeywords[i];

                    bool _strippedGlobally = _keyword.GloballyStripped;
                    string _keywordLabel = _keyword.KeywordName + getStrippedString(_strippedGlobally, true);

                    using (ColorScope.BackgroundAndContent(_keyword.GetIndicatorColor()))
                    {
                        GUILayout.Box(string.Empty, Styles.ClearBox, FixedWidthAndHeight(COLOR_INDICATOR_WIDTH, KEYWORD_HEIGHT));
                    }

                    if (GUILayout.Button(_keywordLabel, Styles.BoxButton, FixedWidthAndHeight(keywordButtonWidth + 1f, KEYWORD_HEIGHT)))
                    {
                        Undo.RecordObject(database, $"Toggled {_keywordLabel} keyword stripping in {_shaderToStrip.ShortName}.");
                        _keyword.SetAsStripped(!_keyword.IsStripped);
                    }

                    if (_strippedGlobally)
                    {
                        using (new DisabledScope())
                        {
                            GUILayout.Toggle(true, string.Empty, Styles.FixedToggle(KEYWORD_HEIGHT), FixedWidthAndHeight(KEYWORD_HEIGHT));
                        }
                    }
                    else
                    {
                        bool _strippedKeyword = GUILayout.Toggle(_keyword.IsStripped, string.Empty, Styles.FixedToggle(KEYWORD_HEIGHT), FixedWidthAndHeight(KEYWORD_HEIGHT));

                        if (_strippedKeyword != _keyword.IsStripped)
                        {
                            Undo.RecordObject(database, $"Toggled {_keywordLabel} keyword stripping in {_shaderToStrip.ShortName}.");
                            _keyword.SetAsStripped(_strippedKeyword);
                        }
                    }

                    if (GUILayout.Button(string.Empty, Styles.FixedSettings(KEYWORD_HEIGHT), FixedWidthAndHeight(KEYWORD_HEIGHT)))
                    {
                        Event _current = Event.current;
                        ShadersLimiterGenericMenu.ShowKeywordMenu(database, _current, _keyword);
                    }
                }
            }
        }

        private void drawExpandedShaderConditionalStripping(ShaderToStrip _shaderToStrip)
        {
            if (_shaderToStrip.IsStripped || _shaderToStrip.NumberOfKeywords == 0)
            {
                return;
            }

            NormalSpace();
            GUILayout.Label("Conditional Compilation", EditorStyles.boldLabel.WithLeftMargin(KEYWORDS_MARGIN).WithColor(Color.white));
            SmallSpace();

            using (new HorizontalScope())
            {
                Space(KEYWORDS_MARGIN);
                EditorGUILayout.LabelField("Compile only the specified variants", FixedWidth(COMPILE_SPECIFIED_LABEL_WIDTH));

                bool _compileOnlySpecified = EditorGUILayout.Toggle(_shaderToStrip.CompileOnlySpecifiedVariants, FixedWidth(LARGE_SPACE));

                if (_compileOnlySpecified != _shaderToStrip.CompileOnlySpecifiedVariants)
                {
                    _shaderToStrip.SetCompileOnlySpecified(_compileOnlySpecified);
                }
            }

            if (_shaderToStrip.CompileOnlySpecifiedVariants == false)
            {
                return;
            }

            if (_shaderToStrip.VariantsCollections.IsNullOrEmpty())
            {
                using (new HorizontalScope())
                {
                    Space(KEYWORDS_MARGIN);
                    EditorGUILayout.HelpBox("Please use '+' button to specify shader variant collections.", MessageType.Info);
                    LargeSpace();
                }
            }

            for (int i = 0; i < _shaderToStrip.VariantsCollections.Count; i++)
            {
                using (new HorizontalScope())
                {
                    Space(KEYWORDS_MARGIN * 2);

                    string _label = $"Collection #{i + 1}";
                    EditorGUILayout.LabelField(_label, GUILayout.Width(COMPILE_SPECIFIED_LABEL_WIDTH - KEYWORDS_MARGIN));

                    float _objectFieldWidth = expandedShaderElementWidth - COMPILE_SPECIFIED_LABEL_WIDTH - KEYWORD_HEIGHT - 8f;
                    ShaderVariantCollection _collection = EditorGUILayout.ObjectField(_shaderToStrip.VariantsCollections[i], typeof(ShaderVariantCollection), false, FixedWidth(_objectFieldWidth)) as ShaderVariantCollection;

                    if (_collection != _shaderToStrip.VariantsCollections[i])
                    {
                        _shaderToStrip.VariantsCollections[i] = _collection;
                        _shaderToStrip.UpdateVariantsCollection(true);
                    }

                    if (GUILayout.Button("X", FixedWidthAndHeight(25f, KEYWORD_HEIGHT)))
                    {
                        _shaderToStrip.VariantsCollections.RemoveAt(i);
                        _shaderToStrip.UpdateVariantsCollection(true);
                        _shaderToStrip.RecalculateHeight();
                        break;
                    }
                }
            }

            Space(3f);

            using (new HorizontalScope())
            {
                FlexibleSpace();

                using (ColorScope.Background(BLUE))
                {
                    if (GUILayout.Button("+", FixedWidthAndHeight(40f, KEYWORD_HEIGHT)))
                    {
                        _shaderToStrip.VariantsCollections.Add(null);
                        _shaderToStrip.RecalculateHeight();
                    }
                }

                LargeSpace();
            }

            SmallSpace();

            if (_shaderToStrip.AvailableVariants.Count > 0)
            {
                using (new HorizontalScope())
                {
                    Space(KEYWORDS_MARGIN);
                    float _buttonWidth = (expandedShaderElementWidth - NORMAL_SPACE) / 2f;

                    if (DrawBoxButton(new GUIContent(SHOW_AVAILABLE_VARIANTS.Replace("0", _shaderToStrip.AvailableVariants.Count.ToString()), TOOLTIP_SHOW_AVAILABLE_VARIANTS), FixedWidthAndHeight(_buttonWidth, BUTTON_HEIGHT)))
                    {
                        ShaderVariantsWindow.Show(_shaderToStrip);
                    }

                    NormalSpace();

                    if (DrawBoxButton(STRIP_NOT_INCLUDED_KEYWORDS_CONTENT, FixedWidthAndHeight(_buttonWidth, BUTTON_HEIGHT)))
                    {
                        Undo.RecordObject(database, $"Strip not included keywords in {_shaderToStrip.ShortName}.");
                        _shaderToStrip.StripKeywordsNotIncludedInCollection();
                    }
                }
            }
            else if (_shaderToStrip.VariantsCollections.Count > 0)
            {
                using (new HorizontalScope())
                {
                    LargeSpace();
                    EditorGUILayout.HelpBox("There are no variants of this shader in any of the specified collections.", MessageType.Info);
                    LargeSpace();
                }
            }
        }

        private void regenerateShadersInOrder()
        {
            if (database == null || database.AllShaders == null)
            {
                return;
            }

            shadersInOrder.Clear();

            foreach (var _shader in database.AllShaders)
            {
                shadersInOrder.Add(_shader);
            }

            switch (database.DisplayOrder)
            {
                case ShadersDisplayOrder.NumberOfKeywords:
                    shadersInOrder = shadersInOrder.OrderByDescending(x => x.AvailableKeywords.Count).ToList();
                    break;

                case ShadersDisplayOrder.NumberOfStrippedKeywords:
                    shadersInOrder = shadersInOrder.OrderByDescending(x => x.GetNumberOfStrippedKeywords()).ToList();
                    break;
            }

            float _currentY = 0f;

            foreach (var _shader in shadersInOrder)
            {
                _shader.RecalculateHeightAndStartPosition(_currentY);
                _currentY += _shader.StartPositionAndHeight.y + 2f;
            }
        }

        private void refreshHeights(ShaderToStrip _shader)
        {
            int _indexOfShader = shadersInOrder.IndexOf(_shader);

            if (_indexOfShader == -1 || _indexOfShader == shadersInOrder.Count - 1)
            {
                return;
            }

            float _currentY = shadersInOrder[_indexOfShader].StartPositionAndHeight.x;

            for (int i = _indexOfShader; i < shadersInOrder.Count; i++)
            {
                shadersInOrder[i].RecalculateHeightAndStartPosition(_currentY);
                _currentY += shadersInOrder[i].StartPositionAndHeight.y + 2f;
            }
        }

        private void drawGloballyStrippedKeywords()
        {
            SmallSpace();

            using (new HorizontalScope())
            {
                float _space = windowWidth * 0.06f;

                Space(_space);
                EditorGUILayout.HelpBox(GLOBAL_SHADERS_INFO, MessageType.Info);
                Space(_space);
            }

            SmallSpace();

            if (database.GloballyStrippedKeywords.Count > MAX_VISIBLE_KEYWORDS)
            {
                float _height = KEYWORD_HEIGHT * (MAX_VISIBLE_KEYWORDS + getExtraLines(KEYWORD_HEIGHT));

                using (var _scrollScope = new ScrollViewScope(keywordsScrollPosition, false, true, FixedHeight(_height)))
                {
                    _drawList();
                    keywordsScrollPosition = _scrollScope.scrollPosition;
                }
            }
            else
            {
                _drawList();
            }

            NormalSpace();

            using (new HorizontalScope())
            {
                EditorGUILayout.LabelField("Add Keyword", FixedWidth(100f));
                keywordToAdd = EditorGUILayout.TextField(keywordToAdd);

                if (DrawBoxButton("Strip Keyword", FixedWidthAndHeight(120f, KEYWORD_HEIGHT)))
                {
                    if (keywordToAdd.Length >= 3)
                    {
                        Undo.RecordObject(database, $"Added {keywordToAdd} keyword to globally stripped.");
                        ShadersLimiterDatabase.AddToGloballyStripped(keywordToAdd);
                    }
                }
            }

            void _drawList()
            {
                float _keywordWidth = keywordButtonWidth + 23f;

                for (int i = 0; i < database.GloballyStrippedKeywords.Count; i++)
                {
                    string _keyword = database.GloballyStrippedKeywords[i];

                    using (new HorizontalScope())
                    {
                        GUILayout.Label($"{i + 1}", KeywordsLabelStyle, FixedWidthAndHeight(INDEX_WIDTH, KEYWORD_HEIGHT));
                        GUILayout.Label(_keyword, Styles.BoxButton, FixedWidthAndHeight(_keywordWidth, KEYWORD_HEIGHT));

                        if (GUILayout.Button(string.Empty, Styles.FixedSettings(KEYWORD_HEIGHT), FixedWidthAndHeight(KEYWORD_HEIGHT)))
                        {
                            Event _current = Event.current;
                            ShadersLimiterGenericMenu.ShowStringKeywordMenu(database, _current, _keyword);
                        }

                        if (GUILayout.Button(string.Empty, Styles.FixedClose(KEYWORD_HEIGHT), FixedWidthAndHeight(KEYWORD_HEIGHT)))
                        {
                            Undo.RecordObject(database, $"Removed {_keyword} keyword from globally stripped.");
                            database.GloballyStrippedKeywords.RemoveAt(i);
                            database.SetAsDirty();
                            break;
                        }
                    }
                }
            }
        }

        private void drawKeywords()
        {
            keywordsTable = DrawEnumToolbar(keywordsTable, KEYWORDS_TOOLBAR_WIDTH, ShadersLimiter.SUBCATEGORY_TOOLBAR_COLOR, 22f, 12);
            SmallSpace();

            switch (keywordsTable)
            {
                case KeywordsTable.Unique:
                    drawUniqueKeywordsList();
                    break;

#if UNITY_2021_2_OR_NEWER
                case KeywordsTable.EnabledGlobal:
                    drawKeywordsList(enabledGlobalKeywords, ENABLED_GLOBAL_KEYWORDS_INFO);
                    break;

                case KeywordsTable.AllGlobal:
                    drawKeywordsList(allGlobalKeywords, ALL_GLOBAL_KEYWORDS_INFO);
                    break;
#else
                default:
                    EditorGUILayout.HelpBox(NOT_SUPPORTED_INFO, MessageType.Info);
                    break;
#endif
            }
        }

        private void drawUniqueKeywordsList()
        {
            EditorGUILayout.HelpBox(KEYWORDS_INFO, MessageType.Info);
            NormalSpace();

            if (database.UniqueKeywords.Count > MAX_VISIBLE_KEYWORDS)
            {
                float _height = KEYWORD_HEIGHT * (MAX_VISIBLE_KEYWORDS + getExtraLines(KEYWORD_HEIGHT));

                using (var _scrollScope = new ScrollViewScope(keywordsScrollPosition, false, false, FixedHeight(_height)))
                {
                    drawUniqueList();
                    keywordsScrollPosition = _scrollScope.scrollPosition;
                }
            }
            else
            {
                drawUniqueList();
            }
        }

        private void drawUniqueList()
        {
            float _space = (windowWidthWithPaddings - keywordButtonWidth - INDEX_WIDTH - KEYWORD_HEIGHT) / 2f;

            for (int i = 0; i < database.UniqueKeywords.Count; i++)
            {
                string _keyword = database.UniqueKeywords[i];
                bool _strippedGlobally = ShadersLimiterDatabase.IsKeywordGloballyStripped(_keyword);
                string _keywordLabel = _keyword + getStrippedString(_strippedGlobally, true);

                using (new HorizontalScope())
                {
                    Space(_space);

                    GUILayout.Label($"{i + 1}", KeywordsLabelStyle, FixedWidthAndHeight(INDEX_WIDTH, KEYWORD_HEIGHT));
                    GUILayout.Label(_keywordLabel, Styles.BoxButton, FixedWidthAndHeight(keywordButtonWidth, KEYWORD_HEIGHT));

                    if (GUILayout.Button(string.Empty, Styles.FixedSettings(KEYWORD_HEIGHT), FixedWidthAndHeight(KEYWORD_HEIGHT)))
                    {
                        Event _current = Event.current;
                        ShadersLimiterGenericMenu.ShowStringKeywordMenu(database, _current, _keyword, true);
                    }
                }
            }
        }

#if UNITY_2021_2_OR_NEWER
        private void drawKeywordsList(GlobalKeyword[] _keywords, string _info)
        {
            EditorGUILayout.HelpBox(_info, MessageType.Info);
            NormalSpace();

            if (_keywords.Length > MAX_VISIBLE_KEYWORDS_GLOBAL)
            {
                float _height = KEYWORD_HEIGHT * (MAX_VISIBLE_KEYWORDS_GLOBAL + getExtraLines(KEYWORD_HEIGHT));

                using (var _scrollScope = new ScrollViewScope(keywordsScrollPosition, false, false, FixedHeight(_height)))
                {
                    _drawList();
                    keywordsScrollPosition = _scrollScope.scrollPosition;
                }
            }
            else
            {
                _drawList();
            }

            SmallSpace();

            float _buttonWidth = windowWidth * 0.5f;
            using (new HorizontalScope())
            {
                FlexibleSpace();

                using (ColorScope.Background(BLUE))
                {
                    if (DrawBoxButton("Refresh Keywords List", FixedWidthAndHeight(_buttonWidth, SINGLE_LINE_HEIGHT)))
                    {
                        enabledGlobalKeywords = Shader.enabledGlobalKeywords;
                        allGlobalKeywords = Shader.globalKeywords;
                    }
                }

                FlexibleSpace();
            }

            void _drawList()
            {
                float _space = (windowWidthWithPaddings - keywordButtonWidth - INDEX_WIDTH - KEYWORD_HEIGHT) / 2f;

                for (int i = 0; i < _keywords.Length; i++)
                {
                    string _keyword = _keywords[i].name;
                    bool _strippedGlobally = ShadersLimiterDatabase.IsKeywordGloballyStripped(_keyword);
                    string _keywordLabel = _keyword + getStrippedString(_strippedGlobally, true);

                    using (new HorizontalScope())
                    {
                        Space(_space);

                        GUILayout.Label($"{i + 1}", KeywordsLabelStyle, FixedWidthAndHeight(INDEX_WIDTH, KEYWORD_HEIGHT));
                        GUILayout.Label(_keywordLabel, Styles.BoxButton, FixedWidthAndHeight(keywordButtonWidth, KEYWORD_HEIGHT));

                        if (GUILayout.Button(string.Empty, Styles.FixedSettings(KEYWORD_HEIGHT), FixedWidthAndHeight(KEYWORD_HEIGHT)))
                        {
                            Event _current = Event.current;
                            ShadersLimiterGenericMenu.ShowStringKeywordMenu(database, _current, _keyword, true);
                        }
                    }
                }
            }
        }
#endif

        private void drawSettingsTab()
        {
            float _labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = SETTING_LABEL_WIDTH;

            using (var _changeScope = new ChangeCheckScope())
            {
                DrawBoldLabel("Main settings");
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("shouldStripShaders"), new GUIContent("Strip shaders"));
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("maxVisibleKeywords"), new GUIContent("Max visible keywords"));
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("deleteComputeShadersCache"), new GUIContent("Delete compute shaders cache"));

                NormalSpace();
                DrawBoldLabel("Logs");
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("printUsageLogs"), new GUIContent("Print usage logs"));
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("printCompiledShadersLogs"), new GUIContent("Print compiled shaders"));
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("printStrippedShaderLogs"), new GUIContent("Print stripped shaders"));

                NormalSpace();
                DrawBoldLabel("Color indicators");
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("strippedShaderColor"), new GUIContent("Stripped"));
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("notStrippedShaderColor"), new GUIContent("Not stripped"));
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("someKeywordsStrippedColor"), new GUIContent("Some keywords stripped"));
                EditorGUILayout.PropertyField(databaseSerializedObject.FindProperty("compileOnlySpecifiedColor"), new GUIContent("Only the specified allowed"));

                LargeSpace();
                DrawBoldLabel("Helpers");

                using (new HorizontalScope())
                {
                    if (DrawBoxButton(TOOLTIP_CLEAR_CACHE_AND_DATABASE_CONTENT, FixedWidthAndHeight(halfSizeButtonWidth, BUTTON_HEIGHT)))
                    {
                        if (EditorUtility.DisplayDialog("Clear Cache and Database", "Clear whole shader cache and database? Next build will take longer to complete and you will have to setup stripping from zero. Can't undo this action!", "Yes", "No"))
                        {
                            ShadersLimiterDatabase.DeleteBuildCache(true);
                            database.ClearDatabase();

                            ShadersLimiter.Log("Cleared whole database and Shader Cache. Please build your game to start using this tool again.");
                        }
                    }

                    Space(NORMAL_SPACE - 1f);

                    if (DrawBoxButton(TOOLTIP_STRIP_ALL_SHADERS_IN_THE_NEXT_BUILD_CONTENT, FixedWidthAndHeight(halfSizeButtonWidth, BUTTON_HEIGHT)))
                    {
                        if (EditorUtility.DisplayDialog("Strip all shaders", "Clear cache and strip all shaders in the next build?", "Yes", "No"))
                        {
                            ShadersLimiterDatabase.DeleteBuildCache(true);
                            ShadersLimiterDatabase.EnableFlagForTheFirstBuild();

                            ShadersLimiter.Log("Cleared Shader Cache and set flag to strip all shaders in the next build.");
                        }
                    }
                }

                SmallSpace();

                using (new HorizontalScope())
                {
                    if (DrawBoxButton(new GUIContent("Reset all settings", TOOLTIP_RESET_ALL_SETTINGS), FixedWidthAndHeight(thirdSizeButtonWidth, BUTTON_HEIGHT)))
                    {
                        database.ResetAllSettingsToDefault();
                    }

                    NormalSpace();

                    if (DrawBoxButton(new GUIContent("Reset logs", TOOLTIP_RESET_LOGS), FixedWidthAndHeight(thirdSizeButtonWidth, BUTTON_HEIGHT)))
                    {
                        database.ResetLogsSettings();
                    }

                    NormalSpace();

                    if (DrawBoxButton(new GUIContent("Reset colors", TOOLTIP_RESET_COLORS), FixedWidthAndHeight(thirdSizeButtonWidth, BUTTON_HEIGHT)))
                    {
                        database.ResetColorsToDefault();
                    }
                }

                LargeSpace();
                DrawBoldLabel("Variant Collections Helpers");
                SmallSpace();

                using (new HorizontalScope())
                {
                    if (DrawBoxButton(new GUIContent("Enable conditional compilation", TOOLTIP_ENABLE_CONDITIONAL_COMPILATION), FixedWidthAndHeight(halfSizeButtonWidth, BUTTON_HEIGHT)))
                    {
                        database.EnableConditionalCompilation();
                    }

                    NormalSpace();

                    if (DrawBoxButton(new GUIContent("Disable conditional compilation", TOOLTIP_DISABLE_CONDITIONAL_COMPILATION), FixedWidthAndHeight(halfSizeButtonWidth, BUTTON_HEIGHT)))
                    {
                        database.DisableConditionalCompilation();
                    }
                }

                SmallSpace();

                if (DrawBoxButton(new GUIContent("Refresh Shader Variants in the whole database", TOOLTIP_REFRESH_SHADER_VARIANTS), FixedWidthAndHeight(wholeSizeButtonWidth, BUTTON_HEIGHT)))
                {
                    ShadersLimiterDatabase.RefreshShaderVariants();
                }

                SmallSpace();

                using (new HorizontalScope())
                {
                    if (DrawBoxButton(new GUIContent("Unstrip all keywords", TOOLTIP_UNSTRIP_ALL_KEYWORDS_IN_WHOLE_DATABASE), FixedWidthAndHeight(halfSizeButtonWidth, BUTTON_HEIGHT)))
                    {
                        database.UnstripAllKeywordsInShaders();
                    }

                    NormalSpace();

                    if (DrawBoxButton(new GUIContent("Strip keywords based on collections", TOOLTIP_STRIP_ALL_KEYWORDS_BASED_ON_COLLECTIONS), FixedWidthAndHeight(halfSizeButtonWidth, BUTTON_HEIGHT)))
                    {
                        database.StripKeywordsBasedOnCollections();
                    }
                }

                LargeSpace();
                currentVariantsCollection = EditorGUILayout.ObjectField("Collection", currentVariantsCollection, typeof(ShaderVariantCollection), false) as ShaderVariantCollection;

                if (currentVariantsCollection != null && AssetDatabase.Contains(currentVariantsCollection) == false) //To make sure that we have an asset, not some serialized object
                {
                    currentVariantsCollection = null;
                }

                if (currentVariantsCollection != null)
                {
                    SmallSpace();

                    using (new HorizontalScope())
                    {
                        if (DrawBoxButton(new GUIContent("Set to all shaders", TOOLTIP_SET_TO_ALL_SHADERS), FixedWidthAndHeight(thirdSizeButtonWidth, BUTTON_HEIGHT)))
                        {
                            database.AddCollectionToAllShaders(currentVariantsCollection);
                        }

                        NormalSpace();

                        if (DrawBoxButton(new GUIContent("Remove from all shaders", TOOLTIP_REMOVE_FROM_ALL_SHADERS), FixedWidthAndHeight(thirdSizeButtonWidth, BUTTON_HEIGHT)))
                        {
                            database.RemoveCollectionFromAllShaders(currentVariantsCollection);
                        }

                        NormalSpace();

                        if (DrawBoxButton(new GUIContent("Clear all collections", TOOLTIP_CLEAR_ALL_COLLECTIONS), FixedWidthAndHeight(thirdSizeButtonWidth, BUTTON_HEIGHT)))
                        {
                            database.ClearAllCollections();
                        }
                    }
                }

                if (_changeScope.changed)
                {
                    database.SetAsDirty();
                }
            }

            EditorGUIUtility.labelWidth = _labelWidth;
        }

        private void registerDescriptionsToDatabase()
        {
            var _allDescriptions = AssetsUtilities.GetAssetsOfType<KeywordDescription>(string.Empty);

            foreach (var _description in _allDescriptions)
            {
                _description.RegisterToDatabase();
            }
        }

        private string getStrippedString(bool _condition, bool _globally)
        {
            if (_condition == false)
            {
                return string.Empty;
            }

            string _string = _globally ? "Stripped Globally" : "Stripped";

            return $"  <b><i>({_string})</i></b>";
        }

        private int getExtraLines(float _singleLineHeight)
        {
            float _heightDifference = windowHeight - minWindowSize.y;
            return Mathf.FloorToInt(_heightDifference / _singleLineHeight);
        }

        private void askToAttackCollection()
        {
            string _assetPath = EditorUtility.OpenFilePanel("Choose Shader Variant Collection", Application.dataPath, "shadervariants");
            string _finalPath = AssetsUtilities.ConvertAbsolutePathToDataPath(_assetPath);
            ShaderVariantCollection _foundCollection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(_finalPath);

            if (_foundCollection != null)
            {
                bool _decision = EditorUtility.DisplayDialog("Add Variants", "Do you wish to attach selected shader variant collection to the newly added shaders and enable conditional compilation?", "Yes", "No");

                addVariantsFromTheCollectionToTheDatabase(_foundCollection, _decision);
            }
            else
            {
                ShadersLimiter.Error($"The Shader Variant Collection was not found at the given path: {_finalPath}.");
            }
        }

        private void addVariantsFromTheCollectionToTheDatabase(ShaderVariantCollection _collection, bool _attach)
        {
            var _foundVariants = ShadersLimiter.GetShaderVariantsInCollection(_collection, null);

            foreach (var _variant in _foundVariants)
            {
                if (_variant.ShaderReference == null)
                {
                    continue;
                }

                ShaderKeyword[] _keywords = new ShaderKeyword[_variant.KeywordsSet.Length];

                for (int i = 0; i < _keywords.Length; i++)
                {
                    _keywords[i] = new ShaderKeyword(_variant.KeywordsSet[i]);
                }

                bool _addedNew = ShadersLimiterDatabase.AddShaderToTheDatabase(_variant.ShaderReference, _keywords);

                if (_attach)
                {
                    var _shaderToStrip = ShadersLimiterDatabase.GetShaderToStrip(_variant.ShaderReference);
                    _shaderToStrip.AddCollection(_collection, false);
                }

                if (_addedNew)
                {
                    ShadersLimiter.Log($"Shader {_variant.ShaderReference.name} was added to the database!");
                    ShadersLimiterDatabase.SetAsDirty();
                }
            }

            ShadersLimiterDatabase.RefreshShaderVariants();
            OnEnable();
        }

        [MenuItem("Window/FewClicks Dev/Shaders Limiter")]
        public static void OpenWindow()
        {
            GetWindow<ShadersLimiterWindow>().Show();
        }
    }
}

