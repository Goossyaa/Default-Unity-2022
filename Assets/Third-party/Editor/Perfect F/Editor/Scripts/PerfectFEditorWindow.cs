using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
#if UNITY_2019_3_OR_NEWER
 using UnityEditor.Compilation;
#elif UNITY_2017_1_OR_NEWER
using System.Reflection;
#endif

namespace PerfectFNamespace
{
    [InitializeOnLoad]
    public class PerfectFEditorWindow : EditorWindow
    {
        static EditorWindow window;
        static Texture2D tLogoBackground, tLogo;


        static PerfectFSettings settings;
        void OnEnable()
        {
        }


        [MenuItem("Window/ShrinkRay Entertainment/Perfect F/Settings")]
        private static void OpenWindow()
        {
            window = GetWindow<PerfectFEditorWindow>();
            window.Show();
            window.titleContent.text = "Perfect F Settings";
        }

        [MenuItem("Window/ShrinkRay Entertainment/Perfect F/Cleanup Scene")]
        static private void RemovePF()
        {
            int i = 0;
            if (EditorUtility.DisplayDialog("Cleanup Scene", "Use this to search and remove any legacy PerfectF helper objects, only needed if you upgraded from an older version of Perfect F", "Yes, I understand!", "No!  Abort!!!"))
            {
                foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
                {
                    //Debug.Log(go.name);
                    if (go.name.Contains("PerfectF"))
                    {
                        DestroyImmediate(go);
                        i++;
                    }
                }
                EditorUtility.DisplayDialog("Cleanup Scene Complete", "Found and deleted " + i + " PerfectF Helper objects.  You should not need to run this routine on this script again.", "Great!");
            }
        }



        public static void QueueRepaint()
        {
            if (window == null) window = GetWindow<PerfectFEditorWindow>();
            window.titleContent.text = "Perfect F Settings";
        }

        static void Init()
        {
            settings = Resources.Load<PerfectFSettings>("Settings/PerfectFSettings");
            tLogoBackground = Resources.Load<Texture2D>("Textures/PerfectFBlue");
            tLogo = Resources.Load<Texture2D>("Textures/PerfectFLogo");
        }

        Vector2 scrollPos;
        //
        private void OnGUI()
        {
            if (settings == null) Init();
            if (tLogo == null) Init();
            if (window == null)
            {
                QueueRepaint();
            }

            EditorGUI.BeginChangeCheck();
            var originalColor = GUI.color;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 255);
            GUI.DrawTexture(new Rect(0, 0, window.position.width, 100), tLogoBackground);
            int x = 80, y = 80;
            GUI.DrawTexture(new Rect(window.position.width / 2 - x / 2, 50 - y / 2, x, y), tLogo);
            EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(window.position.width - 4), GUILayout.Height(window.position.height - 120));

            settings.enabled = EditorGUILayout.Toggle("Enable Perfect F", settings.enabled);
            EditorGUILayout.HelpBox("Where should the Enable/Disable Perfect F button appear?", MessageType.None);
            settings.fLocation = (PerfectFSettings.FLocation)EditorGUILayout.EnumPopup("Place F Button", settings.fLocation);
            EditorGUILayout.HelpBox("If an object is very small, Unity will do an unexpected zoom out before zooming in - instant zoom will solve this.  Use the slider below to adjust where Smooth Zooming begins.", MessageType.None);
            float prev = settings.minSmoothZoom;
            settings.minSmoothZoom = EditorGUILayout.Slider("Start Smooth Framing at:", settings.minSmoothZoom, 0.1f, 4.0f);
            if (prev != settings.minSmoothZoom) PerfectF.microFirstPress = true;
            EditorGUILayout.HelpBox("Use the F key or choose another?", MessageType.None);
            settings.fKey = (KeyCode)EditorGUILayout.EnumPopup("Which key activates?", settings.fKey);
            EditorGUILayout.HelpBox("Which keys will rotate the view?", MessageType.None);
            EditorGUILayout.HelpBox("Shift and Ctrl F were interefering with some unity functions, now use the ; and ' keys by the Enter key to rotate!", MessageType.Error);
            settings.rotLeft = (KeyCode)EditorGUILayout.EnumPopup("Rotate Left", settings.rotLeft);
            settings.rotRight = (KeyCode)EditorGUILayout.EnumPopup("Rotate Right", settings.rotRight);
            EditorGUILayout.HelpBox("Enable NumPad Controls?", MessageType.None);
            EditorGUILayout.HelpBox("Be sure to check if NumLock key is on or off", MessageType.Warning);
            settings.numPadEnabled = EditorGUILayout.Toggle("Enable NumPad Controls", settings.numPadEnabled);
            EditorGUILayout.HelpBox("Change the NumberPad controls?", MessageType.None);
            settings.zfKey = (KeyCode)EditorGUILayout.EnumPopup("Zoom In", settings.zfKey);
            settings.zbKey = (KeyCode)EditorGUILayout.EnumPopup("Zoom Out", settings.zbKey);
            settings.rlKey = (KeyCode)EditorGUILayout.EnumPopup("Rotate Left", settings.rlKey);
            settings.rrKey = (KeyCode)EditorGUILayout.EnumPopup("Rotate Right", settings.rrKey);
            settings.tuKey = (KeyCode)EditorGUILayout.EnumPopup("Tilt Up", settings.tuKey);
            settings.tdKey = (KeyCode)EditorGUILayout.EnumPopup("Tilt Down", settings.tdKey);
            settings.rtulKey = (KeyCode)EditorGUILayout.EnumPopup("Rotate Left Tilt Up", settings.rtulKey);
            settings.rtdlKey = (KeyCode)EditorGUILayout.EnumPopup("Rotate Left Tilt Down", settings.rtdlKey);
            settings.rturKey = (KeyCode)EditorGUILayout.EnumPopup("Rotate Right Tilt Up", settings.rturKey);
            settings.rtdrKey = (KeyCode)EditorGUILayout.EnumPopup("Rotate Right Tilt Down", settings.rtdrKey);

            EditorGUILayout.HelpBox("How many times can you press F to Zoom In?", MessageType.None);
            settings.zoomLevels = EditorGUILayout.IntSlider("Zoom Levels", settings.zoomLevels, 1, 16);
            EditorGUILayout.HelpBox("How many times can you rotate around your selection?", MessageType.None);
            settings.rotAngles = EditorGUILayout.IntSlider("Rotation Angles", settings.rotAngles, 4, 48);

            EditorGUILayout.HelpBox("Adjust angle to look at object?", MessageType.None);
            settings.lookAtObject = EditorGUILayout.Toggle("Look At Object", settings.lookAtObject);


            if (EditorGUI.EndChangeCheck())
            {
                PerfectF.EditorUpdate();
                PerfectF.UpdateActiveButton();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        [MenuItem("Window/ShrinkRay Entertainment/Discord Server")]
        private static void Discord()
        {
            Application.OpenURL("https://discord.gg/W8MKrRH");
        }

        [MenuItem("Window/ShrinkRay Entertainment/Email Us")]
        private static void email()
        {
            Application.OpenURL("mailto:shrinkrayentertainment@gmail.com?subject=Scene%20Pilot%20Question&body=Questions?%20Comments?%20Issues?");
        }

        [MenuItem("Window/ShrinkRay Entertainment/Perfect F/Rate Perfect F")]
        private static void Rate()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/perfect-f-177783");
        }

        [MenuItem("Window/ShrinkRay Entertainment/More ShrinkRay Assets")]
        private static void MoreAssets()
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/49750");
        }
    }
}