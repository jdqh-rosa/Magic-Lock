using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuSpawner : MonoBehaviour
{
    public static RadialMenuSpawner inst;
    public RadialMenu menuPrefab;


    private void Awake()
    {
        inst = this;
    }

    public RadialMenu SpawnMenu(Interactable obj)
    {
        RadialMenu newMenu = Instantiate(menuPrefab) as RadialMenu;
        newMenu.transform.SetParent(transform, false);
        newMenu.label.text = obj.title.ToUpper();
        newMenu.SpawnButtons(obj);
        return newMenu;
    }
}
