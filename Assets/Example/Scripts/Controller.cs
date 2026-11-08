using System.Collections.Generic;
using MagicLock.Radial;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Example game-side controller. Tab switches between Play and Cast; Cast opens the radial menu.
/// Menu input comes from a RadialMenuInputSystem component, added here if the controller lacks one.
/// </summary>
public class Controller : MonoBehaviour
{
    [HideInInspector]
    public ControllerMode Mode;

    public RadialMenuController radialMenuController;

    private void Awake()
    {
        if (radialMenuController && !radialMenuController.GetComponent<RadialMenuInputSystem>()) {
            radialMenuController.gameObject.AddComponent<RadialMenuInputSystem>().controller = radialMenuController;
        }
    }

    private void OnEnable()
    {
        if (radialMenuController) radialMenuController.PathSelected += OnPathSelected;
    }

    private void OnDisable()
    {
        if (radialMenuController) radialMenuController.PathSelected -= OnPathSelected;
    }

    private void Start()
    {
        SetMode(ControllerMode.Cast);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.tabKey.wasPressedThisFrame) {
            SetMode(Mode == ControllerMode.Play ? ControllerMode.Cast : ControllerMode.Play);
        }
    }

    private void OnPathSelected(IReadOnlyList<RadialElement> path)
    {
        Debug.Log($"Selected: {RadialMenuController.JoinNames(path)}", this);
        //GetComponent<Spell>().SetPrefab(...);
    }

    public void SetMode(ControllerMode mode)
    {
        Mode = mode;

        if (radialMenuController) {
            if (mode == ControllerMode.Play) radialMenuController.Close();
            else radialMenuController.Open();
        }

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
