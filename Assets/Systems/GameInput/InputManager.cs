using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameInput
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance {  get; private set; }
        private Input_Default inputActions;

        public void Initialize_Input()
        {
            Debug.Log("<color=white><b>Initializing input manager...</b></color>");
            Instance = this;
            inputActions = new Input_Default();

            inputActions.Default.CameraMovement.started += MoveCamera;
            inputActions.Default.CameraMovement.performed += MoveCamera;
            inputActions.Default.CameraMovement.canceled += MoveCamera;

            inputActions.Enable();
        }

        public Vector2 moveInput;
        private void MoveCamera(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            switch (context.phase)
            {
                default: break;
                case InputActionPhase.Performed:
                    Debug.Log("Input test: " + moveInput);
                    break;
            }
        }
    }
}
