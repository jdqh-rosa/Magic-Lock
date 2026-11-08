using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MagicLock.Radial.Editor
{
    /// <summary>
    /// Builds or edits a complete radial menu: menu-wide style, layers (rings) with optional overrides, and elements
    /// with optional overrides. Works on an in-memory draft; "Create" / "Save" writes the prefab, ring and element
    /// assets. Loading an existing menu prefab and saving updates those assets in place.
    /// </summary>
    public class RadialMenuCreator : EditorWindow
    {
        private const float LeftWidth = 330f;
        private const float PreviewHeight = 300f;

        [SerializeField] private RadialMenuDraft draft;
        [SerializeField] private GameObject loadTarget;
        [SerializeField] private int selectedLayer;
        [SerializeField] private int selectedElement = -1;
        [SerializeField] private bool styleFoldout = true;
        [SerializeField] private bool dirty;
        [SerializeField] private Vector2 leftScroll;
        [SerializeField] private Vector2 rightScroll;

        private SerializedObject _serializedDraft;
        private ReorderableList _layerList;
        private ReorderableList _elementList;
        private int _elementListLayer = -1;
        private Action _pending;

        [MenuItem("RadialMenu/Creator")]
        public static void ShowWindow() => Open(null);

        public static void Open(GameObject menuPrefab)
        {
            var window = GetWindow<RadialMenuCreator>();
            window.titleContent = new GUIContent("Radial Menu Creator");
            window.minSize = new Vector2(760f, 520f);
            if (menuPrefab) window.Load(menuPrefab);
            window.Show();
        }

        private void OnEnable()
        {
            if (!draft) SetDraft(RadialMenuDraft.CreateNew());
            Undo.undoRedoPerformed += Repaint;
        }

        private void OnDisable() => Undo.undoRedoPerformed -= Repaint;

        private void OnDestroy()
        {
            if (draft) DestroyImmediate(draft);
        }

        // ----------------------------------------------------------------------------------------------------
        // Draft management

        private void SetDraft(RadialMenuDraft newDraft)
        {
            if (draft && draft != newDraft) DestroyImmediate(draft);
            draft = newDraft;
            _serializedDraft = null;
            _layerList = null;
            _elementList = null;
            _elementListLayer = -1;
            selectedLayer = draft.layers.Count > 0 ? 0 : -1;
            selectedElement = -1;
            dirty = false;
        }

        private bool ConfirmDiscard() =>
            !dirty || EditorUtility.DisplayDialog("Discard changes?", "The current draft has unsaved changes.",
                "Discard", "Cancel");

        private void Load(GameObject prefab)
        {
            RadialMenu menu = prefab ? prefab.GetComponent<RadialMenu>() : null;
            if (!menu || !PrefabUtility.IsPartOfPrefabAsset(prefab)) {
                EditorUtility.DisplayDialog("Radial Menu Creator",
                    "Pick a prefab asset with a RadialMenu component on its root.", "OK");
                return;
            }

            SetDraft(RadialMenuDraft.FromPrefab(prefab, menu));
            loadTarget = prefab;
        }

        private LayerDraft CurrentLayer =>
            selectedLayer >= 0 && selectedLayer < draft.layers.Count ? draft.layers[selectedLayer] : null;

        private ElementDraft CurrentElement
        {
            get
            {
                LayerDraft layer = CurrentLayer;
                return layer != null && selectedElement >= 0 && selectedElement < layer.elements.Count
                    ? layer.elements[selectedElement]
                    : null;
            }
        }

        /// <summary>Apply pending GUI edits, record an undo step and change the draft directly.</summary>
        private void ModifyDraft(string undoName, Action change)
        {
            _serializedDraft.ApplyModifiedProperties();
            Undo.RecordObject(draft, undoName);
            change();
            _serializedDraft.Update();
            dirty = true;
        }

        // ----------------------------------------------------------------------------------------------------
        // GUI

        private void OnGUI()
        {
            if (!draft) SetDraft(RadialMenuDraft.CreateNew());
            if (_serializedDraft == null || _serializedDraft.targetObject != draft) {
                _serializedDraft = new SerializedObject(draft);
                _layerList = null;
                _elementList = null;
            }

            _serializedDraft.Update();
            ClampSelection();

            EditorGUI.BeginChangeCheck();

            DrawToolbar();
            using (new EditorGUILayout.HorizontalScope()) {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(LeftWidth))) {
                    leftScroll = EditorGUILayout.BeginScrollView(leftScroll);
                    DrawOutput();
                    DrawStyle();
                    DrawLayerList();
                    DrawElementList();
                    EditorGUILayout.EndScrollView();
                }

                using (new EditorGUILayout.VerticalScope()) {
                    rightScroll = EditorGUILayout.BeginScrollView(rightScroll);
                    DrawLayerDetails();
                    DrawElementDetails();
                    EditorGUILayout.EndScrollView();
                    DrawPreview();
                }
            }

            DrawFooter();

            if (EditorGUI.EndChangeCheck()) dirty = true;
            _serializedDraft.ApplyModifiedProperties();

            if (_pending != null) {
                Action pending = _pending;
                _pending = null;
                pending();
                Repaint();
                GUIUtility.ExitGUI();
            }
        }

        private void ClampSelection()
        {
            selectedLayer = draft.layers.Count == 0 ? -1 : Mathf.Clamp(selectedLayer, 0, draft.layers.Count - 1);
            LayerDraft layer = CurrentLayer;
            if (layer == null || selectedElement >= layer.elements.Count) selectedElement = -1;
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50))) {
                    _pending = () =>
                    {
                        if (ConfirmDiscard()) SetDraft(RadialMenuDraft.CreateNew());
                    };
                }

                GUILayout.Space(8);
                GUILayout.Label("Menu prefab", GUILayout.Width(76));
                loadTarget = (GameObject)EditorGUILayout.ObjectField(loadTarget, typeof(GameObject), false,
                    GUILayout.Width(200));
                using (new EditorGUI.DisabledScope(!loadTarget)) {
                    if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50))) {
                        GameObject target = loadTarget;
                        _pending = () =>
                        {
                            if (ConfirmDiscard()) Load(target);
                        };
                    }
                }

                GUILayout.FlexibleSpace();
                string status = draft.sourcePrefab
                    ? $"Editing {AssetDatabase.GetAssetPath(draft.sourcePrefab)}"
                    : "New menu (not saved yet)";
                GUILayout.Label(dirty ? status + " *" : status, EditorStyles.miniLabel);
            }
        }

        private void DrawOutput()
        {
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedDraft.FindProperty(nameof(RadialMenuDraft.menuName)));

            if (draft.sourcePrefab) {
                EditorGUILayout.HelpBox(
                    "Saving updates this prefab and its ring/element assets in place. " +
                    "New layers and elements are created next to it.", MessageType.None);
                if (GUILayout.Button("Save As New Copy Instead")) {
                    ModifyDraft("Detach Radial Menu", draft.Detach);
                }
            }
            else {
                using (new EditorGUILayout.HorizontalScope()) {
                    SerializedProperty folder = _serializedDraft.FindProperty(nameof(RadialMenuDraft.folder));
                    EditorGUILayout.PropertyField(folder);
                    if (GUILayout.Button("...", GUILayout.Width(26))) {
                        string picked = EditorUtility.OpenFolderPanel("Output folder", folder.stringValue, "");
                        string relative = ToProjectPath(picked);
                        if (relative != null) folder.stringValue = relative;
                        else if (!string.IsNullOrEmpty(picked)) {
                            EditorUtility.DisplayDialog("Radial Menu Creator", "Pick a folder inside Assets.", "OK");
                        }
                    }
                }
            }

            EditorGUILayout.LabelField("Saves to", AssetsRoot(), EditorStyles.miniLabel);
            EditorGUILayout.Space();
        }

        private void DrawStyle()
        {
            styleFoldout = EditorGUILayout.Foldout(styleFoldout, "Menu Style (all layers)", true, EditorStyles.foldoutHeader);
            if (styleFoldout) {
                SerializedProperty style = _serializedDraft.FindProperty(nameof(RadialMenuDraft.style));
                using (new EditorGUI.IndentLevelScope()) {
                    foreach (SerializedProperty child in Children(style)) {
                        EditorGUILayout.PropertyField(child, true);
                    }
                }
            }

            EditorGUILayout.Space();
        }

        private void DrawLayerList()
        {
            if (_layerList == null) {
                _layerList = new ReorderableList(_serializedDraft, _serializedDraft.FindProperty(nameof(RadialMenuDraft.layers)),
                    true, true, true, true);
                _layerList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Layers (inner to outer)");
                _layerList.drawElementCallback = (rect, index, active, focused) =>
                {
                    LayerDraft layer = index < draft.layers.Count ? draft.layers[index] : null;
                    if (layer == null) return;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    rect.y += 2;
                    int overrides = CountOverrides(layer.overrides);
                    string extra = overrides > 0 ? $", {overrides} overrides" : "";
                    EditorGUI.LabelField(rect, $"{index + 1}. {layer.assetName}", $"{layer.elements.Count} elements{extra}");
                };
                _layerList.onSelectCallback = list =>
                {
                    selectedLayer = list.index;
                    selectedElement = -1;
                };
                _layerList.onAddCallback = list => ModifyDraft("Add Layer", () =>
                {
                    draft.layers.Add(new LayerDraft { assetName = $"Layer{draft.layers.Count + 1}" });
                    selectedLayer = list.index = draft.layers.Count - 1;
                    selectedElement = -1;
                });
                _layerList.onRemoveCallback = list => ModifyDraft("Remove Layer", () =>
                {
                    draft.layers.RemoveAt(list.index);
                    selectedLayer = list.index = Mathf.Min(list.index, draft.layers.Count - 1);
                    selectedElement = -1;
                });
                _layerList.onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
                {
                    selectedLayer = newIndex;
                    dirty = true;
                };
            }

            _layerList.index = selectedLayer;
            _layerList.DoLayoutList();
        }

        private void DrawElementList()
        {
            LayerDraft layer = CurrentLayer;
            if (layer == null) return;

            if (_elementList == null || _elementListLayer != selectedLayer) {
                _elementListLayer = selectedLayer;
                SerializedProperty elements = _serializedDraft.FindProperty(nameof(RadialMenuDraft.layers))
                    .GetArrayElementAtIndex(selectedLayer).FindPropertyRelative(nameof(LayerDraft.elements));
                _elementList = new ReorderableList(_serializedDraft, elements, true, true, true, true);
                _elementList.drawHeaderCallback = rect =>
                    EditorGUI.LabelField(rect, $"Elements of {CurrentLayer?.assetName}");
                _elementList.elementHeight = 22f;
                _elementList.drawElementCallback = (rect, index, active, focused) =>
                {
                    LayerDraft current = CurrentLayer;
                    if (current == null || index >= current.elements.Count) return;
                    ElementDraft element = current.elements[index];

                    var iconRect = new Rect(rect.x, rect.y + 2, 18, 18);
                    Sprite sprite = element.icon ? element.icon : element.circle;
                    if (sprite) DrawSprite(iconRect, sprite, Color.white);
                    var labelRect = new Rect(rect.x + 22, rect.y + 2, rect.width - 22, EditorGUIUtility.singleLineHeight);
                    int overrides = CountOverrides(element.overrides);
                    EditorGUI.LabelField(labelRect, element.assetName, overrides > 0 ? $"{overrides} overrides" : "");
                };
                _elementList.onSelectCallback = list => selectedElement = list.index;
                _elementList.onAddCallback = list => ModifyDraft("Add Element", () =>
                {
                    List<ElementDraft> elements = CurrentLayer.elements;
                    elements.Add(new ElementDraft { assetName = $"{CurrentLayer.assetName}_Element{elements.Count + 1}" });
                    selectedElement = list.index = elements.Count - 1;
                });
                _elementList.onRemoveCallback = list => ModifyDraft("Remove Element", () =>
                {
                    CurrentLayer.elements.RemoveAt(list.index);
                    selectedElement = list.index = Mathf.Min(list.index, CurrentLayer.elements.Count - 1);
                });
                _elementList.onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
                {
                    selectedElement = newIndex;
                    dirty = true;
                };
            }

            EditorGUILayout.Space();
            _elementList.index = selectedElement;
            _elementList.DoLayoutList();
        }

        private void DrawLayerDetails()
        {
            LayerDraft layer = CurrentLayer;
            if (layer == null) {
                EditorGUILayout.HelpBox("Add a layer to start.", MessageType.Info);
                return;
            }

            SerializedProperty layerProp = _serializedDraft.FindProperty(nameof(RadialMenuDraft.layers))
                .GetArrayElementAtIndex(selectedLayer);

            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField($"Layer {selectedLayer + 1}", EditorStyles.boldLabel);
                if (GUILayout.Button("Duplicate Layer", GUILayout.Width(110))) {
                    ModifyDraft("Duplicate Layer", () =>
                    {
                        draft.layers.Insert(selectedLayer + 1, layer.CloneAsNew());
                        selectedLayer++;
                        selectedElement = -1;
                    });
                    return;
                }
            }

            EditorGUILayout.PropertyField(layerProp.FindPropertyRelative(nameof(LayerDraft.assetName)));
            EditorGUILayout.PropertyField(layerProp.FindPropertyRelative(nameof(LayerDraft.menuName)));
            if (layer.source) {
                using (new EditorGUI.DisabledScope(true)) {
                    EditorGUILayout.ObjectField("Ring Asset", layer.source, typeof(RadialRing), false);
                }
            }

            SerializedProperty overrides = layerProp.FindPropertyRelative(nameof(LayerDraft.overrides));
            overrides.isExpanded = EditorGUILayout.Foldout(overrides.isExpanded,
                $"Layer Overrides ({CountOverrides(layer.overrides)})", true, EditorStyles.foldoutHeader);
            if (overrides.isExpanded) {
                EditorGUILayout.LabelField("Tick a setting to override the menu style for this layer only.",
                    EditorStyles.miniLabel);
                int depth = selectedLayer;
                DrawOverrides(overrides, draft.style,
                    name => name == nameof(RadialRingOverrides.radius) ? draft.style.RadiusFor(depth, null) : null);
            }

            EditorGUILayout.Space();
        }

        private void DrawElementDetails()
        {
            LayerDraft layer = CurrentLayer;
            ElementDraft element = CurrentElement;
            if (layer == null) return;
            if (element == null) {
                EditorGUILayout.HelpBox("Select an element to edit it.", MessageType.None);
                return;
            }

            SerializedProperty elementProp = _serializedDraft.FindProperty(nameof(RadialMenuDraft.layers))
                .GetArrayElementAtIndex(selectedLayer).FindPropertyRelative(nameof(LayerDraft.elements))
                .GetArrayElementAtIndex(selectedElement);

            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField($"Element {selectedElement + 1}", EditorStyles.boldLabel);
                if (GUILayout.Button("Duplicate Element", GUILayout.Width(120))) {
                    ModifyDraft("Duplicate Element", () =>
                    {
                        layer.elements.Insert(selectedElement + 1, element.CloneAsNew());
                        selectedElement++;
                    });
                    return;
                }
            }

            EditorGUILayout.PropertyField(elementProp.FindPropertyRelative(nameof(ElementDraft.assetName)));
            EditorGUILayout.PropertyField(elementProp.FindPropertyRelative(nameof(ElementDraft.displayName)));
            EditorGUILayout.PropertyField(elementProp.FindPropertyRelative(nameof(ElementDraft.circle)), new GUIContent("Background"));
            EditorGUILayout.PropertyField(elementProp.FindPropertyRelative(nameof(ElementDraft.icon)));

            if (element.source) {
                using (new EditorGUI.DisabledScope(true)) {
                    EditorGUILayout.ObjectField("Element Asset", element.source, typeof(RadialElement), false);
                }

                int uses = draft.layers.SelectMany(l => l.elements).Count(e => e.source == element.source);
                if (uses > 1) {
                    EditorGUILayout.HelpBox(
                        $"This element asset fills {uses} slots. They all save to the same asset, so the last slot's " +
                        "settings win. Duplicate the element to give a slot its own asset.", MessageType.Warning);
                }
            }

            SerializedProperty overrides = elementProp.FindPropertyRelative(nameof(ElementDraft.overrides));
            overrides.isExpanded = EditorGUILayout.Foldout(overrides.isExpanded,
                $"Element Overrides ({CountOverrides(element.overrides)})", true, EditorStyles.foldoutHeader);
            if (overrides.isExpanded) {
                EditorGUILayout.LabelField("Tick a setting to override the layer's style for this element only.",
                    EditorStyles.miniLabel);
                DrawOverrides(overrides, draft.style.With(layer.overrides), null);
            }
        }

        /// <summary>
        /// Draws each Optional field of an overrides object. Unticked fields show the inherited value, read-only;
        /// ticking one starts it at the inherited value.
        /// </summary>
        private static void DrawOverrides(SerializedProperty overrides, RadialMenuStyle inherited,
            Func<string, object> special)
        {
            using (new EditorGUI.IndentLevelScope()) {
                foreach (SerializedProperty optional in Children(overrides)) {
                    FieldInfo field = typeof(RadialMenuStyle).GetField(optional.name);
                    if (field == null) {
                        EditorGUILayout.PropertyField(optional, true);
                        continue;
                    }

                    object inheritedValue = special?.Invoke(optional.name) ?? field.GetValue(inherited);
                    DrawOverride(optional, inheritedValue, field.FieldType);
                }
            }
        }

        private static void DrawOverride(SerializedProperty optional, object inheritedValue, Type type)
        {
            SerializedProperty enabled = optional.FindPropertyRelative("enabled");
            SerializedProperty value = optional.FindPropertyRelative("value");
            var label = new GUIContent(optional.displayName, optional.tooltip);

            using (new EditorGUILayout.HorizontalScope()) {
                bool wasEnabled = enabled.boolValue;
                bool isEnabled = EditorGUILayout.Toggle(wasEnabled, GUILayout.Width(30));
                if (isEnabled != wasEnabled) {
                    enabled.boolValue = isEnabled;
                    if (isEnabled) SetValue(value, inheritedValue);
                }

                if (isEnabled) {
                    EditorGUILayout.PropertyField(value, label);
                }
                else {
                    using (new EditorGUI.DisabledScope(true)) {
                        DrawReadOnly(label, inheritedValue, type);
                    }
                }
            }
        }

        private static void SetValue(SerializedProperty property, object value)
        {
            switch (property.propertyType) {
                case SerializedPropertyType.Float:
                    property.floatValue = value is float f ? f : 0f;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = value is bool b && b;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = value is Color c ? c : Color.white;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as Object;
                    break;
            }
        }

        private static void DrawReadOnly(GUIContent label, object value, Type type)
        {
            if (type == typeof(float)) EditorGUILayout.FloatField(label, value is float f ? f : 0f);
            else if (type == typeof(bool)) EditorGUILayout.Toggle(label, value is bool b && b);
            else if (type == typeof(Color)) EditorGUILayout.ColorField(label, value is Color c ? c : Color.white);
            else if (typeof(Object).IsAssignableFrom(type)) EditorGUILayout.ObjectField(label, value as Object, type, false);
            else EditorGUILayout.LabelField(label, new GUIContent(value?.ToString() ?? "-"));
        }

        // ----------------------------------------------------------------------------------------------------
        // Preview

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            Rect area = GUILayoutUtility.GetRect(100f, 10000f, PreviewHeight, PreviewHeight);
            float size = Mathf.Min(area.width, area.height);
            var rect = new Rect(area.x + (area.width - size) / 2f, area.y, size, size);
            if (Event.current.type != EventType.Repaint) return;

            EditorGUI.DrawRect(rect, new Color(0.13f, 0.13f, 0.13f));
            if (draft.layers.Count == 0) return;

            float maxExtent = 1f;
            for (int li = 0; li < draft.layers.Count; li++) {
                LayerDraft layer = draft.layers[li];
                RadialMenuStyle ringStyle = draft.style.With(layer.overrides);
                float extent = draft.style.RadiusFor(li, layer.overrides) +
                               ButtonSize(ringStyle.buttonPrefab) * ringStyle.buttonScale *
                               Mathf.Max(1f, ringStyle.selectedScale) / 2f;
                maxExtent = Mathf.Max(maxExtent, extent);
            }

            float pixelsPerUnit = (size / 2f - 6f) / maxExtent;
            Vector2 center = rect.center;
            Color oldHandles = Handles.color;

            for (int li = 0; li < draft.layers.Count; li++) {
                LayerDraft layer = draft.layers[li];
                RadialMenuStyle ringStyle = draft.style.With(layer.overrides);
                float radius = draft.style.RadiusFor(li, layer.overrides) * pixelsPerUnit;
                bool currentLayer = li == selectedLayer;
                float alpha = currentLayer ? 1f : 0.35f;

                Handles.color = new Color(1f, 1f, 1f, 0.25f * alpha);
                Handles.DrawWireDisc(center, Vector3.forward, radius);

                int count = layer.elements.Count;
                if (count == 0) continue;

                int highlighted = currentLayer && selectedElement >= 0 ? selectedElement : 0;
                for (int i = 0; i < count; i++) {
                    ElementDraft element = layer.elements[i];
                    RadialElementVisual visual =
                        RadialElementVisual.Resolve(ringStyle, element.circle, element.icon, element.overrides);
                    bool isHighlighted = i == highlighted;

                    float step = 360f / count * (i - highlighted);
                    float angle = (ringStyle.selectedAngle + (ringStyle.clockwise ? -step : step)) * Mathf.Deg2Rad;
                    Vector2 pos = center + new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * radius;

                    float buttonSize = ButtonSize(visual.buttonPrefab) * visual.scale *
                                       (isHighlighted ? visual.selectedScale : 1f) * pixelsPerUnit;
                    var buttonRect = new Rect(pos - Vector2.one * buttonSize / 2f, Vector2.one * buttonSize);

                    Color background = isHighlighted ? visual.selectedColor : visual.unselectedColor;
                    background.a *= alpha;
                    Sprite backgroundSprite = visual.background
                        ? visual.background
                        : visual.buttonPrefab && visual.buttonPrefab.Background
                            ? visual.buttonPrefab.Background.sprite
                            : null;
                    if (backgroundSprite) {
                        DrawSprite(buttonRect, backgroundSprite, background);
                    }
                    else {
                        Handles.color = background;
                        Handles.DrawSolidDisc(pos, Vector3.forward, buttonSize / 2f);
                    }

                    if (visual.icon) {
                        Color iconColor = isHighlighted ? visual.selectedIconColor : visual.iconColor;
                        iconColor.a *= alpha;
                        float inset = buttonSize * 0.2f;
                        DrawSprite(new Rect(buttonRect.x + inset, buttonRect.y + inset, buttonSize - inset * 2f,
                            buttonSize - inset * 2f), visual.icon, iconColor);
                    }

                    if (!visual.buttonPrefab) {
                        Handles.color = Color.red;
                        Handles.DrawWireDisc(pos, Vector3.forward, buttonSize / 2f + 2f);
                    }
                    else if (currentLayer && i == selectedElement) {
                        Handles.color = new Color(1f, 0.8f, 0.2f);
                        Handles.DrawWireDisc(pos, Vector3.forward, buttonSize / 2f + 3f);
                    }
                }
            }

            Handles.color = oldHandles;
        }

        private static float ButtonSize(RadialButton prefab)
        {
            if (!prefab) return 50f;
            if (prefab.Background) {
                Vector2 bg = prefab.Background.rectTransform.rect.size;
                if (bg.x > 0f) return Mathf.Max(bg.x, bg.y);
            }

            if (prefab.transform is RectTransform rt && rt.sizeDelta.x > 0f) return Mathf.Max(rt.sizeDelta.x, rt.sizeDelta.y);
            return 50f;
        }

        private static void DrawSprite(Rect rect, Sprite sprite, Color color)
        {
            Texture2D texture = sprite.texture;
            if (!texture) return;

            Rect r = sprite.rect;
            var uv = new Rect(r.x / texture.width, r.y / texture.height, r.width / texture.width, r.height / texture.height);
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTextureWithTexCoords(rect, texture, uv, true);
            GUI.color = old;
        }

        // ----------------------------------------------------------------------------------------------------
        // Validation and saving

        private void DrawFooter()
        {
            List<(string message, MessageType type)> issues = Validate();
            foreach (var issue in issues.Take(6)) EditorGUILayout.HelpBox(issue.message, issue.type);
            if (issues.Count > 6) EditorGUILayout.LabelField($"...and {issues.Count - 6} more.", EditorStyles.miniLabel);

            bool blocked = issues.Any(i => i.type == MessageType.Error);
            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(!draft.sourcePrefab || dirty)) {
                    if (GUILayout.Button(new GUIContent("Add To Scene", "Create a controller for the saved menu in the open scene."),
                            GUILayout.Height(28), GUILayout.Width(120))) {
                        _pending = AddToScene;
                    }
                }

                using (new EditorGUI.DisabledScope(blocked)) {
                    string label = draft.sourcePrefab ? "Save Changes" : "Create Menu";
                    if (GUILayout.Button(label, GUILayout.Height(28), GUILayout.Width(140))) _pending = Save;
                }
            }

            EditorGUILayout.Space(4);
        }

        private List<(string, MessageType)> Validate()
        {
            var issues = new List<(string, MessageType)>();
            if (string.IsNullOrWhiteSpace(draft.menuName)) issues.Add(("Give the menu a name.", MessageType.Error));
            if (!draft.sourcePrefab && (draft.folder == null || !(draft.folder == "Assets" || draft.folder.StartsWith("Assets/")))) {
                issues.Add(("The output folder must be inside Assets.", MessageType.Error));
            }

            if (draft.layers.Count == 0) issues.Add(("Add at least one layer.", MessageType.Error));

            for (int li = 0; li < draft.layers.Count; li++) {
                LayerDraft layer = draft.layers[li];
                if (string.IsNullOrWhiteSpace(layer.assetName)) {
                    issues.Add(($"Layer {li + 1} needs an asset name.", MessageType.Error));
                }

                if (layer.elements.Count == 0) {
                    issues.Add(($"Layer {li + 1} ({layer.assetName}) has no elements.", MessageType.Warning));
                }

                RadialMenuStyle ringStyle = draft.style.With(layer.overrides);
                for (int ei = 0; ei < layer.elements.Count; ei++) {
                    ElementDraft element = layer.elements[ei];
                    if (string.IsNullOrWhiteSpace(element.assetName)) {
                        issues.Add(($"Layer {li + 1}, element {ei + 1} needs an asset name.", MessageType.Error));
                    }

                    if (!element.overrides.buttonPrefab.Or(ringStyle.buttonPrefab)) {
                        issues.Add(($"Layer {li + 1}, element {ei + 1} ({element.assetName}) has no button prefab. " +
                                    "Set one on the menu style, the layer or the element.", MessageType.Error));
                    }
                }
            }

            return issues;
        }

        private string AssetsRoot()
        {
            if (draft.sourcePrefab) {
                return Path.GetDirectoryName(AssetDatabase.GetAssetPath(draft.sourcePrefab))?.Replace('\\', '/');
            }

            return $"{(draft.folder ?? "Assets").TrimEnd('/')}/{SafeName(draft.menuName)}";
        }

        private void Save()
        {
            string root = AssetsRoot();
            string ringFolder = $"{root}/Rings";
            string elementFolder = $"{root}/Elements";

            EnsureFolder(root);
            {

                var rings = new List<RadialRing>();
                foreach (LayerDraft layer in draft.layers) {
                    var elements = new List<RadialElement>();
                    foreach (ElementDraft element in layer.elements) {
                        RadialElement asset = element.source
                            ? element.source
                            : CreateAsset<RadialElement>(elementFolder, element.assetName);
                        Undo.RecordObject(asset, "Save Radial Menu");
                        asset.Name = element.displayName;
                        asset.Circle = element.circle;
                        asset.Icon = element.icon;
                        asset.overrides = element.overrides.Clone();
                        EditorUtility.SetDirty(asset);
                        RenameIfNeeded(asset, element.assetName);
                        element.source = asset;
                        elements.Add(asset);
                    }

                    RadialRing ring = layer.source ? layer.source : CreateAsset<RadialRing>(ringFolder, layer.assetName);
                    Undo.RecordObject(ring, "Save Radial Menu");
                    ring.Elements = elements;
                    ring.MenuName = layer.menuName;
                    ring.overrides = layer.overrides.Clone();
                    EditorUtility.SetDirty(ring);
                    RenameIfNeeded(ring, layer.assetName);
                    layer.source = ring;
                    rings.Add(ring);
                }

                // The creator owns the chain: layer order decides NextRing.
                for (int i = 0; i < rings.Count; i++) rings[i].NextRing = i + 1 < rings.Count ? rings[i + 1] : null;
            }

            AssetDatabase.SaveAssets();
            draft.sourcePrefab = SavePrefab(root, draft.layers[0].source);
            AssetDatabase.SaveAssets();

            // Saving filled in source references; undoing past this point would make the next save duplicate assets.
            Undo.ClearUndo(draft);
            _serializedDraft.Update();
            loadTarget = draft.sourcePrefab;
            dirty = false;
            ShowNotification(new GUIContent($"Saved {draft.sourcePrefab.name}"));
            EditorGUIUtility.PingObject(draft.sourcePrefab);
        }

        private GameObject SavePrefab(string root, RadialRing rootRing)
        {
            string path;
            GameObject contents;
            bool existing = draft.sourcePrefab;

            if (existing) {
                path = AssetDatabase.GetAssetPath(draft.sourcePrefab);
                if (draft.sourcePrefab.name != draft.menuName) {
                    string error = AssetDatabase.RenameAsset(path, SafeName(draft.menuName));
                    if (string.IsNullOrEmpty(error)) path = $"{Path.GetDirectoryName(path)?.Replace('\\', '/')}/{SafeName(draft.menuName)}.prefab";
                    else Debug.LogWarning($"Could not rename the menu prefab: {error}");
                }

                contents = PrefabUtility.LoadPrefabContents(path);
            }
            else {
                path = AssetDatabase.GenerateUniqueAssetPath($"{root}/{SafeName(draft.menuName)}.prefab");
                contents = new GameObject(SafeName(draft.menuName), typeof(RectTransform));
            }

            try {
                RadialMenu menu = contents.GetComponent<RadialMenu>();
                if (!menu) menu = contents.AddComponent<RadialMenu>();
                menu.Data = rootRing;
                menu.style = draft.style.Clone();
                return PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally {
                if (existing) PrefabUtility.UnloadPrefabContents(contents);
                else DestroyImmediate(contents);
            }
        }

        private void AddToScene()
        {
            if (!draft.sourcePrefab) return;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (!canvas) {
                var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            var controllerObject = new GameObject($"{draft.sourcePrefab.name} Controller", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(controllerObject, "Add Radial Menu Controller");
            controllerObject.transform.SetParent(canvas.transform, false);

            var controller = controllerObject.AddComponent<RadialMenuController>();
            controller.menuPrefab = draft.sourcePrefab.GetComponent<RadialMenu>();
            controller.menuCanvas = canvas;
            controller.openOnStart = true;

            var input = controllerObject.AddComponent<RadialMenuInputSystem>();
            input.controller = controller;

            Selection.activeGameObject = controllerObject;
            EditorGUIUtility.PingObject(controllerObject);
        }

        // ----------------------------------------------------------------------------------------------------
        // Helpers

        private static IEnumerable<SerializedProperty> Children(SerializedProperty parent)
        {
            SerializedProperty iterator = parent.Copy();
            SerializedProperty end = parent.GetEndProperty();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
                enterChildren = false;
                yield return iterator.Copy();
            }
        }

        private static int CountOverrides(object overrides)
        {
            if (overrides == null) return 0;
            int count = 0;
            foreach (FieldInfo field in overrides.GetType().GetFields()) {
                object value = field.GetValue(overrides);
                FieldInfo enabled = value?.GetType().GetField("enabled");
                if (enabled != null && (bool)enabled.GetValue(value)) count++;
            }

            return count;
        }

        private static T CreateAsset<T>(string folder, string assetName) where T : ScriptableObject
        {
            EnsureFolder(folder);
            var asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath($"{folder}/{SafeName(assetName)}.asset"));
            return asset;
        }

        private static void RenameIfNeeded(Object asset, string wantedName)
        {
            string safe = SafeName(wantedName);
            if (asset.name == safe) return;
            string error = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(asset), safe);
            if (!string.IsNullOrEmpty(error)) Debug.LogWarning($"Could not rename {asset.name} to {safe}: {error}", asset);
        }

        private static void EnsureFolder(string folder)
        {
            folder = folder.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent)) return;
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        private static string SafeName(string name)
        {
            string safe = string.Join("_", (name ?? "").Split(Path.GetInvalidFileNameChars())).Trim();
            return string.IsNullOrEmpty(safe) ? "Unnamed" : safe;
        }

        /// <summary>Absolute path inside this project's Assets folder to an "Assets/..." path; null otherwise.</summary>
        private static string ToProjectPath(string absolute)
        {
            if (string.IsNullOrEmpty(absolute)) return null;
            string full = absolute.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');
            if (full == dataPath) return "Assets";
            return full.StartsWith(dataPath + "/") ? "Assets" + full.Substring(dataPath.Length) : null;
        }
    }

    /// <summary>Adds an "Open in Radial Menu Creator" button to RadialMenu inspectors.</summary>
    [CustomEditor(typeof(RadialMenu))]
    public class RadialMenuInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GameObject go = ((RadialMenu)target).gameObject;
            GameObject prefab = PrefabUtility.IsPartOfPrefabAsset(go)
                ? go.transform.root.gameObject
                : PrefabUtility.GetCorrespondingObjectFromSource(go);

            using (new EditorGUI.DisabledScope(!prefab || !prefab.GetComponent<RadialMenu>())) {
                if (GUILayout.Button("Open in Radial Menu Creator")) RadialMenuCreator.Open(prefab);
            }

            DrawDefaultInspector();
        }
    }
}
