using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuSpawnerDepr : MonoBehaviour
{
    public static RadialMenuSpawnerDepr inst;
    public RadialMenuDepr menuPrefab;


    private void Awake()
    {
        inst = this;
    }

    public RadialMenuDepr SpawnMenu(Interactable obj)
    {
        RadialMenuDepr newMenu = Instantiate(menuPrefab) as RadialMenuDepr;
        newMenu.transform.SetParent(transform, false);
        newMenu.label.text = obj.title.ToUpper();
        newMenu.SpawnButtons(obj);
        return newMenu;
    }
}
