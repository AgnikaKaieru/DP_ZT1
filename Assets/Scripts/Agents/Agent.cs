using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Components.Agents;
using GameInput;
using Core;

namespace Agents
{
    public class Agent : MonoBehaviour, IAgent, ISelectableObject
    {
        private IAgentManager agentManager;

        public void InitializeAgent(IAgentManager agentManagerP)
        {
            Debug.Log("<color=white><b>Initializing agent...</b></color>");
            agentManager = agentManagerP;
            GameCore.Instance.Event_Pause += PauseEvent;
        }

        private void PauseEvent(bool b)
        {

        }





        // Selection //
        // ISelectableObject //
        public void OnHoverEnter() { ToggleSelectionArrow(true); }
        public void OnHoverExit() { ToggleSelectionArrow(false); }
        public void OnSelect()
        {
            Debug.Log("Selected: " + gameObject.name);
            //Send data to UI
            ToggleSelectionCircle(true);
        }
        public void OnRelease()
        {
            //Remove data from UI
            ToggleSelectionCircle(false);
        }

        // Selection arrow //
        [Header("Selection arrow")]
        /*Alternative for spawning arrow with address
        [SerializeField] private string selectionArrowAddress = "DefaultSelectionArrow";
        [SerializeField] private float selectionArrowHeight = 2.25f;
        [SerializeField] private string selectionArrowAddress = "DefaultSelectionCircle";*/
        [SerializeField] private GameObject selectionArrow;
        [SerializeField] private GameObject selectionCircle;

        private void ToggleSelectionArrow(bool b) { selectionArrow.SetActive(b); }
        private void ToggleSelectionCircle(bool b) { selectionCircle.SetActive(b); }
    }
}