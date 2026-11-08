using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MagicLock.Radial.Editor
{
    [CustomEditor(typeof(RadialMenuController), true)]
    public class RadialMenuControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Radial Menu Editor")) {
                RadialMenuControllerEditorWindow.Open((RadialMenuController)target);
            }

            DrawDefaultInspector();
        }
    }

    /// <summary>
    /// Authoring view of a controller's ring chain (edits the ring assets) plus Play Mode controls for previewing.
    /// </summary>
    public class RadialMenuControllerEditorWindow : EditorWindow
    {
        [SerializeField] private RadialMenuController controller;
        private SerializedObject _serializedController;
        private Vector2 _scrollPos;

        [MenuItem("Window/Radial Menu Controller")]
        private static void OpenFromMenu() =>
            Open(Selection.activeGameObject ? Selection.activeGameObject.GetComponent<RadialMenuController>() : null);

        public static void Open(RadialMenuController target)
        {
            var window = GetWindow<RadialMenuControllerEditorWindow>();
            window.titleContent = new GUIContent("Radial Menu Controller");
            if (target) window.SetController(target);
            window.Show();
        }

        public void SetController(RadialMenuController pController)
        {
            controller = pController;
            _serializedController = null;
        }

        private void OnSelectionChange()
        {
            var selected = Selection.activeGameObject
                ? Selection.activeGameObject.GetComponent<RadialMenuController>()
                : null;
            if (selected) {
                SetController(selected);
                Repaint();
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying) Repaint();
        }

        private void OnGUI()
        {
            if (!controller) {
                EditorGUILayout.HelpBox("Select a GameObject with a RadialMenuController.", MessageType.Info);
                return;
            }

            if (_serializedController == null || _serializedController.targetObject != controller) {
                _serializedController = new SerializedObject(controller);
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField(controller.name, EditorStyles.boldLabel);
            _serializedController.Update();
            EditorGUILayout.PropertyField(_serializedController.FindProperty(nameof(RadialMenuController.menuPrefab)));
            EditorGUILayout.PropertyField(_serializedController.FindProperty(nameof(RadialMenuController.rootRing)));
            _serializedController.ApplyModifiedProperties();

            EditorGUILayout.Space();
            DrawRings();
            EditorGUILayout.Space();
            DrawPlayControls();

            EditorGUILayout.EndScrollView();
        }

        private void DrawRings()
        {
            EditorGUILayout.LabelField("Rings", EditorStyles.boldLabel);

            RadialRing root = controller.RootRing;
            if (!root) {
                EditorGUILayout.HelpBox("No root ring. Assign one, or give the menu prefab a Data ring.", MessageType.Info);
                if (GUILayout.Button("Create Root Ring...")) {
                    RadialRing created = CreateRingAsset("RootRing");
                    if (created) {
                        Undo.RecordObject(controller, "Assign Root Ring");
                        controller.rootRing = created;
                        EditorUtility.SetDirty(controller);
                    }
                }

                return;
            }

            List<RadialRing> rings = root.Chain().ToList();
            for (int i = 0; i < rings.Count; i++) {
                RadialRing ring = rings[i];
                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField($"Layer {i + 1}", GUILayout.Width(60));
                    using (new EditorGUI.DisabledScope(true)) {
                        EditorGUILayout.ObjectField(ring, typeof(RadialRing), false);
                    }

                    EditorGUILayout.LabelField($"{ring.Elements.Count(e => e)} elements", GUILayout.Width(80));
                    if (GUILayout.Button("Edit", GUILayout.Width(50))) {
                        var window = GetWindow<RadialMenuLayerEditorWindow>(false, $"Layer {i + 1}");
                        window.SetRing(ring, i);
                        window.Show();
                    }
                }
            }

            RadialRing last = rings[rings.Count - 1];
            if (last.NextRing) {
                EditorGUILayout.HelpBox($"\"{last.name}\" links back into the chain. Fix its Next Ring.", MessageType.Warning);
            }
            else if (GUILayout.Button("Add Ring...")) {
                RadialRing created = CreateRingAsset($"Layer{rings.Count + 1}Ring");
                if (created) {
                    Undo.RecordObject(last, "Add Radial Ring");
                    last.NextRing = created;
                    EditorUtility.SetDirty(last);
                }
            }
        }

        private void DrawPlayControls()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            if (!Application.isPlaying) {
                EditorGUILayout.HelpBox("Enter Play Mode to preview the menu.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button(controller.IsOpen ? "Close" : "Open")) controller.Toggle();
                if (GUILayout.Button("Rebuild")) controller.Rebuild();
            }

            using (new EditorGUI.DisabledScope(!controller.IsOpen)) {
                using (new EditorGUILayout.HorizontalScope()) {
                    if (GUILayout.Button("Turn Left")) controller.TurnLeft();
                    if (GUILayout.Button("Turn Right")) controller.TurnRight();
                }

                using (new EditorGUILayout.HorizontalScope()) {
                    if (GUILayout.Button("Back")) controller.Back();
                    if (GUILayout.Button("Confirm")) controller.Confirm();
                }
            }

            if (controller.IsOpen) {
                RadialMenu layer = controller.CurrentLayer;
                EditorGUILayout.LabelField("Layer", $"{controller.LayerIndex + 1} / {controller.Layers.Count}");
                EditorGUILayout.LabelField("Highlighted",
                    layer && layer.SelectedElement ? layer.SelectedElement.DisplayName : "-");
                EditorGUILayout.LabelField("Path so far", RadialMenuController.JoinNames(controller.CurrentPath));
            }
        }

        private static RadialRing CreateRingAsset(string defaultName)
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Radial Ring", defaultName, "asset",
                "Choose where to save the new ring.");
            if (string.IsNullOrEmpty(path)) return null;

            var ring = CreateInstance<RadialRing>();
            AssetDatabase.CreateAsset(ring, path);
            AssetDatabase.SaveAssets();
            return ring;
        }
    }
}
