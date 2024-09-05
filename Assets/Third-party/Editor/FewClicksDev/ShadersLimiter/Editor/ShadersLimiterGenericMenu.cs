namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using UnityEditor;
    using UnityEngine;

    public static class ShadersLimiterGenericMenu
    {
        private const string TOOLTIP_SELECT = "Select the current shader in the project view.";
        private const string TOOLTIP_STRIP_WHOLE_SHADER = "Shader won't be included in the next builds.";
        private const string TOOLTIP_CLEAR_CACHE = "Force Unity to recompile Shader in the next build. Clear the cache each time you strip some keywords.";
        private const string TOOLTIP_REMOVE_FROM_DATABASE = "Remove Shader from the database. Can be used if you don't have a specified shader in your project anymore.";
        private const string TOOLTIP_REIMPORT = "Reimport shader, forcing Unity to recompile it.";

        private const string TOOLTIP_FORCE_PRINT_ON_COMPILE = "Force the tool to print logs on shader variant compilation.";
        private const string TOOLTIP_FORCE_PRINT_ON_STRIP = "Force the tool to print logs on shader variant stripping.";

        private const string TOOLTIP_STRIP_KEYWORD = "Strip this keyword from the shader.";
        private const string TOOLTIP_STRIP_GLOBALLY = "Strip this keyword from all shaders in the project.";
        private const string TOOLTIP_GENERATE_DESCRIPTION = "Generate a description for this keyword.";
        private const string TOOLTIP_SHOW_DESCRIPTION = "Open the description window for this keyword.";

        private static KeywordDescription keywordDescription = null;

        public static void ShowShaderMenu(ShadersLimiterDatabase _database, Event _currentEvent, ShaderToStrip _shaderToStrip)
        {
            GenericMenu _menu = new GenericMenu();

            _menu.AddDisabledItem(new GUIContent(_shaderToStrip.Path.Replace("/", " | ")));
            _menu.AddSeparator(string.Empty);
            _menu.AddItem(new GUIContent("Select", TOOLTIP_SELECT), false, _selectAndPing);
            _menu.AddItem(new GUIContent("Strip whole Shader", TOOLTIP_STRIP_WHOLE_SHADER), _shaderToStrip.IsStripped, _stripWholeShader);
            _menu.AddItem(new GUIContent("Clear cache", TOOLTIP_CLEAR_CACHE), false, _clearShaderCache);
            _menu.AddItem(new GUIContent("Remove from the Database", TOOLTIP_REMOVE_FROM_DATABASE), false, _removeFromDatabase);
            _menu.AddSeparator(string.Empty);
            _menu.AddItem(new GUIContent("Force log printing on compile", TOOLTIP_FORCE_PRINT_ON_COMPILE), _shaderToStrip.ForcePrintLogsOnCompile, _toggleForcePrintOnCompile);
            _menu.AddItem(new GUIContent("Force log printing on strip", TOOLTIP_FORCE_PRINT_ON_STRIP), _shaderToStrip.ForcePrintLogsOnStrip, _toggleForcePrintOnStrip);
            _menu.AddSeparator(string.Empty);
            _menu.AddItem(new GUIContent("Reimport", TOOLTIP_REIMPORT), false, _reimport);
            _menu.ShowAsContext();

            _currentEvent.Use();

            void _selectAndPing()
            {
                Selection.activeObject = _shaderToStrip.ShaderReference;
                EditorGUIUtility.PingObject(_shaderToStrip.ShaderReference);
            }

            void _stripWholeShader()
            {
                Undo.RecordObject(_database, $"Toggled stripping whole shader on {_shaderToStrip.ShortName}.");
                _shaderToStrip.SetAsStripped(!_shaderToStrip.IsStripped);
                _clearShaderCache();
            }

            void _clearShaderCache()
            {
                _shaderToStrip.ClearShaderCache();

                ShadersLimiterDatabase.DeleteBuildCache();
                ShadersLimiterDatabase.DeleteSingleShaderCache(_shaderToStrip);
            }

            void _removeFromDatabase()
            {
                _database.RemoveShader(_shaderToStrip);
            }

            void _toggleForcePrintOnCompile()
            {
                Undo.RecordObject(_database, $"Toggled force print on compile on {_shaderToStrip.ShortName}.");
                _shaderToStrip.SetPrintLogsOnCompile(!_shaderToStrip.ForcePrintLogsOnCompile);
            }

            void _toggleForcePrintOnStrip()
            {
                Undo.RecordObject(_database, $"Toggled force print on strip on {_shaderToStrip.ShortName}.");
                _shaderToStrip.SetPrintLogsOnStrip(!_shaderToStrip.ForcePrintLogsOnStrip);
            }

            void _reimport()
            {
                string _assetPath = AssetDatabase.GetAssetPath(_shaderToStrip.ShaderReference);

                if (_assetPath.EndsWith("builtin_extra") == false) // Built in shaders from the old pipeline can't be reimported
                {
                    AssetDatabase.ImportAsset(_assetPath);
                }
                else
                {
                    ShadersLimiter.Warning($"Built-in shaders can't be reimported! Shader path: {_shaderToStrip.Path}.");
                }
            }
        }

        public static void ShowKeywordMenu(ShadersLimiterDatabase _database, Event _currentEvent, Keyword _keyword)
        {
            GenericMenu _menu = new GenericMenu();
            string _keywordName = _keyword.KeywordName;
            keywordDescription = ShadersLimiterDatabase.GetKeywordDescription(_keywordName);

            _menu.AddDisabledItem(new GUIContent(_keywordName));
            _menu.AddSeparator(string.Empty);
            _menu.AddItem(new GUIContent("Strip", TOOLTIP_STRIP_KEYWORD), _keyword.IsStripped, () => stripKeyword(_database, _keyword));
            drawMenuCommonPart(_menu, _database, _currentEvent, _keywordName, true);
        }

        public static void ShowStringKeywordMenu(ShadersLimiterDatabase _database, Event _currentEvent, string _keyword, bool _showGlobalStripping = false)
        {
            GenericMenu _menu = new GenericMenu();
            keywordDescription = ShadersLimiterDatabase.GetKeywordDescription(_keyword);

            _menu.AddDisabledItem(new GUIContent(_keyword));
            _menu.AddSeparator(string.Empty);
            drawMenuCommonPart(_menu, _database, _currentEvent, _keyword, _showGlobalStripping);
        }

        private static void drawMenuCommonPart(GenericMenu _menu, ShadersLimiterDatabase _database, Event _currentEvent, string _keyword, bool _showGlobalStripping = false)
        {
            if (_showGlobalStripping)
            {
                bool _globallyStripped = ShadersLimiterDatabase.IsKeywordGloballyStripped(_keyword);
                _menu.AddItem(new GUIContent("Strip Globally", TOOLTIP_STRIP_GLOBALLY), _globallyStripped, () => stripKeywordGlobally(_database, _keyword));
                _menu.AddSeparator(string.Empty);
            }

            if (keywordDescription == null)
            {
                _menu.AddItem(new GUIContent("Generate Description", TOOLTIP_GENERATE_DESCRIPTION), false, () => generateDescription(_keyword));
            }
            else
            {
                _menu.AddItem(new GUIContent("Show Description", TOOLTIP_SHOW_DESCRIPTION), false, showDescription);
            }

            _menu.ShowAsContext();
            _currentEvent.Use();
        }

        private static void showDescription()
        {
            KeywordDescriptionWindow.Show(keywordDescription);
        }

        private static void stripKeyword(ShadersLimiterDatabase _database, Keyword _keyword)
        {
            Undo.RecordObject(_database, $"Toggled {_keyword.KeywordName} keyword stripping.");
            _keyword.SetAsStripped(!_keyword.IsStripped);
        }

        private static void stripKeywordGlobally(ShadersLimiterDatabase _database, string _keyword)
        {
            if (ShadersLimiterDatabase.IsKeywordGloballyStripped(_keyword))
            {
                Undo.RecordObject(_database, $"Removed {_keyword} keyword from globally stripped.");
                ShadersLimiterDatabase.RemoveFromGloballyStripped(_keyword);
                return;
            }

            Undo.RecordObject(_database, $"Added {_keyword} keyword to globally stripped.");
            ShadersLimiterDatabase.AddToGloballyStripped(_keyword);
        }

        private static void generateDescription(string _keyword)
        {
            string _folderPath = Application.dataPath + "/Plugins/FewClicksDev/ShadersLimiter/Data";
            string _assetPath = EditorUtility.SaveFolderPanel("Choose folder to create a new Keyword Description", _folderPath, string.Empty);

            if (_assetPath.Length > 0)
            {
                string _descriptionName = $"keywordDescription_{_keyword.TrimStart("_")}.asset";
                string _fixedAssetPath = AssetsUtilities.ConvertAbsolutePathToDataPath(_assetPath, _descriptionName);
                _fixedAssetPath = AssetDatabase.GenerateUniqueAssetPath(_fixedAssetPath);

                KeywordDescription _asset = ScriptableObject.CreateInstance<KeywordDescription>();
                _asset.Setup(_keyword);

                AssetDatabase.CreateAsset(_asset, _fixedAssetPath);
                AssetDatabase.Refresh();

                Undo.RegisterCreatedObjectUndo(_asset, $"Created new description for keyword {_keyword}.");

                EditorUtility.FocusProjectWindow();
                EditorGUIUtility.PingObject(_asset);
            }
        }
    }
}
