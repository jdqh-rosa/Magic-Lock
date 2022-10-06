using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    //Menu
    public RadialMenu RadialMenuPrefab;

    [HideInInspector]
    public ControllerMode Mode;

    public RadialMenuController radialMenuController;

    void Start()
    {
        SetMode(ControllerMode.Cast);
        //radialMenuController = new RadialMenuController();
        //radialMenuController.RadialMenuPrefab = RadialMenuPrefab;
        radialMenuController.SpawnMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mode == ControllerMode.Cast) {

            if (Input.GetKeyDown(KeyCode.Space)) {
                //SetMode(ControllerMode.Menu);
                //radialMenuController.SpawnMenu();
            }
            if (Input.GetKeyUp(KeyCode.Space)) {
                //radialMenuController.DestroyMenu();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                radialMenuController.TurnMenuRight();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                radialMenuController.TurnMenuLeft();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                radialMenuController.NextRing();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                radialMenuController.PreviousRing();
            }
        }

        if (Mode == ControllerMode.Play) {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                SetMode(ControllerMode.Cast);
            }
        }
        else if (Mode == ControllerMode.Cast || Mode == ControllerMode.Menu) {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                SetMode(ControllerMode.Play);
            }
        }

        if (Mode == ControllerMode.Cast || Mode == ControllerMode.Play) {

            //do other controller stuff here

        }
    }

    private void MenuClick(string path)
    {
        Debug.Log(path);
        var paths = path.Split('/');
        //GetComponent<Spell>().SetPrefab(int.Parse(paths[1]), int.Parse(paths[2]));
        SetMode(ControllerMode.Cast);
    }
    public void SetMode(ControllerMode mode)
    {
        Mode = mode;

        switch (mode) {
            case ControllerMode.Cast:
                //Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;
                break;
            case ControllerMode.Menu:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case ControllerMode.Play:
                //Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;
                break;
        }
    }
    public enum ControllerMode
    {
        Play, Cast, Menu
    }
}
