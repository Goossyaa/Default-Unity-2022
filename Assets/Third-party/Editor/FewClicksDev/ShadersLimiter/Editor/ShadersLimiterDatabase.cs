namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    [CreateAssetMenu(menuName = "FewClicks Dev/Shaders Limiter/Database", fileName = "data_ShadersLimiter")]
    public class ShadersLimiterDatabase : ScriptableObject
    {
        private static readonly Color DEFAULT_STRIPPED_SHADER_COLOR = new Color(0.6901961f, 0.3294118f, 0.3294118f, 1f);
        private static readonly Color DEFAULT_NOT_STRIPPED_SHADER_COLOR = new Color(0.2784314f, 0.6235294f, 0.2784314f, 1f);
        private static readonly Color DEFAULT_SHADER_WITH_STRIPPED_KEYWORDS_COLOR = new Color(0.8490566f, 0.8034721f, 0.252314f, 1f);
        private static readonly Color DEFAULT_COMPILE_ONLY_SPECIFIED_COLOR = new Color(0.1677643f, 0.3720466f, 0.5471698f, 1f);

        private static int DEFAULTY_MAX_VISIBLE_KEYWORDS = 10;

        public static bool DatabaseExists => instance != null;
        public static bool StrippingEnabled => DatabaseExists ? instance.ShouldStripShaders : false;
        public static bool StrippingEnabledForFirstBuild => DatabaseExists ? instance.StripAllShadersForFirstBuild : false;
        public static bool UsageLogsEnabled => DatabaseExists ? instance.PrintUsageLogs : false;
        public static bool CompiledShadersLogsEnabled => DatabaseExists ? instance.PrintCompiledShadersLogs : false;
        public static bool StrippedShaderLogsEnabled => DatabaseExists ? instance.PrintStrippedShaderLogs : false;
        public static bool ShouldDeleteComputeShadersCache => DatabaseExists ? instance.DeleteComputeShadersCache : false;

        public static int MaxNumberVisibleKeywords => DatabaseExists ? instance.MaxVisibleKeywords : DEFAULTY_MAX_VISIBLE_KEYWORDS;

        private static ShadersLimiterDatabase instance
        {
            get
            {
                if (databaseReference == null)
                {
                    databaseReference = AssetsUtilities.GetScriptableOfType<ShadersLimiterDatabase>();
                }

                return databaseReference;
            }
        }

        private static ShadersLimiterDatabase databaseReference = null;

        //Settings
        [Tooltip("It is the main flag of the tool; it turns on and off shader stripping for this project.")]
        [SerializeField] private bool shouldStripShaders = false;

        [Tooltip("It is the maximum number of keywords that will be visible in the expanded shader view. If you have a lot of keywords, you can increase this number.")]
        [SerializeField] private int maxVisibleKeywords = DEFAULTY_MAX_VISIBLE_KEYWORDS;

        [Tooltip("This flag is being set to true during the first build. It prevents ALL shader compilations (builds will be pink and broken).")]
        [SerializeField] private bool stripAllShadersForFirstBuild = false;

        [Tooltip("Set this flag to true if you want to delete the whole 'ShaderCache' folder from the library when pressing 'Clear Shaders Cache' button; keep it to false if you wish to skip the compute shader cache.")]
        [SerializeField] private bool deleteComputeShadersCache = false;

        [Tooltip("Prints all usage logs to the console.")]
        [SerializeField] private bool printUsageLogs = true;

        [Tooltip("Prints all compiled shaders to the console. It may elongate build time!")]
        [SerializeField] private bool printCompiledShadersLogs = false;

        [Tooltip("Prints all stripped shaders to the console. It may elongate build time!")]
        [SerializeField] private bool printStrippedShaderLogs = false;

        public bool ShouldStripShaders => shouldStripShaders;
        public int MaxVisibleKeywords => maxVisibleKeywords;
        public bool StripAllShadersForFirstBuild => stripAllShadersForFirstBuild;
        public bool DeleteComputeShadersCache => deleteComputeShadersCache;
        public bool PrintUsageLogs => printUsageLogs;
        public bool PrintCompiledShadersLogs => printCompiledShadersLogs;
        public bool PrintStrippedShaderLogs => printStrippedShaderLogs;

        //Display
        [Tooltip("Display order of the shaders")]
        [SerializeField] private ShadersDisplayOrder displayOrder = ShadersDisplayOrder.NumberOfKeywords;

        [Tooltip("Visibility of the shaders")]
        [SerializeField] private ShaderType visibility = ShaderType.Default | ShaderType.Stripped;

        public ShadersDisplayOrder DisplayOrder => displayOrder;
        public ShaderType Visibility => visibility;

        //Colors
        [Tooltip("Color of shader that is completely stripped during the build.")]
        [SerializeField] private Color strippedShaderColor = DEFAULT_STRIPPED_SHADER_COLOR;

        [Tooltip("Default color indicator; shader and its keywords are not stripped.")]
        [SerializeField] private Color notStrippedShaderColor = DEFAULT_NOT_STRIPPED_SHADER_COLOR;

        [Tooltip("This color will be present if the shader is not stripped, but some of its keywords are.")]
        [SerializeField] private Color someKeywordsStrippedColor = DEFAULT_SHADER_WITH_STRIPPED_KEYWORDS_COLOR;

        [Tooltip("The color of the shader that is using conditional compilation; only shaders present in the provided shader variant collections will be compiled.")]
        [SerializeField] private Color compileOnlySpecifiedColor = DEFAULT_COMPILE_ONLY_SPECIFIED_COLOR;

        public Color StrippedShaderColor => strippedShaderColor;
        public Color NotStrippedShaderColor => notStrippedShaderColor;
        public Color SomeKeywordsStrippedColor => someKeywordsStrippedColor;
        public Color CompileOnlySpecifiedColor => compileOnlySpecifiedColor;

        //Data
        [SerializeField] private List<ShaderToStrip> allShaders = new List<ShaderToStrip>();
        [SerializeField] private List<string> globallyStrippedKeywords = new List<string>();
        [SerializeField] private List<string> uniqueKeywords = new List<string>();
        [SerializeField] private List<KeywordDescription> keywordDescriptions = new List<KeywordDescription>();

        public List<ShaderToStrip> AllShaders => allShaders;
        public List<string> GloballyStrippedKeywords => globallyStrippedKeywords;
        public List<string> UniqueKeywords => uniqueKeywords;

        public void ClearDatabase()
        {
            allShaders.Clear();
            SetAsDirty();
        }

        public void ResetAllSettingsToDefault()
        {
            maxVisibleKeywords = DEFAULTY_MAX_VISIBLE_KEYWORDS;
            stripAllShadersForFirstBuild = false;
            deleteComputeShadersCache = false;

            ResetLogsSettings();
            ResetColorsToDefault();

            ShadersLimiter.Log("All settings were changed to defaults.");
        }

        public void ResetLogsSettings()
        {
            Undo.RecordObject(this, $"Reset logs to default.");

            printUsageLogs = true;
            printCompiledShadersLogs = false;
            printStrippedShaderLogs = false;

            SetAsDirty();
        }

        public void ResetColorsToDefault()
        {
            Undo.RecordObject(this, $"Reset colors to default.");

            strippedShaderColor = DEFAULT_STRIPPED_SHADER_COLOR;
            notStrippedShaderColor = DEFAULT_NOT_STRIPPED_SHADER_COLOR;
            someKeywordsStrippedColor = DEFAULT_SHADER_WITH_STRIPPED_KEYWORDS_COLOR;
            compileOnlySpecifiedColor = DEFAULT_COMPILE_ONLY_SPECIFIED_COLOR;

            SetAsDirty();
        }

        public void RemoveShader(ShaderToStrip _shader)
        {
            Undo.RecordObject(this, $"Removing {_shader.ShortName} from the database.");

            allShaders.Remove(_shader);
            SetAsDirty();
        }



        public void EnableConditionalCompilation()
        {
            Undo.RecordObject(this, $"Enabling 'Compile only specified' in the whole database.");

            foreach (var _shader in AllShaders)
            {
                _shader.SetCompileOnlySpecified(true);
            }
        }

        public void DisableConditionalCompilation()
        {
            Undo.RecordObject(this, $"Disabling 'Compile only specified' in the whole database.");

            foreach (var _shader in AllShaders)
            {
                _shader.SetCompileOnlySpecified(false);
            }
        }

        public void UnstripAllKeywordsInShaders()
        {
            Undo.RecordObject(this, $"Unstripping all keywords in the whole database.");

            foreach (var _shader in AllShaders)
            {
                _shader.SetAllKeywordsAsStripped(false);
            }
        }

        public void StripKeywordsBasedOnCollections()
        {
            Undo.RecordObject(this, $"Stripping keywords in the whole database based on the attached shader variant collections.");

            foreach (var _shader in AllShaders)
            {
                _shader.StripKeywordsNotIncludedInCollection();
            }
        }

        public void AddCollectionToAllShaders(ShaderVariantCollection _collection)
        {
            if (_collection == null) //This should never happen
            {
                return;
            }

            Undo.RecordObject(this, $"Adding {_collection.name} shader variant collection to all Shaders to Strip.");

            foreach (var _shader in AllShaders)
            {
                _shader.AddCollection(_collection);
            }
        }

        public void RemoveCollectionFromAllShaders(ShaderVariantCollection _collection)
        {
            if (_collection == null) //This should never happen
            {
                return;
            }

            Undo.RecordObject(this, $"Removing {_collection.name} shader variant collection from all Shaders to Strip.");

            foreach (var _shader in AllShaders)
            {
                _shader.RemoveCollection(_collection);
            }
        }

        public void ClearAllCollections()
        {
            Undo.RecordObject(this, $"Clearing all shader variant collections from all Shaders to Strip.");

            foreach (var _shader in AllShaders)
            {
                _shader.ClearAllCollections();
            }
        }

        private bool addShaderToTheDatabase(Shader _shaderReference, ShaderKeyword[] _keywords)
        {
            if (_shaderReference == null)
            {
                ShadersLimiter.Error("Can't add a null shader to the database!");
                return false;
            }

            var _containsAtIndex = contains(_shaderReference);

            if (_containsAtIndex.Item1 == false)
            {
                AllShaders.Add(new ShaderToStrip(_shaderReference, _keywords));
                return true;
            }

            AllShaders[_containsAtIndex.Item2].UpdateKeywordsToStrip(_keywords);
            instance.SetAsDirty();

            return false;
        }

        private ShaderToStrip getShaderToStrip(Shader _shaderReference)
        {
            for (int i = 0; i < AllShaders.Count; i++)
            {
                if (AllShaders[i].ShaderReference == _shaderReference)
                {
                    return AllShaders[i];
                }
            }

            return default;
        }

        private (bool, int) contains(Shader _shaderReference)
        {
            for (int i = 0; i < AllShaders.Count; i++)
            {
                if (AllShaders[i].ShaderReference == _shaderReference)
                {
                    return (true, i);
                }
            }

            return (false, -1);
        }

        private KeywordDescription getKeywordDescription(string _keyword)
        {
            foreach (var _description in keywordDescriptions)
            {
                if (_description.KeywordName == _keyword)
                {
                    return _description;
                }
            }

            return null;
        }

        private void registerToKeywordsDatabase(KeywordDescription _description)
        {
            if (_description == null)
            {
                return;
            }

            if (keywordDescriptions.Contains(_description) == false)
            {
                keywordDescriptions.Add(_description);
                SetAsDirty();
            }

            for (int i = keywordDescriptions.Count - 1; i >= 0; i--)
            {
                if (keywordDescriptions[i] == null)
                {
                    keywordDescriptions.RemoveAt(i);
                    SetAsDirty();
                }
            }
        }

        public static bool AddShaderToTheDatabase(Shader _shaderReference, ShaderKeyword[] _keywords)
        {
            return instance.addShaderToTheDatabase(_shaderReference, _keywords);
        }

        public static ShaderToStrip GetShaderToStrip(Shader _shaderReference)
        {
            if (DatabaseExists == false)
            {
                return null;
            }

            return instance.getShaderToStrip(_shaderReference);
        }

        public static void SetAsDirty(bool _save = false)
        {
            if (DatabaseExists == false)
            {
                return;
            }

            if (_save)
            {
                instance.SetDirtyAndSave();
            }
            else
            {
                instance.SetAsDirty();
            }
        }

        public static void EnableFlagForTheFirstBuild()
        {
            if (DatabaseExists == false)
            {
                return;
            }

            instance.stripAllShadersForFirstBuild = true;
            instance.SetDirtyAndSave();
        }

        public static void FinishFirstBuild()
        {
            if (DatabaseExists == false)
            {
                return;
            }

            instance.stripAllShadersForFirstBuild = false;
            instance.SetDirtyAndSave();
        }

        public static void RefreshShaderVariants()
        {
            if (DatabaseExists == false)
            {
                return;
            }

            Undo.RecordObject(instance, $"Refreshing shader variants for all Shaders to Strip.");

            foreach (var _shader in instance.AllShaders)
            {
                _shader.UpdateVariantsCollection(false);
            }
        }

        public static Color GetIndicatorColor(ShaderToStrip _shader)
        {
            if (DatabaseExists == false)
            {
                return Color.clear;
            }

            if (_shader.IsStripped)
            {
                return instance.StrippedShaderColor;
            }

            if (_shader.CompileOnlySpecifiedVariants && _shader.NumberOfKeywords > 0 && _shader.AvailableVariants.IsNullOrEmpty() == false)
            {
                return instance.CompileOnlySpecifiedColor;
            }

            bool _allKeywordsAreStripped = _shader.AvailableKeywords.Count > 0 && _shader.GetNumberOfStrippedKeywords() == _shader.AvailableKeywords.Count;

            if (_allKeywordsAreStripped && _shader.CanCompileOnlySpecifiedVariants == false)
            {
                return instance.StrippedShaderColor;
            }

            return _shader.IsAnyKeywordStripped ? instance.SomeKeywordsStrippedColor : instance.NotStrippedShaderColor;
        }

        public static Color GetIndicatorColor(Keyword _keyword)
        {
            if (DatabaseExists == false)
            {
                return Color.clear;
            }

            if (_keyword.StrippedGloballyOrLocally)
            {
                return instance.StrippedShaderColor;
            }

            return instance.NotStrippedShaderColor;
        }

        public static bool IsKeywordGloballyStripped(string _keyword)
        {
            if (DatabaseExists == false)
            {
                return false;
            }

            return instance.globallyStrippedKeywords.Contains(_keyword);
        }

        public static bool AddToGloballyStripped(string _keyword)
        {
            bool _added = instance.globallyStrippedKeywords.AddUnique(_keyword);
            instance.SetAsDirty();

            return _added;
        }

        public static void AddToUniqueKeywords(string _keyword)
        {
            if (DatabaseExists == false)
            {
                return;
            }

            instance.uniqueKeywords.AddUnique(_keyword);
            instance.SetAsDirty();
        }

        public static void RemoveFromGloballyStripped(string _keyword)
        {
            if (DatabaseExists == false)
            {
                return;
            }

            for (int i = instance.globallyStrippedKeywords.Count - 1; i >= 0; i--)
            {
                if (instance.globallyStrippedKeywords[i] == _keyword)
                {
                    instance.globallyStrippedKeywords.RemoveAt(i);
                    instance.SetAsDirty();
                    return;
                }
            }
        }

        public static KeywordDescription GetKeywordDescription(string _keyword)
        {
            if (DatabaseExists == false)
            {
                return null;
            }

            return instance.getKeywordDescription(_keyword);
        }

        public static void RegisterToShaderKeywordsDatabase(KeywordDescription _description)
        {
            if (DatabaseExists == false)
            {
                return;
            }

            instance.registerToKeywordsDatabase(_description);
        }

        public static void DeleteBuildCache(bool _withShaders = false)
        {
            string _buildCachePath = Path.Combine(Application.dataPath, "../Library/BuildCache");
            string _buildPlayerDataPath = Path.Combine(Application.dataPath, "../Library/BuildPlayerData");
            string _playerDataCachePath = Path.Combine(Application.dataPath, "../Library/PlayerDataCache");
            string _splashScreenCachePath = Path.Combine(Application.dataPath, "../Library/SplashScreenCache");

            deleteIfExists(_buildCachePath);
            deleteIfExists(_buildPlayerDataPath);
            deleteIfExists(_playerDataCachePath);
            deleteIfExists(_splashScreenCachePath);

            if (_withShaders) //We don't want to delete EditorEncounteredVariants so we delete folder by folder
            {
                string _shaderCachePath = string.Empty;

                _shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache/builtin");
                deleteIfExists(_shaderCachePath);
                _shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache/shader");
                deleteIfExists(_shaderCachePath);
                _shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache/shadergraph");
                deleteIfExists(_shaderCachePath);
                _shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache/temp");
                deleteIfExists(_shaderCachePath);
                _shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache/vfx");
                deleteIfExists(_shaderCachePath);

                if (ShouldDeleteComputeShadersCache)
                {
                    _shaderCachePath = Path.Combine(Application.dataPath, "../Library/ShaderCache/compute");
                    deleteIfExists(_shaderCachePath);
                }
            }
        }

        public static void DeleteSingleShaderCache(ShaderToStrip _shader)
        {
            string _shaderGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_shader.ShaderReference)).Substring(0, 4);
            string _folderName = $"{_shader.ShortName.RemoveSpaces()}{_shaderGUID}";
            string _shaderCachePath = Path.Combine(Application.dataPath, $"../Library/ShaderCache/shader/{_folderName}");

            deleteIfExists(_shaderCachePath);

            string _assetPath = AssetDatabase.GetAssetPath(_shader.ShaderReference);

            if (_assetPath.EndsWith("builtin_extra") == false) // Built in shaders from the old pipeline can't be reimported
            {
                AssetDatabase.ImportAsset(_assetPath);
            }

            ShadersLimiter.Log($"Deleted cache of {_shader.ShortName}.");
        }

        private static void deleteIfExists(string _path)
        {
            if (Directory.Exists(_path))
            {
                Directory.Delete(_path, true);
            }
        }
    }
}