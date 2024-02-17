using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameInput
{
    public interface ISelectableObject
    {
        Transform transform { get; }
        GameObject gameObject { get; }

        public void OnHoverEnter();
        public void OnHoverExit();
        public void OnSelect();
        public void OnRelease();
    }

    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance {  get; private set; }
        private Input_Default inputActions;
        private bool active = false;

        public void Initialize_Input()
        {
            Debug.Log("<color=white><b>Initializing input manager...</b></color>");
            Instance = this;
            inputActions = new Input_Default();

            inputActions.Default.Pause.started += Pause;
            inputActions.Default.Pause.performed += Pause;
            inputActions.Default.Pause.canceled += Pause;

            inputActions.Default.ToggleHelpDialogue.started += Help;
            inputActions.Default.ToggleHelpDialogue.performed += Help;
            inputActions.Default.ToggleHelpDialogue.canceled += Help;

            inputActions.Default.CameraMovement.started += MoveCamera;
            inputActions.Default.CameraMovement.performed += MoveCamera;
            inputActions.Default.CameraMovement.canceled += MoveCamera;

            inputActions.Default.Select.started += Select;
            inputActions.Default.Select.performed += Select;
            inputActions.Default.Select.canceled += Select;

            inputActions.Enable();
            active = true;
        }





        // Input //
        public bool pause;
        public event Action Event_PauseButtonPressed;
        public event Action Event_HelpButtonPressed;
        public Vector2 moveInput;
        public bool select, pointTo;
        private Transform hoveredObject, selectedObject;

        private void Pause(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                default: break;
                case InputActionPhase.Started:
                    pause = true;
                    if (Event_PauseButtonPressed != null)
                        Event_PauseButtonPressed.Invoke();
                    break;
                case InputActionPhase.Performed: pause = false; break;
                case InputActionPhase.Canceled: pause = false; break;
            }
        }
        private void Help(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                default: break;
                case InputActionPhase.Started:
                    if (Event_HelpButtonPressed != null)
                        Event_HelpButtonPressed.Invoke();
                    break;
            }
        }
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
        private void Select(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                default: break;
                case InputActionPhase.Performed: select = true; break;
                case InputActionPhase.Canceled: select = false; break;
            }
        }




        // Update //
        private void Update()
        {
            if (!active) return;
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 100.0f))
            {
                //Debug.Log("Hit:" + hitInfo.transform.name);
                switch (hitInfo.transform.tag)
                {
                    default:
                        RemoveHoveredObject();
                        if (select) RemoveSelectedObject();
                        break;
                    case "Agent":
                        RemoveHoveredObject(); //In case of hovering directly from agent to agent.
                        if (hitInfo.transform.TryGetComponent(out ISelectableObject selectableObject))
                        {
                            if (select)
                                SetSelectObject(selectableObject);
                            else
                                SetHoveredObject(selectableObject);
                        }
                        else
                        {
                            Debug.LogWarning("No 'ISelectableObject' on agent.");
                            if (select) RemoveSelectedObject();
                        }
                        break;
                }
            }
        }
        private void ForceRemoveSelections()
        {
            RemoveHoveredObject();
            RemoveSelectedObject();
        }

        //Hover
        private void SetHoveredObject(ISelectableObject selectableObject)
        {
            hoveredObject = selectableObject.transform;
            selectableObject.OnHoverEnter();
        }
        private void RemoveHoveredObject()
        {
            if (hoveredObject == null) return;
            hoveredObject.GetComponent<ISelectableObject>().OnHoverExit();
            hoveredObject = null;
        }

        //Selection
        private void SetSelectObject(ISelectableObject selectableObject)
        {
            if (selectedObject == selectableObject.transform) return;
            RemoveSelectedObject();
            selectedObject = selectableObject.transform;
            selectableObject.OnSelect();
        }
        private void RemoveSelectedObject()
        {
            if (selectedObject == null) return;
            selectedObject.GetComponent<ISelectableObject>().OnRelease();
            selectedObject = null;
        }
    }
}
