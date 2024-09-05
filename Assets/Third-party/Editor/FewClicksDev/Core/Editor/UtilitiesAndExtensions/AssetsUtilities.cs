namespace FewClicksDev.Core
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Helper class for assets management
    /// </summary>
    public static class AssetsUtilities
    {
        private const string UNITY_ENGINE_TYPE = "UnityEngine.";
        private const string UNITY_EDITOR_TYPE = "UnityEditor.";
        private const string ASSETS_FOLDER = "Assets/";

        /// <summary>
        /// Returns the first found scriptable of type T with the given name
        /// </summary>
        public static T GetScriptableOfType<T>() where T : ScriptableObject
        {
            return GetAssetOfType<T>(string.Empty);
        }

        /// <summary>
        /// Returns the first asset of type T with the given name
        /// </summary>
        public static T GetAssetOfType<T>(string _name) where T : Object
        {
            string _type = $"{typeof(T)}";

            if (_type.StartsWith(UNITY_ENGINE_TYPE))
            {
                _type = _type.TrimStart(UNITY_ENGINE_TYPE);
            }

            if (_type.StartsWith(UNITY_EDITOR_TYPE))
            {
                _type = _type.TrimStart(UNITY_EDITOR_TYPE);
            }

            string _searchFilter = $"t:{_type} {_name}";
            string[] _guids = AssetDatabase.FindAssets(_searchFilter);

            if (_guids.Length > 0)
            {
                string _assetPath = AssetDatabase.GUIDToAssetPath(_guids[0]);
                return AssetDatabase.LoadAssetAtPath<T>(_assetPath);
            }

            return null;
        }

        public static T[] GetAssetsOfType<T>(string _name) where T : Object
        {
            string _type = $"{typeof(T)}";

            if (_type.StartsWith(UNITY_ENGINE_TYPE))
            {
                _type = _type.TrimStart(UNITY_ENGINE_TYPE);
            }

            string _searchFilter = $"t:{_type} {_name}";
            string[] _guids = AssetDatabase.FindAssets(_searchFilter);

            T[] _assets = new T[_guids.Length];
            int _index = 0;

            foreach (var _guid in _guids)
            {
                string _assetPath = AssetDatabase.GUIDToAssetPath(_guid);
                _assets[_index] = AssetDatabase.LoadAssetAtPath<T>(_assetPath);

                _index++;
            }

            return _assets;
        }

        /// <summary>
        /// Load an asset from GUID
        /// </summary>
        public static T LoadAsset<T>(string _guid) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(_guid));
        }

        public static void SetAsDirty(this Object _object)
        {
            EditorUtility.SetDirty(_object);
        }

        /// <summary>
        /// Setting the dirty flag and saving the asset
        /// </summary>
        public static void SetDirtyAndSave(this Object _object)
        {
            EditorUtility.SetDirty(_object);
            AssetDatabase.SaveAssetIfDirty(_object);
        }

        /// <summary>
        /// Focusing on the Project window and pinging the object
        /// </summary>
        public static void Ping(Object _object)
        {
            if (_object == null)
            {
                return;
            }

            Selection.activeObject = _object;

            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(_object);
        }

        /// <summary>
        /// Returns asset path
        /// </summary>
        public static string GetAssetPath(this Object _object)
        {
            return AssetDatabase.GetAssetPath(_object);
        }

        /// <summary>
        /// Returns asset GUID
        /// </summary>
        public static string GetAssetGUID(this Object _object)
        {
            return AssetDatabase.AssetPathToGUID(_object.GetAssetPath());
        }

        /// <summary>
        /// Return folder path of the asset
        /// </summary>
        public static string GetFolderPath(this Object _object)
        {
            string _assetPath = AssetDatabase.GetAssetPath(_object);
            string _assetName = Path.GetFileName(_assetPath);
            int _length = _assetPath.Length - (_assetName.Length + 1);

            return _assetPath.Substring(0, _length);
        }

        public static string ConvertAbsolutePathToDataPath(string _path)
        {
            if (_path.StartsWith(Application.dataPath))
            {
                _path = _path.TrimStart(Application.dataPath);
            }

            string _finalPath = string.Empty;

            if (_path.StartsWith(ASSETS_FOLDER) == false)
            {
                _finalPath += ASSETS_FOLDER;
            }

            return _finalPath + _path;
        }

        /// <summary>
        /// Converts absolute file path to the one that can be used in AssetDatabase
        /// </summary>
        /// <param name="_path">Absolute asset path</param>
        /// <param name="_fileName">File name</param>
        public static string ConvertAbsolutePathToDataPath(string _path, string _fileName)
        {
            if (_path.StartsWith(Application.dataPath))
            {
                _path = _path.TrimStart(Application.dataPath);
            }

            string _finalPath = string.Empty;

            if (_path.StartsWith(ASSETS_FOLDER) == false)
            {
                _finalPath += ASSETS_FOLDER;
            }

            _finalPath += $"{_path}/{_fileName}";

            return _finalPath;
        }
    }
}