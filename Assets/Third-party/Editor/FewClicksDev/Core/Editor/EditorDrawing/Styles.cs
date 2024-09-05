namespace FewClicksDev.Core
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class Styles
    {
        private static GUIStyle buttonStyle = null;
        private static GUIStyle lightButtonStyle = null;
        private static GUIStyle clearBoxStyle = null;
        private static GUIStyle settingsButtonStyle = null;
        private static GUIStyle documentationButtonStyle = null;
        private static GUIStyle toggleStyle = null;
        private static GUIStyle closeStyle = null;
        private static GUIStyle selectStyle = null;
        private static GUIStyle inspectStyle = null;
        private static GUIStyle sortOrderStyle = null;

        private static Dictionary<float, GUIStyle> fixedToggleStyles = new Dictionary<float, GUIStyle>();
        private static Dictionary<float, GUIStyle> fixedSettingsStyles = new Dictionary<float, GUIStyle>();
        private static Dictionary<float, GUIStyle> fixedCloseStyles = new Dictionary<float, GUIStyle>();
        private static Dictionary<float, GUIStyle> fixedSelectStyles = new Dictionary<float, GUIStyle>();
        private static Dictionary<float, GUIStyle> fixedZoomStyles = new Dictionary<float, GUIStyle>();
        private static Dictionary<float, GUIStyle> fixedSortOrderStyles = new Dictionary<float, GUIStyle>();

        public static GUIStyle BoxButton
        {
            get
            {
                if (buttonStyle == null)
                {
                    buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
                    buttonStyle.SetBackgroundForAllStates(IconsAndTextures.ButtonBackground);

                    buttonStyle.richText = true;
                    buttonStyle.alignment = TextAnchor.MiddleCenter;
                    buttonStyle.fontSize = 12;
                    buttonStyle.fixedHeight = 0;
                    buttonStyle.clipping = TextClipping.Clip;
                    buttonStyle.border = new RectOffset(2, 2, 2, 2);
                    buttonStyle.padding = new RectOffset(0, 0, 0, 0);
                    buttonStyle.margin = new RectOffset(1, 0, 0, 0);

                    buttonStyle.SetBackgroundForActiveAndHover(IconsAndTextures.ButtonActiveBackground);
                }

                return buttonStyle;
            }
        }

        public static GUIStyle LightButton
        {
            get
            {
                if (lightButtonStyle == null)
                {
                    lightButtonStyle = new GUIStyle(BoxButton);
                    lightButtonStyle.SetBackgroundForAllStates(IconsAndTextures.ButtonActiveBackground);
                }

                return lightButtonStyle;
            }
        }

        public static GUIStyle ClearBox
        {
            get
            {
                if (clearBoxStyle == null)
                {
                    clearBoxStyle = new GUIStyle(BoxButton);
                    clearBoxStyle.SetBackgroundForAllStates(IconsAndTextures.ClearBoxBackground);
                }

                return clearBoxStyle;
            }
        }

        public static GUIStyle SettingsButton
        {
            get
            {
                if (settingsButtonStyle == null)
                {
                    settingsButtonStyle = new GUIStyle(BoxButton);
                    settingsButtonStyle.SetBackgroundForAllStates(IconsAndTextures.SettingsBackground);
                    settingsButtonStyle.SetBackgroundForActiveAndHover(IconsAndTextures.SettingsActiveBackground);
                }

                return settingsButtonStyle;
            }
        }

        public static GUIStyle DocumentationButton
        {
            get
            {
                if (documentationButtonStyle == null)
                {
                    documentationButtonStyle = new GUIStyle(BoxButton);
                    documentationButtonStyle.SetBackgroundForAllStates(IconsAndTextures.DocumentationBackground);
                }

                return documentationButtonStyle;
            }
        }

        public static GUIStyle Toggle
        {
            get
            {
                if (toggleStyle == null)
                {
                    toggleStyle = new GUIStyle(BoxButton);

                    toggleStyle.normal.background = IconsAndTextures.ToggleOffBackground;
                    toggleStyle.onNormal.background = IconsAndTextures.ToggleOnBackground;
                    toggleStyle.focused.background = IconsAndTextures.ToggleOffBackground;
                    toggleStyle.onFocused.background = IconsAndTextures.ToggleOnBackground;
                    toggleStyle.active.background = IconsAndTextures.ToggleOnBackground;
                    toggleStyle.onActive.background = IconsAndTextures.ToggleOnActiveBackground;
                    toggleStyle.hover.background = IconsAndTextures.ToggleOffActiveBackground;
                    toggleStyle.onHover.background = IconsAndTextures.ToggleOnActiveBackground;
                }

                return toggleStyle;
            }
        }

        public static GUIStyle Close
        {
            get
            {
                if (closeStyle == null)
                {
                    closeStyle = new GUIStyle(BoxButton);
                    closeStyle.SetBackgroundForAllStates(IconsAndTextures.CloseBackground);
                    closeStyle.SetBackgroundForActiveAndHover(IconsAndTextures.CloseActiveBackground);
                }

                return closeStyle;
            }
        }

        public static GUIStyle Select
        {
            get
            {
                if (selectStyle == null)
                {
                    selectStyle = new GUIStyle(BoxButton);
                    selectStyle.SetBackgroundForAllStates(IconsAndTextures.SelectBackground);
                    selectStyle.SetBackgroundForActiveAndHover(IconsAndTextures.SelectActiveBackground);
                }

                return selectStyle;
            }
        }

        public static GUIStyle Inspect
        {
            get
            {
                if (inspectStyle == null)
                {
                    inspectStyle = new GUIStyle(BoxButton);
                    inspectStyle.SetBackgroundForAllStates(IconsAndTextures.InspectBackground);
                    inspectStyle.SetBackgroundForActiveAndHover(IconsAndTextures.InspectActiveBackground);
                }

                return inspectStyle;
            }
        }

        public static GUIStyle SortOrder
        {
            get
            {
                if (sortOrderStyle == null)
                {
                    sortOrderStyle = new GUIStyle(BoxButton);

                    sortOrderStyle.normal.background = IconsAndTextures.AscendingBackground;
                    sortOrderStyle.onNormal.background = IconsAndTextures.DescendingBackground;
                    sortOrderStyle.focused.background = IconsAndTextures.AscendingBackground;
                    sortOrderStyle.onFocused.background = IconsAndTextures.DescendingBackground;
                    sortOrderStyle.active.background = IconsAndTextures.DescendingBackground;
                    sortOrderStyle.onActive.background = IconsAndTextures.DescendingActiveBackground;
                    sortOrderStyle.hover.background = IconsAndTextures.AscendingActiveBackground;
                    sortOrderStyle.onHover.background = IconsAndTextures.DescendingActiveBackground;
                }

                return sortOrderStyle;
            }
        }

        public static GUIStyle WithMargin(this GUIStyle _style, RectOffset _margin)
        {
            GUIStyle _newStyle = new GUIStyle(_style);
            _newStyle.margin = _margin;

            return _newStyle;
        }

        public static GUIStyle WithLeftMargin(this GUIStyle _style, int _leftOffset)
        {
            GUIStyle _newStyle = new GUIStyle(_style);
            _newStyle.margin = new RectOffset(_leftOffset, 0, 0, 0);

            return _newStyle;
        }

        public static GUIStyle WithColor(this GUIStyle _style, Color _color)
        {
            GUIStyle _newStyle = new GUIStyle(_style);
            _newStyle.SetTextColorForAllStates(_color);

            return _newStyle;
        }

        public static GUIStyle WithBorder(this GUIStyle _style, RectOffset _border)
        {
            GUIStyle _newStyle = new GUIStyle(_style);
            _newStyle.border = _border;

            return _newStyle;
        }

        public static GUIStyle WithFontSize(this GUIStyle _style, int _fontSize)
        {
            GUIStyle _newStyle = new GUIStyle(_style);
            _newStyle.fontSize = _fontSize;

            return _newStyle;
        }

        public static GUIStyle CustomizedButton(float _fixedHeight, TextAnchor _textAnchor, RectOffset _padding)
        {
            GUIStyle _style = new GUIStyle(BoxButton);
            _style.fixedHeight = _fixedHeight;
            _style.alignment = _textAnchor;
            _style.padding = _padding;

            return _style;
        }

        public static GUIStyle FixedToggle(float _width)
        {
            if (fixedToggleStyles.ContainsKey(_width) == false)
            {
                GUIStyle _style = new GUIStyle(Toggle);
                _style.fixedWidth = _width;
                _style.fixedHeight = _width;

                fixedToggleStyles.Add(_width, _style);
            }

            return fixedToggleStyles[_width];
        }

        public static GUIStyle FixedSettings(float _width)
        {
            if (fixedSettingsStyles.ContainsKey(_width) == false)
            {
                GUIStyle _style = new GUIStyle(SettingsButton);
                _style.fixedWidth = _width;
                _style.fixedHeight = _width;

                fixedSettingsStyles.Add(_width, _style);
            }

            return fixedSettingsStyles[_width];
        }

        public static GUIStyle FixedClose(float _width)
        {
            if (fixedCloseStyles.ContainsKey(_width) == false)
            {
                GUIStyle _style = new GUIStyle(Close);
                _style.fixedWidth = _width;
                _style.fixedHeight = _width;

                fixedCloseStyles.Add(_width, _style);
            }

            return fixedCloseStyles[_width];
        }

        public static GUIStyle FixedSelect(float _width)
        {
            if (fixedSelectStyles.ContainsKey(_width) == false)
            {
                GUIStyle _style = new GUIStyle(Select);
                _style.fixedWidth = _width;
                _style.fixedHeight = _width;

                fixedSelectStyles.Add(_width, _style);
            }

            return fixedSelectStyles[_width];
        }

        public static GUIStyle FixedZoom(float _width)
        {
            if (fixedZoomStyles.ContainsKey(_width) == false)
            {
                GUIStyle _style = new GUIStyle(Inspect);
                _style.fixedWidth = _width;
                _style.fixedHeight = _width;

                fixedZoomStyles.Add(_width, _style);
            }

            return fixedZoomStyles[_width];
        }

        public static GUIStyle FixedSortOrder(float _width)
        {
            if (fixedSortOrderStyles.ContainsKey(_width) == false)
            {
                GUIStyle _style = new GUIStyle(SortOrder);
                _style.fixedWidth = _width - 2f;
                _style.fixedHeight = _width - 2f;
                _style.margin = new RectOffset(0, -2, 2, 0);

                fixedSortOrderStyles.Add(_width, _style);
            }

            return fixedSortOrderStyles[_width];
        }
    }

    public static class StylesExtensions
    {
        public static void SetBackgroundForAllStates(this GUIStyle _style, Texture2D _background)
        {
            _style.normal.background = _background;
            _style.onNormal.background = _background;
            _style.focused.background = _background;
            _style.onFocused.background = _background;
            _style.active.background = _background;
            _style.onActive.background = _background;
            _style.hover.background = _background;
            _style.onHover.background = _background;
        }

        public static void SetBackgroundForActiveAndHover(this GUIStyle _style, Texture2D _background)
        {
            _style.active.background = _background;
            _style.onActive.background = _background;
            _style.hover.background = _background;
            _style.onHover.background = _background;
        }

        public static void SetTextColorForAllStates(this GUIStyle _style, Color _color)
        {
            _style.normal.textColor = _color;
            _style.onNormal.textColor = _color;
            _style.focused.textColor = _color;
            _style.onFocused.textColor = _color;
            _style.active.textColor = _color;
            _style.onActive.textColor = _color;
            _style.hover.textColor = _color;
            _style.onHover.textColor = _color;
        }
    }
}