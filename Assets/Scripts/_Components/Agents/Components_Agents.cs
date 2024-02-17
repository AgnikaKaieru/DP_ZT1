using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Components.Agents
{
    public interface IAgentManager
    {
        public void Initialize_AgentManager();

        public float3 GetPointInsideArea();

        public void ReleaseAgent(GameObject agent);
    }
    public interface IAgent
    {
        public void InitializeAgent(IAgentManager agentManager);

        public float GetMoveSpeed();

        public void Damage(int amount);
    }
}
