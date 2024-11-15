using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

public class RadialMenuLayerEditorWindow : EditorWindow
{
    private RadialMenu layer;
    private int layerIndex;
    private ReorderableList list;

    public void SetLayer(RadialMenu layer, int layerIndex)
    {
        this.layer = layer;
        this.layerIndex = layerIndex;
        list = new ReorderableList(layer.Data.Elements, typeof(RadialElement), true, true, true, true);

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += 2;

            // Get the RadialElement instance
            var element = layer.Data.Elements[index];

            // Display fields from the RadialElement scriptable object
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "Element:");

            // Draw the RadialElement object itself
            layer.Data.Elements[index] = (RadialElement)EditorGUI.ObjectField(
                new Rect(rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                element, typeof(RadialElement), false);
        };
    }

    private void OnGUI()
    {
        if (!layer)
        {
            EditorGUILayout.LabelField("No Radial Menu Layer assigned.");
            return;
        }

        EditorGUILayout.LabelField($"Radial Menu Layer {layerIndex + 1} Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Editable fields for RadialMenu properties
        layer.GapWidthDegree = EditorGUILayout.FloatField("Gap Width Degree", layer.GapWidthDegree);
        layer.radius = EditorGUILayout.FloatField("Radius", layer.radius);

        if (!layer.Data)
        {
            return;
        }

        list.DoLayoutList();
        /*
        if (layer.Data)
        {
            EditorGUILayout.LabelField("Radial Elements", EditorStyles.boldLabel);
            for (int i = 0; i < layer.Data.Elements.Length; i++)
            {
                layer.Data.Elements[i] = (RadialElement)EditorGUILayout.ObjectField($"Element {i + 1}",
                    layer.Data.Elements[i], typeof(RadialElement), true);
            }
        }

        if (GUILayout.Button("Add Element"))
        {
            layer.Data.AddElement(ScriptableObject.CreateInstance<RadialElement>());
        }*/

        // Save button
        if (GUILayout.Button("Save Changes"))
        {
            EditorUtility.SetDirty(layer);
            if (layer.Data)
            {
                EditorUtility.SetDirty(layer.Data);
                foreach (var element in layer.Data.Elements)
                {
                    if (element)
                    {
                        EditorUtility.SetDirty(element);
                    }
                }
            }
        }
    }
}