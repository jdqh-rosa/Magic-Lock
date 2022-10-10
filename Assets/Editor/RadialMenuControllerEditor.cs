//C# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(RadialMenuController))]
[CanEditMultipleObjects]
public class RadialMenuControllerEditor : Editor
{
    /*
    RadialMenuController radialMC;
    SerializedProperty createBool;


    void OnEnable()
    {
        createBool = serializedObject.FindProperty("createBool");
    }

    //stop whatever you're trying to do here and just display everything using for loops
    int layerInt = 0;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        RadialMenuController radialMC = (RadialMenuController)target;
        List<RadialMenu> menuLayers = new List<RadialMenu>();

        menuLayers = radialMC.menuLayers;

        EditorGUILayout.PropertyField(createBool);
        serializedObject.ApplyModifiedProperties();
        if (createBool.boolValue)
        {
            layerInt = EditorGUILayout.IntField(layerInt);
            for (int i = 0; i < layerInt; i++)
            {
                if (i > menuLayers.Count - 1)
                {
                    RadialMenu menu = Instantiate(radialMC.RadialMenuPrefab, FindObjectOfType<Canvas>().transform);
                    menuLayers.Add(menu);
                }
                //use gui gameobject container to get elements
                Debug.Log(menuLayers[i].ToString());
                //menuLayers[1].Data.Elements.
                SerializedProperty elementsProperty = serializedObject.FindProperty("menuLayers[i].Data");
                EditorGUILayout.PropertyField(elementsProperty, true);
                if (i + 1 < menuLayers.Count)
                {
                    menuLayers[i].Data.NextRing = menuLayers[i + 1].Data;
                }
                EditorGUILayout.PropertyField(elementsProperty, true);
            }
            if (menuLayers.Count > layerInt + 1)
            {
                for (int i = layerInt; i < menuLayers.Count; i++)
                {
                    Destroy(menuLayers[i]);
                    menuLayers.RemoveAt(i);
                }
            }
        }
    }
    */
}