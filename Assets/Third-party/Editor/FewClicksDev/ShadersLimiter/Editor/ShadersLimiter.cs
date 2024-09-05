namespace FewClicksDev.ShadersLimiter
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    using static UnityEngine.ShaderVariantCollection;

    [System.Flags]
    public enum ShaderType
    {
        None = 0,
        Default = 1,
        Hidden = 2,
        Legacy = 4,
        Stripped = 8
    }

    public enum ShadersDisplayOrder
    {
        NumberOfKeywords = 0,
        NumberOfStrippedKeywords = 1
    }

    public static class ShadersLimiter
    {
        public const string SHADERS_LIMITER_NAME = "SHADERS LIMITER";
        public const string VERSION = "1.2.0";

        public static readonly Color MAIN_COLOR = new Color(0.1850303f, 0.3962264f, 0.2129241f, 1f);
        public static readonly Color LOGS_COLOR = new Color(0.4580367f, 0.6886792f, 0.4641453f, 1f);
        public static readonly Color SUBCATEGORY_TOOLBAR_COLOR = new Color(0.09211463f, 0.2830189f, 0.1193867f, 1f);

        private static ShadersLimiterDatabase database = null;

        [MenuItem("Assets/FewClicks Dev/Edit Shader Variants Collection", priority = 2005)]
        public static void OpenCollection()
        {
            VariantsCollectionEditorWindow.OpenWindow(Selection.activeObject as ShaderVariantCollection);
        }

        [MenuItem("Assets/FewClicks Dev/Edit Shader Variants Collection", priority = 2005, validate = true)]
        public static bool OpenCollectionValidation()
        {
            if (Selection.activeObject == null)
            {
                return false;
            }

            return Selection.activeObject.GetType() == typeof(ShaderVariantCollection);
        }

        public static void Log(string _message)
        {
            if (ShadersLimiterDatabase.UsageLogsEnabled == false)
            {
                return;
            }

            BaseLogger.Log(SHADERS_LIMITER_NAME, _message, LOGS_COLOR);
        }

        public static void Warning(string _message)
        {
            if (ShadersLimiterDatabase.UsageLogsEnabled == false)
            {
                return;
            }

            BaseLogger.Warning(SHADERS_LIMITER_NAME, _message, LOGS_COLOR);
        }

        public static void Error(string _message)
        {
            if (ShadersLimiterDatabase.UsageLogsEnabled == false)
            {
                return;
            }

            BaseLogger.Error(SHADERS_LIMITER_NAME, _message, LOGS_COLOR);
        }

        public static void Log(string _message, bool _condition)
        {
            if (_condition == false)
            {
                return;
            }

            BaseLogger.Log(SHADERS_LIMITER_NAME, _message, LOGS_COLOR);
        }

        public static void DeleteCache(this Shader _shader)
        {
            if (_shader == null)
            {
                return;
            }

            if (ShadersLimiterDatabase.DatabaseExists == false)
            {
                Error("Can't delete shader cache, database doesn't exist.");
                return;
            }

            ShaderToStrip _currentShader = ShadersLimiterDatabase.GetShaderToStrip(_shader);

            if (_currentShader == null)
            {
                Error("Can't delete shader cache, because it hasn't been found in the database.");
                return;
            }

            ShadersLimiterDatabase.DeleteSingleShaderCache(_currentShader);
        }

        public static bool IsVisible(this ShaderToStrip _shader)
        {
            findInstance();

            if (database == null)
            {
                return false;
            }

            if (database.Visibility is ShaderType.None)
            {
                return false;
            }

            if (database.Visibility.ContainsFlag(ShaderType.Stripped))
            {
                if (_shader.IsStripped || (_shader.AllKeywordsStripped && _shader.CanCompileOnlySpecifiedVariants == false))
                {
                    return true;
                }
            }

            if (database.Visibility.ContainsFlag(ShaderType.Stripped) == false)
            {
                if (_shader.IsStripped || (_shader.AllKeywordsStripped && _shader.CanCompileOnlySpecifiedVariants == false))
                {
                    return false;
                }
            }

            return database.Visibility.ContainsFlag(_shader.CurrentShaderType);
        }

        public static bool Contains(this ShaderKeyword[] _keywordsCollection, Keyword _keywordToStrip, Shader _shaderReference)
        {
            foreach (var _keyword in _keywordsCollection)
            {
                if (_keyword.GetNameWithShader(_shaderReference).Equals(_keywordToStrip.KeywordName))
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetKeywordsAsString(this ShaderKeyword[] _keywords, Shader _shaderReference)
        {
            string _keywordsString = string.Empty;

            for (int i = 0; i < _keywords.Length; i++)
            {
                _keywordsString += _keywords[i].GetNameWithShader(_shaderReference);

                if (i < _keywords.Length - 1)
                {
                    _keywordsString += ", ";
                }
            }

            return _keywordsString;
        }

        public static string GetNameWithShader(this ShaderKeyword _keyword, Shader _shaderReference)
        {
#if UNITY_2021_2_OR_NEWER
            return _keyword.name;
#else
			return ShaderKeyword.GetKeywordName(_shaderReference, _keyword);
#endif
        }

        public static List<Shader> GetShadersInCollection(ShaderVariantCollection _collection)
        {
            if (_collection == null)
            {
                return null;
            }

            List<Shader> _list = new List<Shader>();

            SerializedObject _variantsObject = new SerializedObject(_collection);
            SerializedProperty _allShadersProperty = _variantsObject.FindProperty("m_Shaders");

            for (int i = 0; i < _allShadersProperty.arraySize; i++)
            {
                SerializedProperty _shaderVariantProperty = _allShadersProperty.GetArrayElementAtIndex(i);
                SerializedProperty _shaderProperty = _shaderVariantProperty.FindPropertyRelative("first");

                Shader _shader = _shaderProperty.objectReferenceValue as Shader;
                _list.AddUnique(_shader);
            }

            return _list;
        }

        public static List<CompiledShaderVariant> GetShaderVariantsInCollection(ShaderVariantCollection _collection, Shader _shaderReference)
        {
            List<CompiledShaderVariant> _list = new List<CompiledShaderVariant>();

            SerializedObject _variantsObject = new SerializedObject(_collection);
            SerializedProperty _allShadersProperty = _variantsObject.FindProperty("m_Shaders");

            for (int i = 0; i < _allShadersProperty.arraySize; i++)
            {
                SerializedProperty _shaderVariantProperty = _allShadersProperty.GetArrayElementAtIndex(i);
                SerializedProperty _shaderProperty = _shaderVariantProperty.FindPropertyRelative("first");

                Shader _shader = _shaderProperty.objectReferenceValue as Shader;

                if (_shaderReference != null && _shader != _shaderReference) //Optional filtering by shader reference
                {
                    continue;
                }

                SerializedProperty _variantsProperty = _shaderVariantProperty.FindPropertyRelative("second").FindPropertyRelative("variants");

                for (int j = 0; j < _variantsProperty.arraySize; j++)
                {
                    SerializedProperty _currentVariantProperty = _variantsProperty.GetArrayElementAtIndex(j);

                    PassType _passType = (PassType) _currentVariantProperty.FindPropertyRelative("passType").intValue;
                    string[] _keywords = _currentVariantProperty.FindPropertyRelative("keywords").stringValue.Split(' ');

                    ShaderVariant _variant = new ShaderVariant();
                    _variant.passType = _passType;
                    _variant.keywords = _keywords;

                    _list.Add(new CompiledShaderVariant(_shader, _variant));
                    ShadersLimiterDatabase.SetAsDirty();
                }
            }

            return _list;
        }

        private static void findInstance()
        {
            if (database == null)
            {
                database = AssetsUtilities.GetScriptableOfType<ShadersLimiterDatabase>();
            }
        }
    }
}
