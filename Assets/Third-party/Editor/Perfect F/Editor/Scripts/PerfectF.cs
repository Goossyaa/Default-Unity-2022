using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using UnityEngine.Tilemaps;

namespace PerfectFNamespace
{
    [InitializeOnLoad]
    [CustomEditor(typeof(PerfectFType))]
    public class PerfectF : Editor
    {
        static GameObject selectedGameObject, selectedGameObjectPrev;
        static float selectedGameObjectRect = 0f, selectedGameObjectRectPrev = 0f;
        static int zoomCount = 0, rotCount = 0, tiltCount, zoomCountPrev = 0, zoomDir = 1;
        static int zoomLevelsPrev = 5, rotAnglesPrev = 12;
        static string selectionType = "";
        static GameObject zoomObject;
        static Bounds bounds;
        static bool isNewPress = false, newSpawn = false, nothingSelected = true, doFrame = false, fDown = false;

        static public bool microFirstPress = false;

        static PerfectFSettings settings;
        static Texture2D tActiveButton, tEnabled, tDisabled, tForward, tBackward, tRotateLeft, tRotateRight, tTiltUp, tTiltDown;

        static bool debugMode = false;


        static PerfectF()
        {
            boundsType = new BoundsType();

            EditorApplication.update += EditorUpdate;

#if UNITY_2018
            SceneView.onSceneGUIDelegate -= OnScene;
            SceneView.onSceneGUIDelegate += OnScene;
#endif

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
            SceneView.duringSceneGui += OnScene;
#endif
        }

        static void UnDelegate()
        {
            EditorApplication.update -= EditorUpdate;

#if UNITY_2018
            SceneView.onSceneGUIDelegate -= OnScene;
#endif

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
#endif
        }


        static public void EditorUpdate()
        {
            if (settings == null) { LoadSettings(); return; }
            if (!settings.enabled) return;

            //if (zoomObject == null)
            //{
            //    zoomObject = (GameObject)Instantiate(Resources.Load("Prefabs/PerfectFPoint"), new Vector3(0, 0, 0), Quaternion.identity);
            //    //zoomObject.hideFlags = HideFlags.HideInHierarchy;
            //    zoomObject.tag = "EditorOnly";
            //}
            if (tEnabled == null || tDisabled == null)
            {
                tEnabled = Resources.Load<Texture2D>("Textures/PerfectFEnabled");
                tDisabled = Resources.Load<Texture2D>("Textures/PerfectFDisabled");
                tForward = Resources.Load<Texture2D>("Textures/PerfectFForward");
                tBackward = Resources.Load<Texture2D>("Textures/PerfectFBackward");
                tRotateLeft = Resources.Load<Texture2D>("Textures/PerfectFRotateLeft");
                tRotateRight = Resources.Load<Texture2D>("Textures/PerfectFRotateRight");
                tTiltUp = Resources.Load<Texture2D>("Textures/PerfectFTiltUp");
                tTiltDown = Resources.Load<Texture2D>("Textures/PerfectFTiltDown");
                UpdateActiveButton();
            }

            if (Selection.activeGameObject != null)
            {
                selectedGameObject = Selection.activeGameObject;
                nothingSelected = false;
                UpdateActiveButton();

                if (selectedGameObject.GetComponent<RectTransform>() != null)
                {
                    selectedGameObjectRect = selectedGameObject.GetComponent<RectTransform>().rect.width + selectedGameObject.GetComponent<RectTransform>().rect.height;
                }

                if (selectedGameObject.transform.hasChanged)
                {
                    selectedGameObject.transform.hasChanged = false;
                    SetNewSpawn(true);
                    isNewPress = true;
                }

                if (selectedGameObjectPrev == null || selectedGameObject.GetInstanceID() != selectedGameObjectPrev.GetInstanceID())
                {
                    selectedGameObjectPrev = selectedGameObject;
                    SetNewSpawn(true);
                    isNewPress = true;
                }

                if (boundsType == BoundsType.canvas && (selectedGameObjectRectPrev == 0f || selectedGameObjectRect != selectedGameObjectRectPrev))
                {
                    selectedGameObjectRectPrev = selectedGameObjectRect;
                    SetNewSpawn(true);
                    isNewPress = true;
                }

                if (zoomLevelsPrev != settings.zoomLevels)
                {
                    SetNewSpawn(true);
                    isNewPress = true;
                    zoomLevelsPrev = settings.zoomLevels;
                }

                if (rotAnglesPrev != settings.rotAngles)
                {
                    SetNewSpawn(true);
                    isNewPress = true;
                    rotAnglesPrev = settings.rotAngles;
                }


            }
            else
            {
                nothingSelected = isNewPress = true;
                UpdateActiveButton();
            }

        }


        static void SetNewSpawn(bool TF)
        {
            SpawnPoints();
            if (boundsType == BoundsType.terrain) tiltCount = 10;
            zoomCount = 1;
        }

        private static void LoadSettings()
        {
            settings = Resources.Load<PerfectFSettings>("Settings/PerfectFSettings");
            //string[] results = AssetDatabase.FindAssets("Assets/Perfect F/Resources/Settings/PerfectFSettings.asset");
            //foreach (string guid in results)
            //{
            //    settings = AssetDatabase.LoadAssetAtPath<PerfectFSettings>(AssetDatabase.GUIDToAssetPath(guid));
            //}
            zoomLevelsPrev = settings.zoomLevels;
            rotAnglesPrev = settings.rotAngles;
            //if(zoomObject != null) zoomObject.tag = "EditorOnly";
        }

        static Event e;
        private static void OnScene(SceneView sceneview)
        {
            if (settings == null) { LoadSettings(); return; }
            if (!settings.enabled) return;
            if (SceneView.lastActiveSceneView == null) return;
            e = Event.current;

            #region OnScreen Controls
            if (settings.fLocation != PerfectFSettings.FLocation.Hide)
            {
                Handles.BeginGUI();
                {
                    var originalColor = GUI.color;
                    GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, .75f);
                    if (nothingSelected) GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, .25f);

                    float xFButton = 0, yFButton = 0, xOffset = 0, yOffset = 0;

                    switch (settings.fLocation)
                    {
                        case PerfectFSettings.FLocation.TopLeft:
                            xFButton = yFButton = 16;
                            xOffset = 48;
                            yOffset = 0;
                            break;
                        case PerfectFSettings.FLocation.TopRight:
                            xFButton = sceneview.position.width - 150;
                            yFButton = 16;
                            xOffset = -24 * 3;
                            yOffset = 0;
                            break;
                        case PerfectFSettings.FLocation.BotLeft:
                            xFButton = 16;
                            yFButton = sceneview.position.height - 64 - 37;
                            xOffset = 48;
                            yOffset = 0;
                            break;
                        case PerfectFSettings.FLocation.BotRight:
                            xFButton = sceneview.position.width - 64;
                            yFButton = sceneview.position.height - 64 - 37;
                            xOffset = -24 * 3;
                            yOffset = 0;
                            break;
                    }

                    GUIStyle s = new GUIStyle(GUI.skin.textArea);
                    s.alignment = TextAnchor.MiddleCenter;
                    s.fontSize = 8;
                    if (!nothingSelected) GUI.TextArea(new Rect(xFButton, yFButton + 48, 48, 12), selectionType, s);

                    // GUI.Label(new Rect(0, 0, 200, 20), SceneView.lastActiveSceneView.camera.transform.position.ToString());

                    if (GUI.Button(new Rect(xFButton, yFButton, 48, 48), tActiveButton, GUIStyle.none))
                    {
                        if (!nothingSelected)
                        {
                            if (boundsType == BoundsType.terrain)
                            {
                                tiltCount = 10;
                            }
                            else if (boundsType == BoundsType.canvas)
                            {
                                rotCount = 0;
                                tiltCount = 0;
                                zoomCount = 1;
                            }
                            else if (boundsType == BoundsType.camera)
                            {
                                rotCount = tiltCount = zoomCount = 0;
                            }
                            else
                            {
                                FindClosestView();
                                zoomCount = zoomCountPrev = 1;
                            }
                            isNewPress = false;
                            ExecuteZoom();
                        }
                    }

                    if (!isNewPress && !nothingSelected && !doFrame && boundsType != BoundsType.camera)
                    {
                        if (GUI.Button(new Rect(xFButton + xOffset, yFButton + yOffset, 24, 24), tForward, GUIStyle.none))
                        {
                            ZoomView(-1, false);
                            ExecuteZoom();
                        }
                        if (GUI.Button(new Rect(xFButton + xOffset + 24, yFButton + yOffset, 24, 24), tTiltUp, GUIStyle.none))
                        {
                            TiltView("UP", false);
                            ExecuteZoom();
                        }
                        if (GUI.Button(new Rect(xFButton + xOffset + 24 + 24, yFButton + yOffset, 24, 24), tTiltDown, GUIStyle.none))
                        {
                            TiltView("DOWN", false);
                            ExecuteZoom();
                        }

                        if (GUI.Button(new Rect(xFButton + xOffset, yFButton + yOffset + 24, 24, 24), tBackward, GUIStyle.none))
                        {
                            ZoomView(1, false);
                            ExecuteZoom();
                        }
                        if (GUI.Button(new Rect(xFButton + xOffset + 24, yFButton + yOffset + 24, 24, 24), tRotateLeft, GUIStyle.none))
                        {
                            RotateView(1, false);
                            ExecuteZoom();
                        }
                        if (GUI.Button(new Rect(xFButton + xOffset + 24 + 24, yFButton + yOffset + 24, 24, 24), tRotateRight, GUIStyle.none))
                        {
                            RotateView(-1, false);
                            ExecuteZoom();
                        }
                    }
                    GUI.color = originalColor;
                }
                Handles.EndGUI();
            }
            #endregion

            if (nothingSelected) return;

            if (newSpawn)
            {
                //SetNewSpawn(false);
                //SpawnPoints();
                //if (boundsType == BoundsType.terrain) tiltCount = 10;
                //zoomCount = 1;
                e.Use();
                return;
            }



            if (e.type == EventType.KeyUp &&
                (e.keyCode == settings.fKey ||
                    e.keyCode == settings.zfKey ||
                    e.keyCode == settings.zbKey ||
                    e.keyCode == settings.rlKey ||
                    e.keyCode == settings.rrKey ||
                    e.keyCode == settings.tuKey ||
                    e.keyCode == settings.tdKey ||
                    e.keyCode == settings.rtdlKey ||
                    e.keyCode == settings.rtulKey ||
                    e.keyCode == settings.rturKey ||
                    e.keyCode == settings.rtdrKey ||
                    e.keyCode == settings.rotLeft ||
                    e.keyCode == settings.rotRight))
            {
                if (!fDown) KeyDown();
                ExecuteZoom();
                isNewPress = false;
                fDown = false;
                return;
            }

            if (e.type == EventType.KeyDown)
            {
                KeyDown();
            }


            if (debugMode)
            {
                if (spawnPoints != null)
                    for (int t = 0; t < settings.tiltAngles; t++)
                        for (int r = 0; r < settings.rotAngles; r++)
                            for (int z = 0; z < settings.zoomLevels; z++)
                            {
                                float a = 1 - (Vector3.Distance(spawnPoints[t, r, z], SceneView.lastActiveSceneView.camera.transform.position));
                                Handles.color = Color.black;
                                if (z == zoomCount) Handles.color = new Color(1, 0, 0, 1);
                                if (r == rotCount) Handles.color = new Color(0, 1, 0, 1);
                                if (t == tiltCount) Handles.color = new Color(0, 0, 1, 1);
                                Handles.DrawWireDisc(spawnPoints[t, r, z], Vector3.up, bounds.extents.z / 5);
                                Handles.color = Color.white;
                                Handles.DrawWireCube(bounds.center, bounds.size);
                            }
            }
        }

        static void KeyDown()
        {
            if (fDown)
            {
                //e.Use();
                goto skipF;
            }
            fDown = true;

            if (e.keyCode == settings.fKey)
            {
                if (isNewPress && boundsType == BoundsType.canvas)
                {
                    rotCount = 0;
                    tiltCount = 0;
                    zoomCount = 1;
                    isNewPress = false;
                }

                if (isNewPress)
                {
                    if (boundsType == BoundsType.terrain)
                    {
                        tiltCount = 10;
                    }
                    else if (boundsType == BoundsType.camera)
                    {
                        rotCount = tiltCount = zoomCount = 0;
                    }
                    else
                    {
                        FindClosestView();
                        zoomCount = zoomCountPrev = 1;
                    }
                    isNewPress = false;
                }
                else
                {
                    FindClosestView();
                    ZoomView(1);
                    isNewPress = false;
                }
                //e.Use();
                //if (e.modifiers == settings.rotLeft)
                //{
                //    RotateView(1);
                //}

                //if (e.modifiers == settings.rotRight)
                //{
                //    RotateView(-1);
                //}
                //e.Use();
                ExecuteZoom();
            }
            else if (e.keyCode == settings.rotLeft)
            {
                RotateView(1, false);
                isNewPress = false;
                ExecuteZoom();
            }
            else if (e.keyCode == settings.rotRight)
            {
                RotateView(-1, false);
                isNewPress = false;
                ExecuteZoom();
            }
            if (settings.numPadEnabled)
            {
                if (e.keyCode == settings.rlKey)
                {
                    RotateView(1, false);
                    ExecuteZoom();
                }
                if (e.keyCode == settings.rrKey)
                {
                    RotateView(-1, false);
                    ExecuteZoom();
                }
                if (e.keyCode == settings.tuKey)
                {
                    TiltView("UP", false);
                    ExecuteZoom();
                }
                if (e.keyCode == settings.tdKey)
                {
                    TiltView("DOWN", false);
                    ExecuteZoom();
                }
                if (e.keyCode == settings.rtdlKey)
                {
                    RotateView(1, false);
                    TiltView("DOWN", false);
                    ExecuteZoom();
                }
                if (e.keyCode == settings.rtulKey)
                {
                    RotateView(1, false);
                    TiltView("UP", false);
                    ExecuteZoom();
                }
                if (e.keyCode == settings.rturKey)
                {
                    RotateView(-1, false);
                    TiltView("UP", false);
                    ExecuteZoom();
                }
                if (e.keyCode == settings.rtdrKey)
                {
                    RotateView(-1, false);
                    TiltView("DOWN", false);
                    ExecuteZoom();
                }
                if (e.keyCode == settings.zfKey)
                {
                    ZoomView(-1, false);
                    ExecuteZoom();
                }
                if (e.keyCode == settings.zbKey)
                {
                    ZoomView(1, false);
                    ExecuteZoom();
                }
            }
        skipF:;
        }

        static void FindClosestView()
        {
            if (spawnPoints == null) return;
            int minT = 9999, minR = 9999, minZ = 9999;
            float currentDist, minDist = 9999;
            var svc = SceneView.lastActiveSceneView.camera.transform.position;
            for (int t = 0; t < settings.tiltAngles; t++)
            {
                for (int r = 0; r < settings.rotAngles; r++)
                {
                    for (int z = 0; z < settings.zoomLevels; z++)
                    {
                        if (t == 9 || t == 3) continue;
                        currentDist = Vector3.Distance(svc, spawnPoints[t, r, z]);
                        if (currentDist < minDist)
                        {
                            minDist = currentDist;
                            minT = t;
                            minR = r;
                            minZ = z;
                        }
                    }
                }
            }
            zoomCount = minZ;
            rotCount = minR;
            tiltCount = minT;
        }
        static public void UpdateActiveButton()
        {
            if (!nothingSelected)
            {
                tActiveButton = tEnabled;
            }
            else
            {
                tActiveButton = tDisabled;
            }
            SceneView.RepaintAll();
        }

        static private void ExecuteZoom()
        {
            if (zoomObject == null)
            {
                zoomObject = (GameObject)Instantiate(Resources.Load("Prefabs/PerfectFPoint"), new Vector3(0, 0, 0), Quaternion.identity);
                //zoomObject.hideFlags = HideFlags.HideInHierarchy;
                zoomObject.tag = "EditorOnly";
            }
            if (doFrame)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
            else if (boundsType == BoundsType.camera)
            {
                zoomObject.transform.position = selectedGameObject.transform.position;
                zoomObject.transform.rotation = selectedGameObject.transform.rotation;
                SceneView.lastActiveSceneView.AlignViewToObject(zoomObject.transform);

            }
            else
            {
                zoomObject.transform.position = spawnPoints[tiltCount, rotCount, zoomCount];
                if (settings.lookAtObject)
                {
                    zoomObject.transform.LookAt(bounds.center);
                }
                else
                {
                    zoomObject.transform.rotation = SceneView.currentDrawingSceneView.camera.transform.rotation;
                }

                if ((bounds.extents.x > settings.minSmoothZoom || bounds.extents.y > settings.minSmoothZoom || bounds.extents.z > settings.minSmoothZoom) && selectionType != "Micro")
                {
                    SceneView.lastActiveSceneView.AlignViewToObject(zoomObject.transform);
                }
                else
                {
                    if (/*selectionType == "Micro" &&*/ microFirstPress)
                    {
                        SceneView.lastActiveSceneView.FrameSelected();
                        microFirstPress = false;
                    }
                    SceneView.lastActiveSceneView.pivot = zoomObject.transform.position;
                    SceneView.lastActiveSceneView.rotation = zoomObject.transform.rotation;
                    SceneView.lastActiveSceneView.Repaint();

                }
                // 
                // SceneView.lastActiveSceneView.LookAt(selectedGameObject.transform.position);
            }
            if (zoomObject != null)
            {
                GameObject.DestroyImmediate(zoomObject);

            }

            e.Use();
        }

        static Vector3[,,] spawnPoints;
        enum BoundsType { none, renderer, mesh, camera, canvas, terrain }
        static BoundsType boundsType;


        static Bounds CalculateBounds(GameObject go)
        {
            Bounds b = new Bounds(go.transform.position, Vector3.zero);
            Renderer[] rList = go.GetComponentsInChildren<Renderer>();
            Vector3 min = new Vector3(), max = new Vector3();
            blist = new List<Vector3>();
            bool first = true;
            for (int i = 0; i < rList.Length; i++)
            {
                if (rList[i].GetComponent<MeshRenderer>() != null || rList[i].GetComponent<SkinnedMeshRenderer>() != null)
                {
                    if (first)
                    {
                        min = rList[i].bounds.min;
                        max = rList[i].bounds.max;
                        first = false;
                    }
                    blist.Add(rList[i].bounds.center);
                    min = Vector3.Min(min, rList[i].bounds.min);
                    max = Vector3.Max(max, rList[i].bounds.max);

                    b.center = (max - min) / 2;
                    b.max = max;
                    b.min = min;
                    // Debug.Log(b.extents.x);
                }
            }
            // if (b.extents.x < 0.01) b.extents = new Vector3(10.1f, 10.1f, 10.1f);
            return b;
        }

        static List<Vector3> blist;
        static float dist;

        static private void SpawnPoints()
        {
            if (SceneView.lastActiveSceneView == null) return;
            boundsType = new BoundsType();

            if (selectedGameObject.GetComponent<Canvas>() != null || selectedGameObject.GetComponentInParent<Canvas>() != null)
            {
                boundsType = BoundsType.canvas;
                rotCount = 0;
                tiltCount = 0;
                zoomCount = 1;
                bounds = new Bounds(selectedGameObject.transform.position,
                    new Vector3(
                        selectedGameObject.GetComponent<RectTransform>().rect.width / 1.5f,
                        selectedGameObject.GetComponent<RectTransform>().rect.height / 1.5f,
                        0));
                selectionType = "Canvas";
            }
            else if (selectedGameObject.GetComponent<Camera>() != null)
            {
                boundsType = BoundsType.camera;
                selectionType = "Camera";
                return;
            }
            else if (selectedGameObject.GetComponent<Terrain>() != null)
            {
                boundsType = BoundsType.terrain;
                tiltCount = 10;
                zoomCount = 1;
                var t = selectedGameObject.GetComponent<Terrain>();
                bounds = new Bounds(t.terrainData.bounds.center + t.transform.position, t.terrainData.bounds.size);
                selectionType = "Terrain";
            }
            else if (selectedGameObject.GetComponent<MeshRenderer>() != null)
            {
                boundsType = BoundsType.mesh;
                bounds = CalculateBounds(selectedGameObject.gameObject);
                if (bounds == new Bounds()) return;
                selectionType = "Mesh";
            }
            else
            {
                boundsType = BoundsType.none;
                //float ncp = SceneView.currentDrawingSceneView.camera.nearClipPlane;
                bounds = new Bounds(selectedGameObject.transform.position, new Vector3(1, 1, 1));
                selectionType = "Empty";
            }

            // Debug.Log(bounds);

            Vector3 max = bounds.size;
            float radius = max.magnitude / 2f;
            float x = 60; if (SceneView.lastActiveSceneView?.camera != null) x = SceneView.lastActiveSceneView.camera.fieldOfView;
            float a = 1.3f; if (SceneView.lastActiveSceneView?.camera != null) a = SceneView.lastActiveSceneView.camera.aspect;
            float horizontalFOV = 2f * Mathf.Atan(Mathf.Tan(x * Mathf.Deg2Rad / 2f) * a) * Mathf.Rad2Deg;
            float fov = Mathf.Min(SceneView.lastActiveSceneView.camera.fieldOfView, horizontalFOV);
            dist = radius / (Mathf.Sin(fov * Mathf.Deg2Rad / 2f));

            // Debug.Log("dist < SceneView.lastActiveSceneView.camera.nearClipPlane " + dist + " < " + SceneView.lastActiveSceneView.camera.nearClipPlane);
            //#if UNITY_2018
            if ((bounds.extents.x < 0.1 || bounds.extents.y < 0.1 || bounds.extents.z < 0.1) && boundsType != BoundsType.canvas)
            {
                doFrame = true;

                // boundsType = BoundsType.mesh;
                // bounds = CalculateBounds(selectedGameObject.gameObject);
                // if (bounds == new Bounds()) return;

                selectionType = "Micro";
                microFirstPress = true;
                // return;
            }
            //#endif
            //#if UNITY_2019 || UNITY_2020
            //            if (dist < SceneView.lastActiveSceneView.camera.nearClipPlane && boundsType != BoundsType.canvas) {
            //                SceneView.CameraSettings settings = new SceneView.CameraSettings();
            //                settings.dynamicClip = false;
            //                settings.nearClip = 0.001f;
            //                SceneView sceneView = SceneView.lastActiveSceneView;
            //                sceneView.cameraSettings = settings;
            //                selectionType = "Micro";
            //            }
            //#endif

            doFrame = false;

            Transform f = SceneView.lastActiveSceneView.camera.transform;

            spawnPoints = new Vector3[settings.tiltAngles, settings.rotAngles, settings.zoomLevels];
            int i = 0;
            for (int t = 0; t < settings.tiltAngles; t++)
            {
                for (int r = 0; r < settings.rotAngles; r++)
                {
                    for (int z = 0; z < settings.zoomLevels; z++, i++)
                    {

                        if (boundsType == BoundsType.canvas)
                        {
                            f.position = (bounds.center - (dist * z) * selectedGameObject.transform.forward);
                            f.RotateAround(bounds.center, Vector3.left, (float)t / settings.tiltAngles * 360);
                            f.RotateAround(bounds.center, Vector3.up, (float)r / settings.rotAngles * 360);
                        }
                        else
                        {
                            float z2 = z;
                            if (z == 0) z2 = dist * 0.25f;
                            if (z >= 1) z2 = Mathf.Lerp(dist * 0.25f, dist * 2, (float)z / settings.zoomLevels);
                            f.position = (bounds.center - z2 * Vector3.forward);
                            f.RotateAround(bounds.center, Vector3.left, (float)t / settings.tiltAngles * 360);
                            f.RotateAround(bounds.center, Vector3.up, (float)r / settings.rotAngles * 360);
                        }
                        spawnPoints[t, r, z] = f.position;
                        // Debug.Log(spawnPoints[t, r, z].z);
                    }
                }
            }
        }


        static private void RotateView(int i, bool revertZoom = true)
        {
            rotCount += i;
            rotCount = (int)nfmod(rotCount, settings.rotAngles);
            if (revertZoom) zoomCount = zoomCountPrev;
        }

        static private void TiltView(string dir, bool revertZoom = true)
        {
            int i = 0;
            if (dir == "UP")
            {
                int t = tiltCount + 1;
                if (t > settings.tiltAngles - 1) t = 0;
                if (t < 0) t = settings.tiltAngles - 1;
                if (spawnPoints[t, rotCount, zoomCount].y > spawnPoints[tiltCount, rotCount, zoomCount].y) i = +1;
                if (spawnPoints[t, rotCount, zoomCount].y < spawnPoints[tiltCount, rotCount, zoomCount].y) i = -1;
            }
            else
            {
                int t = tiltCount - 1;
                if (t > settings.tiltAngles - 1) t = 0;
                if (t < 0) t = settings.tiltAngles - 1;
                if (spawnPoints[t, rotCount, zoomCount].y > spawnPoints[tiltCount, rotCount, zoomCount].y) i = +1;
                if (spawnPoints[t, rotCount, zoomCount].y < spawnPoints[tiltCount, rotCount, zoomCount].y) i = -1;
            }
            int x = tiltCount + i;
            if (x == 9 || x == 3) return;
            tiltCount += i;
            tiltCount = (int)nfmod(tiltCount, settings.tiltAngles);
        }

        static private void ZoomView(int i, bool cycle = true)
        {
            if (zoomCount + i == 0 && boundsType == BoundsType.canvas) return;

            zoomCountPrev = zoomCount;
            //zoomCount += i;
            if (cycle)
            {
                if (zoomCount + i * zoomDir < 0 || zoomCount + i * zoomDir > settings.zoomLevels - 1) zoomDir *= -1;
                zoomCount += i * zoomDir;
                //zoomCount = (int)nfmod(zoomCount, settings.zoomLevels);
            }
            else
            {
                zoomCount += i;
                zoomCount = Mathf.Clamp(zoomCount, 0, settings.zoomLevels - 1);
            }
        }

        static float nfmod(float a, float b)
        {
            return a - b * Mathf.Floor(a / b);
        }
    }
}