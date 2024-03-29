using System.Collections;
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

    private void Start()
    {
        if (createBool)
        {
            menuLayers = new List<RadialMenu>();
        }
    }

    override public void Update() { }

    public void SpawnMenu()
    {
        index = 0;

        if (!createBool)
        {
            menuLayers = new List<RadialMenu>();
            RadialMenu newMenu = Instantiate(RadialMenuPrefab, FindObjectOfType<Canvas>().transform);
            menuLayers.Add(newMenu);
        }
        menuLayers[index].SpawnButtons();

        RadialRing swa = menuLayers[index].Data;
        int i = 1;
        while(swa.NextRing != null)
        {
            swa = swa.NextRing;
            i++;
        }
        paths = new string[i];
    }
    public string DestroyMenu()
    {
        var path = menuLayers[index].Path;

        foreach (var layer in menuLayers)
        {
            Destroy(layer.gameObject);
        }
        menuLayers.Clear();

        return path;
    }
    public void TurnMenuRight()
    {
        menuLayers[index].TurnMenu(-1);
    }
    public void TurnMenuLeft()
    {
        menuLayers[index].TurnMenu(1);
    }

    public string NextRing()
    {
        var next = menuLayers[index].Data.NextRing;
        if (next != null)
        {
            Debug.Log(string.Format("index:{0} , count:{1}", index + 1, menuLayers.Count));
            if (index + 1 == menuLayers.Count)
            {
                menuLayers.Add(menuLayers[index].NextRing(null));
                paths[index] = menuLayers[index].Path;
                menuLayers[index + 1].SpawnButtons();
            }
            else
            {
                menuLayers[index].NextRing(menuLayers[index].Data);
                paths[index] = menuLayers[index].Path;
                menuLayers[index + 1].gameObject.SetActive(true);
                menuLayers[index + 1].TurnMenu(0);
            }
            index++;
            return null;
        }
        else
        {
            menuLayers[index].NextRing(menuLayers[index].Data);
            paths[index] = menuLayers[index].Path;
            string ss = "";
            foreach(string s in paths)
            {
                ss += s + ", ";
            }
            Debug.Log(ss);
            return menuLayers[index].Path;
        }
    }

    public void PreviousRing()
    {
        if (index <= 0) return;
        menuLayers[index].gameObject.SetActive(false);
        //menuLayers.Remove(menuLayers[index]);
        --index;
    }
}
