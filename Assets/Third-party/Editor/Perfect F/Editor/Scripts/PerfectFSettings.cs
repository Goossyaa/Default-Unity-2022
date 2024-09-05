using UnityEditor;
using UnityEngine;

public class PerfectFSettings : ScriptableObject
{
    public enum FLocation
    {
        TopLeft,
        TopRight,
        BotLeft,
        BotRight,
        Hide
    };

    [SerializeField]
    public FLocation fLocation = FLocation.TopLeft;
    [SerializeField]
    public bool enabled = true, numPadEnabled = true, lookAtObject = true, smoothZoom = true;
    [SerializeField]
    public KeyCode rotLeft = KeyCode.Semicolon, rotRight = KeyCode.Quote;
    //public EventModifiers rotLeft = EventModifiers.Shift, rotRight = EventModifiers.Control;
    [SerializeField]
    public KeyCode
        fKey = KeyCode.F,
        zfKey = KeyCode.Keypad5,
        zbKey = KeyCode.Keypad0,
        rtdlKey = KeyCode.Keypad1,
        rlKey = KeyCode.Keypad4,
        rtulKey = KeyCode.Keypad7,
        tuKey = KeyCode.Keypad8,
        rturKey = KeyCode.Keypad9,
        rrKey = KeyCode.Keypad6,
        rtdrKey = KeyCode.Keypad3,
        tdKey = KeyCode.Keypad2;
    [SerializeField]
    public int zoomLevels = 5, rotAngles = 12;
    [SerializeField]
    [HideInInspector]
    public int tiltAngles = 12;

    [SerializeField]
    public float minSmoothZoom = 1.5f;

    //[MenuItem("Assets/Create/My Scriptable Object")]
    //public static void CreateMyAsset()
    //{
    //    PerfectFSettings asset = ScriptableObject.CreateInstance<PerfectFSettings>();

    //    AssetDatabase.CreateAsset(asset, "Assets/PerfectFSettings.asset");
    //    AssetDatabase.SaveAssets();

    //    EditorUtility.FocusProjectWindow();

    //    Selection.activeObject = asset;
    //}
}
