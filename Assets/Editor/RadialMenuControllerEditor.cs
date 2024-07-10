using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RadialMenuController))]
public class RadialMenuControllerEditor : Editor
{
    private RadialMenuController controller;

    [MenuItem("Window/Radial Menu Controller")]
    static void Init()
    {
        RadialMenuControllerEditorWindow window = (RadialMenuControllerEditorWindow)EditorWindow.GetWindow<RadialMenuControllerEditorWindow>();
        window.titleContent = new GUIContent("Radial Menu Controller Editor");
        window.Show();
    }

    private void OnEnable()
    {
        controller = target as RadialMenuController;
        if (controller == null)
        {
            Debug.LogError("Target is not a RadialMenuController");
            return;
        }

        // Additional initialization logic if needed
    }

    public override void OnInspectorGUI()
    {
        if (controller == null)
        {
            EditorGUILayout.LabelField("No Radial Menu Controller assigned.");
            return;
        }

        if (GUILayout.Button("Open Radial Menu Editor"))
        {
            ShowRadialMenuEditor();
        }

        DrawDefaultInspector();
    }

    private void ShowRadialMenuEditor()
    {
        RadialMenuControllerEditorWindow window = (RadialMenuControllerEditorWindow)EditorWindow.GetWindow<RadialMenuControllerEditorWindow>();
        window.SetController(controller);
        window.Show();
    }
}

public class RadialMenuControllerEditorWindow : EditorWindow
{
    private RadialMenuController controller;
    private Vector2 scrollPos;

    public void SetController(RadialMenuController controller)
    {
        this.controller = controller;
    }

    private void OnGUI()
    {
        if (controller == null)
        {
            EditorGUILayout.LabelField("No Radial Menu Controller assigned.");
            return;
        }

        EditorGUILayout.LabelField("Radial Menu Controller Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Radial Menu Controller Properties
        controller.RadialMenuPrefab = (RadialMenu)EditorGUILayout.ObjectField("Radial Menu Prefab", controller.RadialMenuPrefab, typeof(RadialMenu), true);
        controller.CreateBool = EditorGUILayout.Toggle("Create Bool", controller.CreateBool);

        EditorGUILayout.Space();

        // Menu Management Buttons
        if (GUILayout.Button("Create Menu"))
        {
            controller.CreateMenu();
        }
        if (GUILayout.Button("Spawn Menu"))
        {
            controller.SpawnMenu();
        }
        if (GUILayout.Button("Destroy Menu"))
        {
            controller.DestroyMenu();
        }

        EditorGUILayout.Space();

        // Menu Control Buttons
        EditorGUILayout.LabelField("Menu Controls", EditorStyles.boldLabel);
        if (GUILayout.Button("Turn Left"))
        {
            controller.TurnMenuLeft();
        }
        if (GUILayout.Button("Turn Right"))
        {
            controller.TurnMenuRight();
        }
        if (GUILayout.Button("Next Ring"))
        {
            controller.NextRing();
        }
        if (GUILayout.Button("Previous Ring"))
        {
            controller.PreviousRing();
        }

        EditorGUILayout.Space();

        // Menu Layers
        EditorGUILayout.LabelField("Menu Layers", EditorStyles.boldLabel);
        if (controller.MenuLayers != null && controller.MenuLayers.Count > 0)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < controller.MenuLayers.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField($"Layer {i + 1}", EditorStyles.boldLabel);
                controller.MenuLayers[i].GapWidthDegree = EditorGUILayout.FloatField("Gap Width Degree", controller.MenuLayers[i].GapWidthDegree);
                controller.MenuLayers[i].radius = EditorGUILayout.FloatField("Radius", controller.MenuLayers[i].radius);

                if (GUILayout.Button($"Edit Layer {i + 1}"))
                {
                    ShowLayerEditor(controller.MenuLayers[i], i);
                }
                if (GUILayout.Button("Save Layer"))
                {
                    SaveLayer(controller.MenuLayers[i], i);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.LabelField("No menu layers available.");
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(controller);
        }
    }

    private void ShowLayerEditor(RadialMenu layer, int layerIndex)
    {
        RadialMenuLayerEditorWindow window = (RadialMenuLayerEditorWindow)EditorWindow.GetWindow<RadialMenuLayerEditorWindow>(false, $"Layer {layerIndex + 1} Editor");
        window.SetLayer(layer, layerIndex);
        window.Show();
    }
    private void SaveLayer(RadialMenu layer, int layerIndex)
    {
        if (!controller.MenuCheck())
        {
            Debug.LogWarning("No current layer to save.");
            return;
        }
        
        RadialMenu currentLayer = controller.MenuLayers[controller.Index];
        RadialRing data = ScriptableObject.CreateInstance<RadialRing>();
        data.CopyFrom(currentLayer);

        string path = EditorUtility.SaveFilePanelInProject("Save Radial Menu Data", $"{currentLayer.name}_Data", "asset", "Please enter a file name to save the radial menu data.");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"Saved Radial Menu Data to {path}");
        }
    }
}
