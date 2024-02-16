using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Components.Agents
{
    public interface IAgentManager
    {
        public void Initialize_AgentManager();
    }
    public interface IAgent
    {
        public void InitializeAgent(IAgentManager agentManager);
    }
}
