using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RadialMenuController))]
public class RadialMenuControllerEditor : Editor
{
    private RadialMenuController controller;

    [MenuItem("Window/Radial Menu Controller")]
    static void Init() {
        RadialMenuControllerEditorWindow window = (RadialMenuControllerEditorWindow)EditorWindow.GetWindow<RadialMenuControllerEditorWindow>();
        window.titleContent = new GUIContent("Radial Menu Controller Editor");
        window.Show();
    }

    private void OnEnable() {
        controller = (RadialMenuController)target;
    }

    public override void OnInspectorGUI() {
        if (GUILayout.Button("Open Radial Menu Editor")) {
            ShowRadialMenuEditor();
        }

        DrawDefaultInspector();
    }

    private void ShowRadialMenuEditor() {
        RadialMenuControllerEditorWindow window = (RadialMenuControllerEditorWindow)EditorWindow.GetWindow<RadialMenuControllerEditorWindow>();
        window.SetController(controller);
        window.Show();
    }
}

public class RadialMenuControllerEditorWindow : EditorWindow
{
    private RadialMenuController controller;

    public void SetController(RadialMenuController controller) {
        this.controller = controller;
    }

    private void OnGUI() {
        if (controller == null) {
            EditorGUILayout.LabelField("No Radial Menu Controller assigned.");
            return;
        }

        EditorGUILayout.LabelField("Radial Menu Controller Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Radial Menu Controller Properties
        controller.RadialMenuPrefab = (RadialMenu)EditorGUILayout.ObjectField("Radial Menu Prefab", controller.RadialMenuPrefab, typeof(RadialMenu), true);
        controller.createBool = EditorGUILayout.Toggle("Create Bool", controller.createBool);

        EditorGUILayout.Space();

        // Menu Management Buttons
        if (GUILayout.Button("Create Menu")) {
            controller.CreateMenu();
        }
        if (GUILayout.Button("Spawn Menu")) {
            controller.SpawnMenu();
        }
        if (GUILayout.Button("Destroy Menu")) {
            controller.DestroyMenu();
        }

        EditorGUILayout.Space();

        // Menu Control Buttons
        EditorGUILayout.LabelField("Menu Controls", EditorStyles.boldLabel);
        if (GUILayout.Button("Turn Left")) {
            controller.TurnMenuLeft();
        }
        if (GUILayout.Button("Turn Right")) {
            controller.TurnMenuRight();
        }
        if (GUILayout.Button("Next Ring")) {
            controller.NextRing();
        }
        if (GUILayout.Button("Previous Ring")) {
            controller.PreviousRing();
        }

        EditorGUILayout.Space();

        // Menu Layers
        EditorGUILayout.LabelField("Menu Layers", EditorStyles.boldLabel);
        if (controller.menuLayers != null && controller.menuLayers.Count > 0) {
            for (int i = 0; i < controller.menuLayers.Count; i++) {
                if (GUILayout.Button($"Edit Layer {i + 1}")) {
                    ShowLayerEditor(controller.menuLayers[i], i);
                }
            }
        }
        else {
            EditorGUILayout.LabelField("No menu layers available.");
        }
    }

    private void ShowLayerEditor(RadialMenu layer, int layerIndex) {
        RadialMenuLayerEditorWindow window = (RadialMenuLayerEditorWindow)EditorWindow.GetWindow<RadialMenuLayerEditorWindow>(false, $"Layer {layerIndex + 1} Editor");
        window.SetLayer(layer, layerIndex);
        window.Show();
    }
}