namespace FewClicksDev.Core
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public static class ScriptableObjectsUtilities
    {
        private const string ASSET = ".asset";

        [MenuItem("Assets/FewClicks Dev/Create Scriptable from Script", priority = 2000)]
        public static void CreateScriptableFromScript()
        {
            string _folderPath = AssetsUtilities.GetFolderPath(Selection.activeObject);
            string _assetPath = EditorUtility.SaveFolderPanel("Choose folder to create scriptable", _folderPath, string.Empty);

            if (_assetPath.Length > 0)
            {
                string _assetName = Selection.activeObject.name;
                string _newScriptableName = $"New {_assetName}{ASSET}";
                string _fixedAssetPath = AssetsUtilities.ConvertAbsolutePathToDataPath(_assetPath, _newScriptableName);

                _fixedAssetPath = AssetDatabase.GenerateUniqueAssetPath(_fixedAssetPath);

                MonoScript _script = (MonoScript) Selection.activeObject;
                Type _scriptType = _script.GetClass();
                ScriptableObject _asset = ScriptableObject.CreateInstance(_scriptType);

                AssetDatabase.CreateAsset(_asset, _fixedAssetPath);
                AssetDatabase.Refresh();

                EditorUtility.FocusProjectWindow();
                EditorGUIUtility.PingObject(_asset);
            }
        }

        [MenuItem("Assets/FewClicks Dev/Create Scriptable from Script", true, priority = 2000)]
        public static bool CreateScriptableFromScriptValidate()
        {
            return isSelectionScriptableObject();
        }

        [MenuItem("Assets/FewClicks Dev/Find Scriptables of Type", priority = 2001)]
        public static void FindAssetsOfType()
        {
            MonoScript _script = (MonoScript) Selection.activeObject;
            Type _scriptType = _script.GetClass();
            EditorExtensions.SetSearchString($"t:{_scriptType}");
        }

        [MenuItem("Assets/FewClicks Dev/Find Scriptables of Type", true, priority = 2001)]
        public static bool FindAssetsOfTypeValidate()
        {
            return isSelectionScriptableObject();
        }

        private static bool isSelectionScriptableObject()
        {
            if (Selection.activeObject == null)
            {
                return false;
            }

            if (Selection.activeObject.GetType() != typeof(MonoScript))
            {
                return false;
            }

            MonoScript _script = (MonoScript) Selection.activeObject;
            Type _scriptClass = _script.GetClass();

            if (_scriptClass == null)
            {
                return false;
            }

            return _scriptClass.IsSubclassOf(typeof(ScriptableObject));
        }
    }
}