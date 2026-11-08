using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MagicLock.Radial.Editor
{
    /// <summary>Edits the elements of one <see cref="RadialRing"/> asset.</summary>
    public class RadialMenuLayerEditorWindow : EditorWindow
    {
        [SerializeField] private RadialRing ring;
        [SerializeField] private int layerIndex;

        private SerializedObject _serializedRing;
        private ReorderableList _list;

        public void SetRing(RadialRing pRing, int pLayerIndex)
        {
            ring = pRing;
            layerIndex = pLayerIndex;
            _serializedRing = null;
        }

        private void EnsureList()
        {
            if (_serializedRing != null && _serializedRing.targetObject == ring) return;

            _serializedRing = new SerializedObject(ring);
            _list = new ReorderableList(_serializedRing, _serializedRing.FindProperty(nameof(RadialRing.Elements)),
                true, true, true, true);
            _list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Elements");
            _list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, _list.serializedProperty.GetArrayElementAtIndex(index),
                    new GUIContent($"Element {index + 1}"));
            };
        }

        private void OnGUI()
        {
            if (!ring) {
                EditorGUILayout.LabelField("No radial ring assigned.");
                return;
            }

            EnsureList();
            _serializedRing.Update();

            EditorGUILayout.LabelField($"Layer {layerIndex + 1}: {ring.name}", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_serializedRing.FindProperty(nameof(RadialRing.MenuName)));
            _list.DoLayoutList();

            _serializedRing.ApplyModifiedProperties();

            if (GUILayout.Button("Save Ring Asset")) {
                AssetDatabase.SaveAssetIfDirty(ring);
            }
        }
    }
}
