using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ResourceManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Components.Agents;

namespace Core
{
    public class SceneCore_LvlA : SceneCore
    {
        private IAgentManager agentManager;

        // Initialization //
        public override void Initialize_SceneCore()
        {
            base.Initialize_SceneCore();

            agentManager = GetComponent<IAgentManager>();
            agentManager.Initialize_AgentManager();
        }

        // Scene start //
        public override void StartScene()
        {
            base.StartScene();

            GameCore.Instance.Resume();
        }
    }
}
