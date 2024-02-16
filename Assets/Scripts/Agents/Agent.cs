using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Components.Agents;

namespace Agents
{
    public class Agent : MonoBehaviour, IAgent
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
    }
}