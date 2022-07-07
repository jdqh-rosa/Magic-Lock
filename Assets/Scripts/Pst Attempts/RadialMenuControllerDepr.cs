using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuControllerDepr : Interactable
{
    public RadialMenuDepr[] menuLayers = new RadialMenuDepr[2];
    private int currentLayer = 0;
    override public void Update()
    {
        base.Update();

        menuLayers[0] = radialMenu;

        if (radialMenu != null) {
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                menuLayers[currentLayer].TurnMenu(-1);
            }    
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                menuLayers[currentLayer].TurnMenu(1);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                currentLayer = (currentLayer >= menuLayers.Length-1) ? currentLayer : currentLayer + 1;
                if (menuLayers[currentLayer] == null) {
                    menuLayers[currentLayer] = RadialMenuSpawnerDepr.inst.SpawnMenu(this);
                    menuLayers[currentLayer].radius += 50;
                }
                menuLayers[currentLayer].gameObject.SetActive(true);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                currentLayer = (currentLayer <= 0) ? currentLayer : currentLayer - 1;
                menuLayers[currentLayer+1].gameObject.SetActive(false);
            }
        }
        else {
            currentLayer=0;
        }
    }
}
