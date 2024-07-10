using System.Collections.Generic;
using UnityEngine;

public class RadialMenuController : Interactable
{
    public RadialMenu RadialMenuPrefab;
    protected RadialMenu RadialMenuInstance;

    public List<RadialMenu> menuLayers;
    private int index = 0;

    public bool createBool = false;

    public string[] paths;

    public float layerRadiusDifference = 100f;

    private void Start() {
        //if (createBool) {
        //    menuLayers = new List<RadialMenu>();
        //}
    }

    override public void Update() {
        Debug.Log(menuLayers?.Count ?? 0);
    }

    public void CreateMenu() {
        if (menuLayers == null) {
            menuLayers = new List<RadialMenu>();
        }

        RadialMenuInstance = Instantiate(RadialMenuPrefab, FindObjectOfType<Canvas>().transform);
        if (menuLayers.Count > 0) {
            menuLayers[menuLayers.Count - 1].Data.NextRing = RadialMenuInstance.Data;
        }
        menuLayers.Add(RadialMenuInstance);

        RadialRing swa = RadialMenuInstance.Data;
        int i = 1;
        while (swa.NextRing != null) {
            swa = swa.NextRing;
            i++;
        }
        if (paths == null || paths.Length > i) {
            paths = new string[i];
        }
        else {
            paths.CopyTo(paths = new string[i], 0);
        }
    }

    public void SpawnMenu() {
        if (createBool) CreateMenu();

        if (menuLayers == null || menuLayers.Count == 0) {
            Debug.LogWarning("Menu layers have not been created. Please create the menu first.");
            return;
        }

        //if (!createBool) {
        //    RadialMenu newMenu = Instantiate(RadialMenuPrefab, FindObjectOfType<Canvas>().transform);
        //    newMenu.Data = RadialMenuInstance.Data;
        //    newMenu.GapWidthDegree = RadialMenuInstance.GapWidthDegree;
        //    newMenu.radius = RadialMenuInstance.radius;
        //    // Copy other properties as needed

        //    menuLayers[index] = newMenu;
        //}

        menuLayers[index].gameObject.SetActive(true);

        menuLayers[index].SpawnButtons();
    }

    public string DestroyMenu() {
        var path = menuLayers[index].Path;

        foreach (var layer in menuLayers) {
            DestroyImmediate(layer.gameObject);
        }
        menuLayers.Clear();

        return path;
    }

    public void TurnMenuRight() {
        menuLayers[index].TurnMenu(-1);
    }

    public void TurnMenuLeft() {
        menuLayers[index].TurnMenu(1);
    }

    public string NextRing() {
        var next = menuLayers[index].Data.NextRing;
        if (next != null) {
            Debug.Log(string.Format("index:{0} , count:{1}", index + 1, menuLayers.Count));
            if (index + 1 == menuLayers.Count) {
                menuLayers.Add(menuLayers[index].NextRing());
                paths[index] = menuLayers[index].Path;
                menuLayers[index + 1].Data = menuLayers[index].Data.NextRing;
                menuLayers[index + 1].radius = menuLayers[index].radius + layerRadiusDifference;
                menuLayers[index + 1].callback = menuLayers[index].callback;
                menuLayers[index + 1].SpawnButtons();
            }
            else {
                menuLayers[index].NextRing(false);
                paths[index] = menuLayers[index].Path;
                menuLayers[index + 1].gameObject.SetActive(true);
            }
            index++;
            return null;
        }
        else {
            menuLayers[index].NextRing();
            paths[index] = menuLayers[index].Path;
            string ss = "";
            foreach (string s in paths) {
                ss += s + ", ";
            }
            Debug.Log(ss);
            return menuLayers[index].Path;
        }
    }

    public void PreviousRing() {
        if (index <= 0) return;
        menuLayers[index].gameObject.SetActive(false);
        --index;
    }
}