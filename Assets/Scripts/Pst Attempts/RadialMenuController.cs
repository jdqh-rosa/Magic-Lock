using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuController : Interactable
{
    public RadialMenu[] menuLayers = new RadialMenu[2];
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
                    menuLayers[currentLayer] = RadialMenuSpawner.inst.SpawnMenu(this);
                    menuLayers[currentLayer].radius += 50;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                currentLayer = (currentLayer <= 0) ? currentLayer : currentLayer - 1;
            }
        }
        else {
            currentLayer=0;
        }
    }
}
