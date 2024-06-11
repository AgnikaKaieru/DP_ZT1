using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Codice.Client.BaseCommands;
using Codice.CM.Common;

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
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        private Cinemachine3rdPersonFollow cinemashineFollow;
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private float cameraMoveSpeed = 10.0f, cameraZoomSpeed = 2.0f, cameraZoomDamping = 1.0f;
        private float cameraDistance, cameraDistanceTarget;
        public Vector2 cameraMoveLimitRange = new Vector2(5,5);
        public Vector2 cameraZoomRange = new Vector2(10,30);

        // Initialization //
        #region Initialization
        public void Initialize_Input()
        {
            Debug.Log("<color=white><b>Initializing input manager...</b></color>");
            Instance = this;
            cinemashineFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            cameraDistanceTarget = cinemashineFollow.CameraDistance;
            cameraZoomRange = new Vector2(10, 30);
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

            inputActions.Default.Scroll.started += CameraZoom;
            inputActions.Default.Scroll.performed += CameraZoom;
            inputActions.Default.Scroll.canceled += CameraZoom;

            inputActions.Default.Select.started += Select;
            inputActions.Default.Select.performed += Select;
            inputActions.Default.Select.canceled += Select;

            inputActions.Enable();
            active = true;
        }
        #endregion

        // Update //
        private void Update()
        {
            if (!active) return;
            Update_Camera();
            Update_ObjectSelection();
        }



        // Input //
        [Header("Input values")]
        public bool pause;
        public event Action Event_PauseButtonPressed;
        public event Action Event_HelpButtonPressed;
        public Vector2 moveInput;
        public Vector2 scrollInput;
        public bool select, pointTo;
        private Transform hoveredObject, selectedObject;

        #region Input events
        #region Input events (Main)
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
        #endregion
        #region Input events (Camera)
        private void MoveCamera(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                default: moveInput = Vector2.zero; break;
                case InputActionPhase.Performed: moveInput = context.ReadValue<Vector2>(); break;
            }
        }
        private void CameraZoom(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                default: scrollInput = Vector2.zero; break;
                case InputActionPhase.Performed: scrollInput = context.ReadValue<Vector2>(); break;
            }
        }
        #endregion
        #region Input events (Agents)
        private void Select(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                default: break;
                case InputActionPhase.Performed: select = true; break;
                case InputActionPhase.Canceled: select = false; break;
            }
        }
        #endregion
        #endregion



        // Camera logic //
        #region Camera
        private void Update_Camera()
        {
            //Camera movement
            if (moveInput != Vector2.zero)
            {
                Vector3 positionModvalue = new Vector3(moveInput.x, 0, moveInput.y) * cameraMoveSpeed * Time.deltaTime;
                Vector3 newPosition = cameraTarget.transform.position + positionModvalue;
                newPosition.x = math.clamp(newPosition.x, -cameraMoveLimitRange.x, cameraMoveLimitRange.x);
                newPosition.z = math.clamp(newPosition.z, -cameraMoveLimitRange.y, cameraMoveLimitRange.y);
                cameraTarget.transform.position = newPosition;
            }

            //Camera zoom
            if (scrollInput.y != 0.0f)
            {
                float zoomAmount = -scrollInput.y * cameraZoomSpeed * Time.deltaTime;
                cameraDistanceTarget = math.clamp(cameraDistanceTarget + zoomAmount, cameraZoomRange.x, cameraZoomRange.y);
            }
            cameraDistance = Mathf.Lerp(cameraDistance, cameraDistanceTarget, cameraZoomDamping * Time.deltaTime);
            cinemashineFollow.CameraDistance = cameraDistance;
        }
        #endregion

        // Object selection //
        #region Selection
        private void Update_ObjectSelection()
        {
            //Object selection
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
        #endregion
    }
}
