using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RadialMenuCreator : EditorWindow
{
    string menuName;
    int layerCount;
    string[] layerNames;
    int[] elementCount;
    string[][] elementNames;
    Sprite[][] elementIcons;

    [MenuItem("RadialMenu/Creator")]
    public static void ShowWindow()
    {
        GetWindow<RadialMenuCreator>();
    }

    public void OnGUI()
    {
        GUILayout.Label("Radial Menu Creator");
        layerCount = EditorGUILayout.IntField("Layer count", layerCount);
        layerNames = new string[layerCount]; 
        elementCount = new int[layerCount];
        for(int i = 0; i < layerCount; i++)
        {
            layerNames[i] = EditorGUILayout.TextField("Layer Name", layerNames[i]);
            elementCount[i] = EditorGUILayout.IntField("Element Amount", elementCount[i]);
            elementNames = new string[i][];
            elementIcons = new Sprite[i][];
            for(int j=0; j<elementCount[i]; j++)
            {
                //assign element values
            }
        }

        //button that creates prefab menu in assigned unity folder

    }
}
