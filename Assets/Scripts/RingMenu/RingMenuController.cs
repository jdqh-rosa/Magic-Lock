using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingMenuController : MonoBehaviour
{
    //Menu
    public RingMenuMB MainMenuPrefab;
    protected RingMenuMB MainMenuInstance;
    [HideInInspector]
    public ControllerMode Mode;

    void Start()
    {
        SetMode(ControllerMode.Play);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) {
            SetMode(ControllerMode.Menu);
            MainMenuInstance = Instantiate(MainMenuPrefab, FindObjectOfType<Canvas>().transform);
            MainMenuInstance.callback = MenuClick;
        }

        if (Mode == ControllerMode.Play) {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                SetMode(ControllerMode.Build);
            }
        }
        else if (Mode == ControllerMode.Build || Mode == ControllerMode.Menu) {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                SetMode(ControllerMode.Play);
            }
        }

        if(Mode == ControllerMode.Build || Mode == ControllerMode.Play) {




        }
    }

    private void MenuClick(string path)
    {
        Debug.Log(path);
        var paths = path.Split('/');
        //GetComponent<Spell>().SetPrefab(int.Parse(paths[1]), int.Parse(paths[2]));
        SetMode(ControllerMode.Build);
    }

    public void SetMode(ControllerMode mode)
    {
        Mode = mode;
        if (mode != ControllerMode.Menu && MainMenuInstance != null) {
            Destroy(MainMenuInstance);
        }

        switch (mode)
        {
            case ControllerMode.Build:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case ControllerMode.Menu:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case ControllerMode.Play:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
        }
    }

    public enum ControllerMode
    {
        Play, Build, Menu
    }
}
