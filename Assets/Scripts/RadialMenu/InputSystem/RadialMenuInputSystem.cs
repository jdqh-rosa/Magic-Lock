using UnityEngine;
using UnityEngine.InputSystem;

namespace MagicLock.Radial
{
    /// <summary>
    /// Drives a <see cref="RadialMenuController"/> from Input System actions. Rebind the actions in the inspector,
    /// or skip this component and call the controller's methods from your own input code.
    /// </summary>
    [AddComponentMenu("Radial Menu/Radial Menu Input (Input System)")]
    public class RadialMenuInputSystem : MonoBehaviour
    {
        public RadialMenuController controller;

        public InputAction turnLeft = Action("Turn Left", "<Keyboard>/leftArrow", "<Gamepad>/dpad/left");
        public InputAction turnRight = Action("Turn Right", "<Keyboard>/rightArrow", "<Gamepad>/dpad/right");
        public InputAction confirm = Action("Confirm", "<Keyboard>/upArrow", "<Keyboard>/enter", "<Gamepad>/buttonSouth");
        public InputAction back = Action("Back", "<Keyboard>/downArrow", "<Keyboard>/backspace", "<Gamepad>/buttonEast");

        private static InputAction Action(string actionName, params string[] bindings)
        {
            var action = new InputAction(actionName, InputActionType.Button);
            foreach (string binding in bindings) action.AddBinding(binding);
            return action;
        }

        private void Reset() => controller = GetComponent<RadialMenuController>();

        private void Awake()
        {
            if (!controller) controller = GetComponent<RadialMenuController>();
        }

        private void OnEnable()
        {
            turnLeft.Enable();
            turnRight.Enable();
            confirm.Enable();
            back.Enable();
        }

        private void OnDisable()
        {
            turnLeft.Disable();
            turnRight.Disable();
            confirm.Disable();
            back.Disable();
        }

        private void Update()
        {
            if (!controller) return;
            if (turnLeft.WasPerformedThisFrame()) controller.TurnLeft();
            if (turnRight.WasPerformedThisFrame()) controller.TurnRight();
            if (confirm.WasPerformedThisFrame()) controller.Confirm();
            if (back.WasPerformedThisFrame()) controller.Back();
        }
    }
}
