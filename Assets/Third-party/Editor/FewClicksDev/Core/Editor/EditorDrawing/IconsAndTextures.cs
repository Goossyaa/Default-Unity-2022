namespace FewClicksDev.Core
{
    using UnityEditor;
    using UnityEngine;

    public static class IconsAndTextures
    {
        // UNITY
        public static Texture2D Zoom => EditorGUIUtility.IconContent("d_ViewToolZoom@2x").image as Texture2D;
        public static Texture2D VisibilityOn => EditorGUIUtility.IconContent("d_scenevis_visible_hover@2x").image as Texture2D;
        public static Texture2D VisibilityOff => EditorGUIUtility.IconContent("d_SceneViewVisibility@2x").image as Texture2D;
        public static Texture2D PickableOn => EditorGUIUtility.IconContent("d_scenepicking_pickable_hover@2x").image as Texture2D;
        public static Texture2D PickableOff => EditorGUIUtility.IconContent("d_scenepicking_notpickable_hover@2x").image as Texture2D;

        // CUSTOM
        private static Texture2D buttonBackground = null;
        private static Texture2D buttonActiveBackground = null;
        private static Texture2D clearBoxBackground = null;
        private static Texture2D toggleOnBackground = null;
        private static Texture2D toggleOnActiveBackground = null;
        private static Texture2D toggleOffBackground = null;
        private static Texture2D toggleOffActiveBackground = null;
        private static Texture2D settingsBackground = null;
        private static Texture2D settingsActiveBackground = null;
        private static Texture2D documentationBackground = null;
        private static Texture2D closeBackground = null;
        private static Texture2D closeActiveBackground = null;
        private static Texture2D selectBackground = null;
        private static Texture2D selectActiveBackground = null;
        private static Texture2D inspectBackground = null;
        private static Texture2D inspectActiveBackground = null;
        private static Texture2D ascendingBackground = null;
        private static Texture2D ascendingActiveBackground = null;
        private static Texture2D descendingBackground = null;
        private static Texture2D descendingActiveBackground = null;

        public static Texture2D ButtonBackground => findAndReturnIcon(ref buttonBackground, "sp_Button");
        public static Texture2D ButtonActiveBackground => findAndReturnIcon(ref buttonActiveBackground, "sp_Button_Active");
        public static Texture2D ClearBoxBackground => findAndReturnIcon(ref clearBoxBackground, "sp_ClearBox");
        public static Texture2D ToggleOnBackground => findAndReturnIcon(ref toggleOnBackground, "sp_ToggleOn");
        public static Texture2D ToggleOnActiveBackground => findAndReturnIcon(ref toggleOnActiveBackground, "sp_ToggleOn_Active");
        public static Texture2D ToggleOffBackground => findAndReturnIcon(ref toggleOffBackground, "sp_ToggleOff");
        public static Texture2D ToggleOffActiveBackground => findAndReturnIcon(ref toggleOffActiveBackground, "sp_ToggleOff_Active");
        public static Texture2D SettingsBackground => findAndReturnIcon(ref settingsBackground, "sp_Settings");
        public static Texture2D SettingsActiveBackground => findAndReturnIcon(ref settingsActiveBackground, "sp_Settings_Active");
        public static Texture2D DocumentationBackground => findAndReturnIcon(ref documentationBackground, "sp_Documentation");
        public static Texture2D CloseBackground => findAndReturnIcon(ref closeBackground, "sp_Close");
        public static Texture2D CloseActiveBackground => findAndReturnIcon(ref closeActiveBackground, "sp_Close_Active");
        public static Texture2D SelectBackground => findAndReturnIcon(ref selectBackground, "sp_Select");
        public static Texture2D SelectActiveBackground => findAndReturnIcon(ref selectActiveBackground, "sp_Select_Active");
        public static Texture2D InspectBackground => findAndReturnIcon(ref inspectBackground, "sp_Inspect");
        public static Texture2D InspectActiveBackground => findAndReturnIcon(ref inspectActiveBackground, "sp_Inspect_Active");
        public static Texture2D AscendingBackground => findAndReturnIcon(ref ascendingBackground, "sp_Ascending");
        public static Texture2D AscendingActiveBackground => findAndReturnIcon(ref ascendingActiveBackground, "sp_Ascending_Active");
        public static Texture2D DescendingBackground => findAndReturnIcon(ref descendingBackground, "sp_Descending");
        public static Texture2D DescendingActiveBackground => findAndReturnIcon(ref descendingActiveBackground, "sp_Descending_Active");

        private static Texture2D findAndReturnIcon(ref Texture2D _icon, string _textureName)
        {
            if (_icon == null)
            {
                _icon = AssetsUtilities.GetAssetOfType<Texture2D>(_textureName);
            }

            return _icon;
        }
    }
}