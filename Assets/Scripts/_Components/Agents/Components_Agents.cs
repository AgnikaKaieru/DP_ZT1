using System;
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
        public string GetAgentName();
        public float GetMoveSpeed();

        //Initialization
        public event Action Event_OnAgentRelease;
        public void InitializeAgent(IAgentManager agentManager, int id);

        //Hp
        public event Action<int> Event_OnHpValueChange;
        public int GetAgentMaxHp();
        public int GetAgentCurrentHp();
        public void Damage(int amount);
    }
}
