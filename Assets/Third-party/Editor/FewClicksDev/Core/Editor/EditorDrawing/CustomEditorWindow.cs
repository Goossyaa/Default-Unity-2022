namespace FewClicksDev.Core
{
    using UnityEditor;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;

    /// <summary>
    /// Base class for unified editor windows
    /// </summary>
    public abstract class CustomEditorWindow : EditorWindow, IHasCustomMenu
    {
        protected const float VERTICAL_SLIDER_WIDTH = 14f;
        protected const float FOOTER_HEIGHT = 54f;

        protected abstract string windowName { get; }
        protected abstract string version { get; }
        protected abstract Vector2 minWindowSize { get; }
        protected abstract Color mainColor { get; }

        protected virtual bool debugModeEnabled { get; } = false;

        protected virtual bool askForReview { get; } = false;
        protected virtual string reviewURL { get; } = string.Empty;

        protected virtual bool hasDocumentation { get; } = false;
        protected virtual string documentationURL { get; } = string.Empty;

        protected virtual float leftPadding => LARGE_SPACE;
        protected virtual float rightPadding => LARGE_SPACE;

        protected Vector2 scrollPosition = Vector2.zero;
        protected Color defaultGUIColor = Color.white;

        protected float windowWidth => position.width;
        protected float windowHeight => position.height;

        protected bool isVerticalSliderVisible { get; private set; }
        protected float verticalSliderWidth => isVerticalSliderVisible ? VERTICAL_SLIDER_WIDTH : 0f;

        protected float windowWidthWithLeftPadding => windowWidth - leftPadding;
        protected float windowWidthWithRightPadding => windowWidth - rightPadding;
        protected float windowWidthWithPaddings => windowWidth - leftPadding - rightPadding - verticalSliderWidth;

        protected float halfSizeButtonWidth => (windowWidthWithPaddings - LARGE_SPACE) / 2f;
        protected float thirdSizeButtonWidth => ((windowWidthWithPaddings - (2 * LARGE_SPACE)) / 3f) + 1f;
        protected float wholeSizeButtonWidth => windowWidthWithPaddings - 4f;

        protected float windowWidthScaled(float _percentage) => windowWidth * Mathf.Clamp01(_percentage);

        /// <summary>
        /// Simple way to add context options to the window. It's called by the Unity Editor
        /// </summary>
        public virtual void AddItemsToMenu(GenericMenu _menu)
        {
            if (debugModeEnabled)
            {
                GUIContent _printWindowSizeContent = new GUIContent("Print window size");
                _menu.AddItem(_printWindowSizeContent, false, _printWindowSize);
                _menu.AddSeparator(string.Empty);
            }

            if (askForReview)
            {
                GUIContent _reviewContent = new GUIContent("Leave a Review");
                _menu.AddItem(_reviewContent, false, _openReview);
                _menu.AddSeparator(string.Empty);
            }

            if (hasDocumentation)
            {
                GUIContent _documentationContent = new GUIContent("Open Documentation");
                _menu.AddItem(_documentationContent, false, _openDocumentation);
            }

            void _printWindowSize()
            {
                BaseLogger.Log("CUSTOM EDITOR WINDOW", $"Window size: {windowWidth} x {windowHeight}", Color.white);
            }

            void _openReview()
            {
                Application.OpenURL(reviewURL);
            }

            void _openDocumentation()
            {
                Application.OpenURL(documentationURL);
            }
        }

        protected virtual void OnEnable()
        {
            minSize = minWindowSize;
            titleContent = new GUIContent(windowName);
        }

        protected virtual void Update()
        {
            Repaint();
        }

        protected virtual void OnGUI()
        {
            try
            {
                defaultGUIColor = GUI.color;

                using (var _scrollScope = new ScrollViewScope(scrollPosition))
                {
                    scrollPosition = _scrollScope.scrollPosition;

                    using (new HorizontalScope())
                    {
                        Space(leftPadding);

                        using (new VerticalScope())
                        {
                            drawWindowGUI();
                        }

                        Space(rightPadding - verticalSliderWidth);
                    }

                    checkForVisibleSlider();
                }

                drawWindowFooter();
            }
            catch (System.ArgumentException) { }
        }

        protected abstract void drawWindowGUI();

        protected virtual void drawWindowFooter()
        {
            DrawLine();

            NormalSpace();
            DrawCenteredBoldLabel($"{windowName} :: v{version}");
            LargeSpace();
        }

        private void checkForVisibleSlider()
        {
            Rect _rect = GUILayoutUtility.GetLastRect();

            if (_rect.height > 1)
            {
                if (_rect.height + FOOTER_HEIGHT > windowHeight)
                {
                    isVerticalSliderVisible = true;
                }
                else
                {
                    isVerticalSliderVisible = false;
                }
            }
        }
    }
}