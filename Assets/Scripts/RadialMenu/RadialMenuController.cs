using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuController : Interactable
{
    public RadialMenu RadialMenuPrefab;
    protected RadialMenu RadialMenuInstance;

    private List<RadialMenu> menuLayers;
    private int index = 0;

    // Update is called once per frame
    override public void Update()
    {
        base.Update();
    }

    public void SpawnMenu()
    {
        menuLayers = new List<RadialMenu>();
        index = 0;
        RadialMenu newMenu = Instantiate(RadialMenuPrefab, FindObjectOfType<Canvas>().transform);
        //menuLayers[index] = newMenu;
        menuLayers.Add(newMenu);
        newMenu.SpawnButtons();
    }
    public string DestroyMenu()
    {
        var path = menuLayers[index].Path;

        foreach(var layer in menuLayers) {
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
        if (next != null) {
            Debug.Log(string.Format("index:{0} , count:{1}", index+1, menuLayers.Count));
            if (index+1 == menuLayers.Count) {
                menuLayers.Add(menuLayers[index].NextRing());
                menuLayers[index+1].SpawnButtons();
            }
            else {
                menuLayers[index+1].gameObject.SetActive(true);
            }
            index++;
            return null;
        }
        else {
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
