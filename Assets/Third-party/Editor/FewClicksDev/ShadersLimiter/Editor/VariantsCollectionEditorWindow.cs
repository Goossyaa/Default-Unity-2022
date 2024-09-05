namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    using static FewClicksDev.Core.EditorDrawer;

    public class VariantsCollectionEditorWindow : CustomEditorWindow
    {
        public enum WindowMode
        {
            Preview = 0,
            Merge = 1,
            InsertVariants = 2,
            Passes = 3
        }

        public enum PreviewMode
        {
            Variants = 0,
            Shaders = 1
        }

        public enum InsertVariantsMode
        {
            LogFile = 0,
            TextFile = 1,
            ExternalFile = 2,
            PastedString = 3,
            EditorEncounteredVariants = 4
        }

        [System.Serializable]
        public class NewVariant
        {
            [SerializeField] private Shader shaderReference = null;
            [SerializeField] private int count = 0;
            [SerializeField] private List<string> allKeywords = new List<string>();

            public Shader ShaderReference => shaderReference;
            public int Count => count;
            public List<string> AllKeywords => allKeywords;

            public NewVariant(Shader _shaderReference)
            {
                shaderReference = _shaderReference;
                count = 1;
            }

            public void IncrementCount(string[] _keywords)
            {
                count++;

                foreach (var _keyword in _keywords)
                {
                    allKeywords.AddUnique(_keyword);
                }
            }
        }

        private static readonly System.Array PASSES = System.Enum.GetValues(typeof(PassType));

        private const float TOOLBAR_WIDTH = 0.84f;
        private const float SUBCATEGORY_TOOLBAR_WIDTH = 0.65f;
        private const float BUTTON_HEIGHT = 24f;
        private const float SINGLE_LINE_HEIGHT = 28f;
        private const float SHADERS_LINE_HEIGHT = 22f;
        private const float INDEX_WIDTH = 45f;
        private const float WIDE_LABEL_WIDTH = 240f;

        private const string MISSING_SHADER_LINE = "Shader ";
        private const string COMPILED_SHADER_LINE = "Compiled shader: ";
        private const string STAGE_VERTEX = "stage vertex: variant";
        private const string STAGE_PIXEL = "stage pixel: variant";
        private const string KEYWORDS = "keywords";
        private const string NO_KEYWORDS = "<no keywords>";
        private const string NOT_FOUND = "not found.";
        private const string REAL_SHADER = "real shader";
        private const string LOG_EXTENSION = ".log";
        private const string TEXT_EXTENSION = ".txt";

        private const string ENCOUNTERED_SHADERS_COUNT_METHOD_NAME = "GetCurrentShaderVariantCollectionShaderCount";
        private const string ENCOUNTERED_VARIANTS_COUNT_METHOD_NAME = "GetCurrentShaderVariantCollectionVariantCount";
        private const string SAVE_CURRENT_VARIANT_COLLECTION_METHOD_NAME = "SaveCurrentShaderVariantCollection";
        private const string VARIANT_COLLECTION_DEFAULT_PATH = "Assets/ShaderVariantCollection.shadervariants";

        protected override string windowName => "Variants Collection Editor";
        protected override string version => ShadersLimiter.VERSION;
        protected override Vector2 minWindowSize => new Vector2(980f, 600f);
        protected override Color mainColor => ShadersLimiter.MAIN_COLOR;

        private WindowMode windowMode = WindowMode.Preview;
        private PreviewMode previewMode = PreviewMode.Variants;
        private InsertVariantsMode insertVariantsMode = InsertVariantsMode.ExternalFile;

        private ShaderVariantCollection mainCollection = null;
        private ShaderVariantCollection additionalCollection = null;
        private Shader currentShader = null;
        private List<CompiledShaderVariant> currentVariants = null;
        private List<Shader> foundShaders = new List<Shader>();

        private bool updateDatabaseAfterAnyModification = true;
        private DefaultAsset gameLogFile = null;
        private TextAsset gameTextFile = null;
        private string pastedLogString = string.Empty;
        private string externalLogFilePath = string.Empty;

        private string keywordsFilter = string.Empty;
        private string keywords = string.Empty;
        private PassType currentPass = PassType.ScriptableRenderPipeline;

        private string shadersFilter = string.Empty;
        private Vector2 shadersScrollPosition = Vector2.zero;

        private List<NewVariant> newVariants = new List<NewVariant>();

        private float passWidth => Screen.width - INDEX_WIDTH - (LARGE_SPACE * 3f);

        protected override void OnEnable()
        {
            base.OnEnable();
            Undo.undoRedoPerformed += refreshWindow;
        }

        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= refreshWindow;
        }

        public override void AddItemsToMenu(GenericMenu _menu)
        {
            base.AddItemsToMenu(_menu);

            GUIContent _additionalContent = new GUIContent("Open Shaders Limiter");
            _menu.AddItem(_additionalContent, false, _openShadersLimiter);

            void _openShadersLimiter()
            {
                ShadersLimiterWindow.OpenWindow();
            }
        }

        protected override void drawWindowGUI()
        {
            NormalSpace();
            windowMode = DrawEnumToolbar(windowMode, TOOLBAR_WIDTH, mainColor);
            SmallSpace();
            DrawLine();
            SmallSpace();

            switch (windowMode)
            {
                case WindowMode.Preview:
                    drawPreviewTab();
                    break;

                case WindowMode.Merge:
                    drawMergeTab();
                    break;

                case WindowMode.InsertVariants:
                    drawInsertVariantsTab();
                    break;

                case WindowMode.Passes:
                    drawPassesTab();
                    break;
            }
        }

        public void OpenWithCollection(ShaderVariantCollection _collection)
        {
            mainCollection = _collection;
            findShadersInTheCollection();
        }

        private void refreshWindow()
        {
            recalculateCurrentVariants();
            Repaint();
        }

        private void drawPreviewTab()
        {
            if (mainCollection != null)
            {
                previewMode = DrawEnumToolbar(previewMode, SUBCATEGORY_TOOLBAR_WIDTH, ShadersLimiter.SUBCATEGORY_TOOLBAR_COLOR, 22f, 12);
                SmallSpace();
                DrawLine();
                SmallSpace();
            }

            using (var _changeScope = new ChangeCheckScope())
            {
                using (new HorizontalScope())
                {
                    mainCollection = EditorGUILayout.ObjectField("Main collection", mainCollection, typeof(ShaderVariantCollection), false) as ShaderVariantCollection;

                    if (mainCollection != null)
                    {
                        GUIStyle _labelToTheRight = new GUIStyle(EditorStyles.boldLabel);
                        _labelToTheRight.alignment = TextAnchor.MiddleRight;

                        EditorGUILayout.LabelField($"{mainCollection.shaderCount} shaders with {mainCollection.variantCount} variants", _labelToTheRight, FixedWidth(200f));
                    }
                }

                currentShader = EditorGUILayout.ObjectField("Shader", currentShader, typeof(Shader), false) as Shader;

                if (_changeScope.changed)
                {
                    recalculateCurrentVariants();
                    findShadersInTheCollection();
                }
            }

            if (mainCollection == null)
            {
                NormalSpace();
                EditorGUILayout.HelpBox("Please assign shader variant collection before proceeding!", MessageType.Warning);
                return;
            }

            switch (previewMode)
            {
                case PreviewMode.Variants:
                    drawVariantsTab();
                    break;

                case PreviewMode.Shaders:
                    drawShadersTab();
                    break;
            }
        }

        private void findShadersInTheCollection()
        {
            foundShaders = ShadersLimiter.GetShadersInCollection(mainCollection);
        }

        private void drawVariantsTab()
        {
            if (currentShader == null)
            {
                return;
            }

            keywords = EditorGUILayout.TextField("Keywords", keywords);
            currentPass = (PassType) EditorGUILayout.EnumPopup("Pass", currentPass);
            SmallSpace();

            using (new HorizontalScope())
            {
                FlexibleSpace();

                if (DrawBoxButton("Add Variant to the collection", FixedWidthAndHeight(windowWidth / 2f, BUTTON_HEIGHT)))
                {
                    addShaderVariant();
                }

                FlexibleSpace();
            }

            if (currentVariants.IsNullOrEmpty() == false)
            {
                ShaderVariantsWindow.DrawShaderVariants(mainCollection, currentShader, currentVariants, windowWidthWithPaddings, ref keywordsFilter);
            }
        }

        private void drawShadersTab()
        {
            SmallSpace();
            DrawLine();
            SmallSpace();

            shadersFilter = EditorGUILayout.TextField("Filter", shadersFilter);
            SmallSpace();

            using (var _scrollScope = new ScrollViewScope(shadersScrollPosition))
            {
                GUIStyle _labelStyle = Styles.CustomizedButton(SHADERS_LINE_HEIGHT, TextAnchor.MiddleCenter, new RectOffset(0, 0, 0, 0));

                int _index = 0;

                foreach (var _shader in foundShaders)
                {
                    if (_shader == null || _shader.name.ToLower().Contains(shadersFilter.ToLower()) == false)
                    {
                        continue;
                    }

                    using (new HorizontalScope())
                    {
                        GUILayout.Label($"{_index + 1}", _labelStyle, FixedWidth(30f));

                        using (new HorizontalScope(Styles.BoxButton, FixedHeight(SHADERS_LINE_HEIGHT)))
                        {
                            Space(4f);
                            EditorGUILayout.ObjectField(_shader, typeof(Shader), false);
                        }

                        if (GUILayout.Button("Inspect", _labelStyle, FixedWidthAndHeight(60f, SHADERS_LINE_HEIGHT)))
                        {
                            GUI.FocusControl(null);
                            currentShader = _shader;
                            recalculateCurrentVariants();

                            previewMode = PreviewMode.Variants;
                            Repaint();
                        }

                        Space(4f);
                    }

                    _index++;
                }

                shadersScrollPosition = _scrollScope.scrollPosition;
            }
        }

        private void addShaderVariant()
        {
            string[] _keywords = keywords.Trim().ToUpper().Split(' ');
            bool _error = false;

            ShaderVariantCollection.ShaderVariant _variant = default;

            try
            {
                _variant = new ShaderVariantCollection.ShaderVariant(currentShader, currentPass, _keywords);
            }
            catch (System.ArgumentException)
            {
                _error = true;
            }

            if (_error)
            {
                ShadersLimiter.Error($"Can't create a variant of {currentShader.name} with pass {currentPass}.");
                return;
            }

            bool _added = mainCollection.Add(_variant);

            if (_added)
            {
                ShadersLimiter.Log($"Variant of {currentShader.name} with pass {currentPass} was added to the collection {mainCollection.name}.");
                AssetsUtilities.SetAsDirty(mainCollection);
                updateNewVariants(currentShader, _keywords);
                recalculateCurrentVariants();
            }
            else
            {
                ShadersLimiter.Log($"Setup variant is probably already added to the collection.");
            }
        }

        private void recalculateCurrentVariants()
        {
            if (mainCollection == null || currentShader == null)
            {
                return;
            }

            currentVariants = ShadersLimiter.GetShaderVariantsInCollection(mainCollection, currentShader);
        }

        private void drawMergeTab()
        {
            using (new LabelWidthScope(WIDE_LABEL_WIDTH))
            {
                DrawBoldLabel("Settings");
                updateDatabaseAfterAnyModification = EditorGUILayout.Toggle("Update database after any modification", updateDatabaseAfterAnyModification);

                NormalSpace();
                DrawBoldLabel("Collections");
                mainCollection = EditorGUILayout.ObjectField("Main collection", mainCollection, typeof(ShaderVariantCollection), false) as ShaderVariantCollection;
                additionalCollection = EditorGUILayout.ObjectField("Additional collection", additionalCollection, typeof(ShaderVariantCollection), false) as ShaderVariantCollection;
            }

            if (mainCollection == null || additionalCollection == null || mainCollection == additionalCollection)
            {
                SmallSpace();
                EditorGUILayout.HelpBox("Main or Additional collections are null or the same! Can't merge.", MessageType.Warning);
                return;
            }

            SmallSpace();
            EditorGUILayout.HelpBox("Additional collection will be merged into the main collection, make sure that this behaviour is desired.", MessageType.Info);
            SmallSpace();

            using (new HorizontalScope())
            {
                float _buttonWidth = Mathf.Max(windowWidth / 2f, 200f);

                FlexibleSpace();

                using (ColorScope.Background(BLUE))
                {
                    if (DrawBoxButton("Merge collections", FixedWidthAndHeight(_buttonWidth, DEFAULT_LINE_HEIGHT)))
                    {
                        mergeTwoCollectionsTogether(mainCollection, additionalCollection);
                    }
                }

                FlexibleSpace();
            }

            drawNewVariantsButton();
        }

        private void drawInsertVariantsTab()
        {
            using (new LabelWidthScope(WIDE_LABEL_WIDTH))
            {
                DrawBoldLabel("Settings");
                updateDatabaseAfterAnyModification = EditorGUILayout.Toggle("Update database after any modification", updateDatabaseAfterAnyModification);

                NormalSpace();
                DrawBoldLabel("References");
                mainCollection = EditorGUILayout.ObjectField("Main collection", mainCollection, typeof(ShaderVariantCollection), false) as ShaderVariantCollection;

                if (mainCollection == null)
                {
                    SmallSpace();
                    EditorGUILayout.HelpBox("The Shader Variant Collection is null! Please assign one before continuing.", MessageType.Warning);
                    return;
                }

                insertVariantsMode = (InsertVariantsMode) EditorGUILayout.EnumPopup("Insert mode", insertVariantsMode);
            }

            switch (insertVariantsMode)
            {
                case InsertVariantsMode.LogFile:
                    drawAddVariantsfromLogFile();
                    break;

                case InsertVariantsMode.TextFile:
                    drawAddVariantsfromTextFile();
                    break;

                case InsertVariantsMode.ExternalFile:
                    drawAddVariantsfromExternalFile();
                    break;

                case InsertVariantsMode.PastedString:
                    drawAddVariantsfromPastedString();
                    break;

                case InsertVariantsMode.EditorEncounteredVariants:
                    drawAddVariantsfromGraphicSettings();
                    break;
            }

            drawNewVariantsButton();
        }

        private void drawAddVariantsfromLogFile()
        {
            using (new LabelWidthScope(WIDE_LABEL_WIDTH))
            {
                using (var _changeScope = new ChangeCheckScope())
                {
                    gameLogFile = EditorGUILayout.ObjectField(new GUIContent("Game log", "Default Asset with .log file extension"), gameLogFile, typeof(DefaultAsset), false) as DefaultAsset;

                    if (_changeScope.changed)
                    {
                        if (gameLogFile != null)
                        {
                            string _path = AssetDatabase.GetAssetPath(gameLogFile);
                            string _extension = Path.GetExtension(_path).ToLower();

                            bool _properExtension = _extension == LOG_EXTENSION;

                            if (_properExtension == false)
                            {
                                gameLogFile = null;
                                ShadersLimiter.Error($"Game log file has to have a {LOG_EXTENSION} file extension.");
                            }
                        }
                    }
                }
            }

            if (gameLogFile != null)
            {
                drawInfoAndConfirm("Shaders variants found in the player log file will be merged into the variants collection, make sure that this behaviour is desired.");
            }
            else
            {
                drawWarningInfo("Please assign an asset with .log extension before proceeding!");
            }
        }

        private void drawAddVariantsfromTextFile()
        {
            using (new LabelWidthScope(WIDE_LABEL_WIDTH))
            {
                using (var _changeScope = new ChangeCheckScope())
                {
                    gameTextFile = EditorGUILayout.ObjectField(new GUIContent("Game log text file", "Text Asset with .txt file extension"), gameTextFile, typeof(TextAsset), false) as TextAsset;

                    if (_changeScope.changed)
                    {
                        if (gameTextFile != null)
                        {
                            string _path = AssetDatabase.GetAssetPath(gameTextFile);
                            string _extension = Path.GetExtension(_path).ToLower();

                            bool _properExtension = _extension == TEXT_EXTENSION;

                            if (_properExtension == false)
                            {
                                gameTextFile = null;
                                ShadersLimiter.Error($"Game log text file file has to have a {TEXT_EXTENSION} file extension.");
                            }
                        }
                    }
                }
            }

            if (gameTextFile != null)
            {
                drawInfoAndConfirm("Shaders variants found in the player log file will be merged into the variants collection, make sure that this behaviour is desired.");
            }
            else
            {
                drawWarningInfo("Please assign a text file with .txt extension before proceeding!");
            }
        }

        private void drawAddVariantsfromExternalFile()
        {
            using (new HorizontalScope())
            {
                EditorGUILayout.LabelField("File path", GUILayout.Width(WIDE_LABEL_WIDTH));

                float _width = windowWidthWithPaddings - WIDE_LABEL_WIDTH - 9f;

                using (new DisabledScope())
                {
                    EditorGUILayout.TextField(externalLogFilePath);
                }

                if (GUILayout.Button("...", FixedWidth(21f)))
                {
                    string _newPath = EditorUtility.OpenFilePanelWithFilters("Select a file", string.Empty, new string[] { "Text files", "txt,log" });

                    if (_newPath.IsNullOrEmpty() == false)
                    {
                        externalLogFilePath = _newPath;
                    }
                }
            }

            if (externalLogFilePath.IsNullOrEmpty() == false)
            {
                drawInfoAndConfirm("Shaders variants found in the attached file will be merged into the variants collection, make sure that this behaviour is desired.");
            }
            else
            {
                drawWarningInfo("Please search for a text file with .txt or .log extensions before proceeding!");
            }
        }

        private void drawAddVariantsfromPastedString()
        {
            using (new HorizontalScope())
            {
                EditorGUILayout.LabelField("Pasted log content", GUILayout.Width(WIDE_LABEL_WIDTH));

                float _width = windowWidthWithPaddings - WIDE_LABEL_WIDTH - 9f;
                pastedLogString = EditorGUILayout.TextArea(pastedLogString, FixedWidthAndHeight(_width, 120f));
            }

            if (pastedLogString.IsNullOrEmpty() == false)
            {
                drawInfoAndConfirm("Shaders variants found in the pasted string will be merged into the variant collection, make sure that this behaviour is desired.");
            }
            else
            {
                drawWarningInfo("Please paste log file content into the text area before proceeding!");
            }
        }

        private void drawAddVariantsfromGraphicSettings()
        {
            SmallSpace();

            int _shaders = (int) AssemblyExtensions.InvokeInternalStaticMethod(typeof(ShaderUtil), ENCOUNTERED_SHADERS_COUNT_METHOD_NAME);
            int _variants = (int) AssemblyExtensions.InvokeInternalStaticMethod(typeof(ShaderUtil), ENCOUNTERED_VARIANTS_COUNT_METHOD_NAME);
            GUILayout.Label($"Currently tracked: {_shaders} shaders with {_variants} total variants");

            drawInfoAndConfirm("Shaders variants found in the 'EditorEncounteredVariants' file in the 'Library/ShaderCache' folder will be merged into the variant collection, make sure that this behaviour is desired.");
        }

        private void drawInfoAndConfirm(string _helpBoxInfo)
        {
            SmallSpace();
            EditorGUILayout.HelpBox(_helpBoxInfo, MessageType.Info);
            SmallSpace();

            using (new HorizontalScope())
            {
                float _buttonWidth = Mathf.Max(windowWidth / 2f, 200f);

                FlexibleSpace();

                using (ColorScope.Background(BLUE))
                {
                    if (DrawBoxButton("Insert variants into the collection", FixedWidthAndHeight(_buttonWidth, DEFAULT_LINE_HEIGHT)))
                    {
                        switch (insertVariantsMode)
                        {
                            case InsertVariantsMode.LogFile:
                                addShaderVariantsFromLogFile();
                                break;

                            case InsertVariantsMode.TextFile:
                                addShaderVariantsFromTextFile();
                                break;

                            case InsertVariantsMode.ExternalFile:
                                addShaderVariantsFromExternalFile();
                                break;

                            case InsertVariantsMode.PastedString:
                                addShaderVariantsFromPastedString();
                                break;

                            case InsertVariantsMode.EditorEncounteredVariants:
                                addShaderVariantsFromGraphicSettings();
                                break;
                        }

                        if (updateDatabaseAfterAnyModification)
                        {
                            updateShadersAndRefreshVariants();
                        }
                    }
                }

                FlexibleSpace();
            }
        }

        private void drawWarningInfo(string _warning)
        {
            SmallSpace();
            EditorGUILayout.HelpBox(_warning, MessageType.Warning);
        }

        private void addShaderVariantsFromLogFile()
        {
            string[] _allLines = File.ReadAllLines(AssetDatabase.GetAssetPath(gameLogFile));
            addShaderVariantsFromLogsLines(_allLines, gameLogFile.name);
        }

        private void addShaderVariantsFromTextFile()
        {
            string[] _allLines = File.ReadAllLines(AssetDatabase.GetAssetPath(gameTextFile));
            addShaderVariantsFromLogsLines(_allLines, gameTextFile.name);
        }

        private void addShaderVariantsFromExternalFile()
        {
            if (externalLogFilePath.IsNullOrEmpty())
            {
                ShadersLimiter.Error("External file path is empty!");
                return;
            }

            if (File.Exists(externalLogFilePath) == false)
            {
                ShadersLimiter.Error("External file doesn't exist!");
                return;
            }

            string[] _allLines = File.ReadAllLines(externalLogFilePath);
            addShaderVariantsFromLogsLines(_allLines, externalLogFilePath);
        }

        private void addShaderVariantsFromPastedString()
        {
            string[] _allLines = pastedLogString.Split('\n');
            addShaderVariantsFromLogsLines(_allLines, "Pasted log");
        }

        private void addShaderVariantsFromGraphicSettings()
        {
            //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/GraphicsSettingsInspectors/GraphicsSettingsInspectorShaderPreload.cs
            AssemblyExtensions.InvokeInternalStaticMethod(typeof(ShaderUtil), SAVE_CURRENT_VARIANT_COLLECTION_METHOD_NAME, VARIANT_COLLECTION_DEFAULT_PATH);

            ShaderVariantCollection _newCollection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(VARIANT_COLLECTION_DEFAULT_PATH);
            mergeTwoCollectionsTogether(mainCollection, _newCollection);

            AssetDatabase.DeleteAsset(VARIANT_COLLECTION_DEFAULT_PATH);
            AssetDatabase.Refresh();
        }

        private void addShaderVariantsFromLogsLines(string[] _lines, string _fileName)
        {
            List<string> _missingShadersLines = new List<string>();
            List<string> _compiledShadersLines = new List<string>();

            foreach (var _line in _lines)
            {
                if (_line.Trim().StartsWith(MISSING_SHADER_LINE))
                {
                    _missingShadersLines.Add(_line);
                }

                if (_line.Trim().StartsWith(COMPILED_SHADER_LINE))
                {
                    _compiledShadersLines.Add(_line);
                }
            }

            if (_missingShadersLines.IsNullOrEmpty() && _compiledShadersLines.IsNullOrEmpty())
            {
                ShadersLimiter.Log($"Shaders Limiter didn't find any missing or compiled shaders in the provided source.");
                return;
            }

            bool _anyAdded = false;
            int _addedCount = 0;

            Undo.RecordObject(mainCollection, $"Adding shader variants from {_fileName} to the collection.");

            foreach (var _uniqueLine in _missingShadersLines)
            {
                string[] _split = _uniqueLine.Split(',');

                if (_split.Length < 4)
                {
                    continue;
                }

                string _shaderName = _split[0].TrimStart(MISSING_SHADER_LINE).Trim();
                Shader _foundShader = _findShaderWithName(ref _shaderName);

                if (_foundShader == null)
                {
                    ShadersLimiter.Error($"{_shaderName} couldn't be found in the project, make sure that this shader is present in the project!");
                    continue;
                }

                string _foundKeywords = _split[3].Trim().TrimStart(STAGE_VERTEX).TrimStart(STAGE_PIXEL).TrimEnd(NOT_FOUND).Trim();

                if (_foundKeywords.Contains(NO_KEYWORDS))
                {
                    addShaderWithKeywords(ref _anyAdded, ref _addedCount, _foundShader, new string[] { });
                    continue;
                }

                string[] _splitKeywords = _foundKeywords.ToUpper().Split(' ');
                addShaderWithKeywords(ref _anyAdded, ref _addedCount, _foundShader, _splitKeywords);
            }

            foreach (var _compiledLine in _compiledShadersLines)
            {
                string[] _split = _compiledLine.Split(',');

                if (_split.Length < 4)
                {
                    continue;
                }

                string _shaderName = _split[0].TrimStart(COMPILED_SHADER_LINE).Trim();
                Shader _foundShader = _findShaderWithName(ref _shaderName);

                if (_foundShader == null)
                {
                    ShadersLimiter.Error($"{_shaderName} couldn't be found in the project, make sure that this shader is present in the project!");
                    continue;
                }

                string _foundKeywords = _split[3].Trim().TrimStart(KEYWORDS).Trim();

                if (_foundKeywords.Contains(NO_KEYWORDS))
                {
                    addShaderWithKeywords(ref _anyAdded, ref _addedCount, _foundShader, new string[] { });
                    continue;
                }

                string[] _splitKeywords = _foundKeywords.ToUpper().Split(' ');
                addShaderWithKeywords(ref _anyAdded, ref _addedCount, _foundShader, _splitKeywords);
            }

            if (_anyAdded)
            {
                ShadersLimiter.Log($"Added {_addedCount} shader variants to the {mainCollection.name}.");
                AssetsUtilities.SetAsDirty(mainCollection);
                recalculateCurrentVariants();

                if (updateDatabaseAfterAnyModification)
                {
                    updateShadersAndRefreshVariants();
                }
            }
            else
            {
                ShadersLimiter.Log($"All shaders and variants from {_fileName} are already in the {mainCollection.name}.");
            }

            Shader _findShaderWithName(ref string _shaderName)
            {
                Shader _foundShader = Shader.Find(_shaderName);

                if (_foundShader == null)
                {
                    int _startIndex = _shaderName.IndexOf('(');
                    int _endIndex = _shaderName.IndexOf(')');

                    if (_startIndex == -1 || _endIndex == -1)
                    {
                        return null;
                    }

                    //Sometimes in the logs there is a "real shader" string after the shader name so we can search using that
                    string _trimmedRealName = _shaderName.Substring(_startIndex + 1, _endIndex - _startIndex - 1);
                    _trimmedRealName = _trimmedRealName.TrimStart(REAL_SHADER).Trim();

                    _foundShader = Shader.Find(_trimmedRealName);
                    _shaderName = _trimmedRealName;
                }

                return _foundShader;
            }
        }

        private void addShaderWithKeywords(ref bool _anyAdded, ref int _addedCount, Shader _foundShader, string[] _keywords)
        {
            bool _error = false;

            for (int i = 0; i < _keywords.Length; i++)
            {
                _keywords[i] = _keywords[i].Trim().ToUpper();
            }

            foreach (var _pass in PASSES)
            {
                ShaderVariantCollection.ShaderVariant _variant = default;

                try
                {
                    _variant = new ShaderVariantCollection.ShaderVariant(_foundShader, (PassType) _pass, _keywords);
                }
                catch (System.ArgumentException)
                {
                    _error = true;
                }

                if (_error)
                {
                    _error = false;
                    continue;
                }

                bool _added = mainCollection.Add(_variant);

                if (_added)
                {
                    _anyAdded = true;
                    _addedCount++;
                    updateNewVariants(_variant.shader, _keywords);
                    ShadersLimiter.Log($"Variant of {_foundShader.name} was added to the collection {mainCollection.name}");
                }
            }
        }

        private void drawPassesTab()
        {
            GUIStyle _labelStyle = Styles.CustomizedButton(SINGLE_LINE_HEIGHT, TextAnchor.MiddleCenter, new RectOffset(0, 0, 0, 0));
            GUIStyle _buttonStyle = Styles.CustomizedButton(SINGLE_LINE_HEIGHT, TextAnchor.MiddleLeft, new RectOffset(5, 0, 0, 0));

            using (ColorScope.Background(BLUE))
            {
                using (new HorizontalScope())
                {
                    GUILayout.Label($"Index", _labelStyle, FixedWidth(INDEX_WIDTH));
                    GUILayout.Label($"Shader Pass", _buttonStyle, FixedWidth(passWidth));
                }
            }

            using (var _scrollViewScope = new ScrollViewScope(scrollPosition))
            {
                scrollPosition = _scrollViewScope.scrollPosition;

                for (int i = 0; i < PASSES.Length; i++)
                {
                    using (new HorizontalScope())
                    {
                        int _index = (int) PASSES.GetValue(i);
                        PassType _pass = (PassType) _index;
                        string _shaderPassLabel = _pass.ToString().InsertSpaceBeforeUpperCaseAndNumeric();

                        GUILayout.Label($"{_index}", _labelStyle, FixedWidth(INDEX_WIDTH));
                        GUILayout.Label($"{_shaderPassLabel}", _buttonStyle, FixedWidth(passWidth));
                    }
                }
            }

            NormalSpace();
        }

        private void drawNewVariantsButton()
        {
            if (newVariants.IsNullOrEmpty())
            {
                return;
            }

            SmallSpace();
            DrawLine();
            SmallSpace();

            SmallSpace();
            EditorGUILayout.HelpBox("Shaders Limiter discovered new shader variants in the attached collections. It's recommended to delete associated shaders' caches and force Unity to recompile them during the next build.", MessageType.Warning);
            SmallSpace();

            using (ColorScope.Background(ORANGE))
            {
                using (new HorizontalScope())
                {
                    FlexibleSpace();

                    if (DrawBoxButton("Show new variants", FixedWidthAndHeight(windowWidth / 2f, BUTTON_HEIGHT)))
                    {
                        NewVariantsWindow.OpenWindow(newVariants);
                    }

                    FlexibleSpace();
                }
            }
        }

        private void updateNewVariants(Shader _shader, string[] _keywords)
        {
            foreach (var _newVariant in newVariants)
            {
                if (_newVariant.ShaderReference == _shader)
                {
                    _newVariant.IncrementCount(_keywords);
                    return;
                }
            }

            newVariants.Add(new NewVariant(_shader));
        }

        private void mergeTwoCollectionsTogether(ShaderVariantCollection _mainCollection, ShaderVariantCollection _additionalCollection)
        {
            if (_mainCollection == null || _additionalCollection == null)
            {
                ShadersLimiter.Error("Main or Additional collections are null! Can't merge.");
                return;
            }

            List<CompiledShaderVariant> _additionalVariants = ShadersLimiter.GetShaderVariantsInCollection(_additionalCollection, null);

            if (_additionalVariants.IsNullOrEmpty())
            {
                FlexibleSpace();
                return;
            }

            bool _anyAdded = false;
            int _addedCount = 0;

            Undo.RecordObject(_mainCollection, $"Adding variants from {_additionalCollection.name} to {_mainCollection.name}.");

            foreach (var _shaderVariant in _additionalVariants)
            {
                bool _error = false;
                ShaderVariantCollection.ShaderVariant _variant = default;

                try
                {
                    _variant = new ShaderVariantCollection.ShaderVariant(_shaderVariant.ShaderReference, _shaderVariant.ShaderPass, _shaderVariant.KeywordsSet);
                }
                catch (System.ArgumentException)
                {
                    _error = true;
                }

                if (_error)
                {
                    continue;
                }

                bool _added = _mainCollection.Add(_variant);

                if (_added)
                {
                    _anyAdded = true;
                    _addedCount++;
                    updateNewVariants(_variant.shader, _shaderVariant.KeywordsSet);
                    ShadersLimiter.Log($"Variant of {_shaderVariant.ShaderReference.name} was added to the {_mainCollection.name}.");
                }
            }

            if (_anyAdded)
            {
                ShadersLimiter.Log($"Added {_addedCount} shader variants to the {_mainCollection.name}.");
                AssetsUtilities.SetAsDirty(_mainCollection);

                if (updateDatabaseAfterAnyModification)
                {
                    updateShadersAndRefreshVariants();
                }
            }
            else
            {
                ShadersLimiter.Log($"All shaders and variants from {_additionalCollection.name} are already in the {_mainCollection.name}.");
            }
        }

        private void updateShadersAndRefreshVariants()
        {
            foreach (var _newShader in newVariants)
            {
                ShaderKeyword[] _keywords = new ShaderKeyword[_newShader.AllKeywords.Count];

                for (int i = 0; i < _keywords.Length; i++)
                {
                    _keywords[i] = new ShaderKeyword(_newShader.AllKeywords[i]);
                }

                bool _addedNew = ShadersLimiterDatabase.AddShaderToTheDatabase(_newShader.ShaderReference, _keywords);

                if (_addedNew)
                {
                    ShadersLimiter.Log($"Shader {_newShader.ShaderReference.name} was added to the database!");
                    ShadersLimiterDatabase.SetAsDirty();
                }
            }

            ShadersLimiterDatabase.RefreshShaderVariants();
        }

        [MenuItem("Window/FewClicks Dev/Shader Variants Collection Editor")]
        public static void OpenWindow()
        {
            GetWindow<VariantsCollectionEditorWindow>().Show();
        }

        public static void OpenWindow(ShaderVariantCollection _collection)
        {
            var _window = GetWindow<VariantsCollectionEditorWindow>();
            _window.Show();
            _window.OpenWithCollection(_collection);
        }
    }
}