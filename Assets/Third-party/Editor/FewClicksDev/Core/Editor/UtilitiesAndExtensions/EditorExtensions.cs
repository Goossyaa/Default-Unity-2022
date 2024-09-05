namespace FewClicksDev.Core
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public static class EditorExtensions
    {
        private const int NUMBER_OF_CHANNELS_IN_COLOR = 4;

        public static void SetSearchString(string _searchString)
        {
            EditorUtility.FocusProjectWindow();

            Type _projectBrowserType = Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
            EditorWindow _window = EditorWindow.GetWindow(_projectBrowserType);

            MethodInfo _setSearchMethodInfo = _projectBrowserType.GetMethod("SetSearch", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, new Type[] { typeof(string) }, null);

            _setSearchMethodInfo.Invoke(_window, new object[] { _searchString });
        }

        public static Color LoadColor(string _name, Color _defaultValue)
        {
            Color _color = Color.white;
            string _colorString = EditorPrefs.GetString(_name, GetStringFromColor(_defaultValue));
            string[] _colorElements = _colorString.Split(':');

            if (_colorElements.Length < NUMBER_OF_CHANNELS_IN_COLOR)
            {
                return _color;
            }

            float.TryParse(_colorElements[0], out float _r);
            float.TryParse(_colorElements[1], out float _g);
            float.TryParse(_colorElements[2], out float _b);
            float.TryParse(_colorElements[3], out float _a);

            _color = new Color(_r, _g, _b, _a);

            return _color;
        }

        public static string GetStringFromColor(Color _color)
        {
            return $"{_color.r}:{_color.g}:{_color.b}:{_color.a}";
        }

        public static void ShowNotification(string _message)
        {
            EditorWindow _window = EditorWindow.focusedWindow;

            if (_window == null)
            {
                return;
            }

            _window.ShowNotification(new GUIContent(_message));
        }
    }
}