namespace FewClicksDev.Core
{
    using UnityEditor;
    using UnityEngine;

    public static class PreferencesExtensions
    {
        private const int NUMBER_OF_CHANNELS_IN_COLOR = 4;

        public static Color LoadColor(string _name, Color _value)
        {
            Color _color = Color.white;
            string _colorString = EditorPrefs.GetString(_name, GetStringFromColor(_value));
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
    }
}