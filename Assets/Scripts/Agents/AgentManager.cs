using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Components.Agents;
using Core;

namespace Agents
{
    public class AgentManager : MonoBehaviour, IAgentManager
    {
        [Header("Playable area")]
        [Tooltip("Size of area agents can move on.")]
        [SerializeField] private float2 playableAreaSize = new float2(10, 10);

        [Header("Agents")]
        [SerializeField] private string agentAddress = "DefaultAgent";
        [Tooltip("Minimum and maximum amount agents spawning at the start. (X-Minimum, Y-Maximum)")]
        [SerializeField] private int2 initialAgentAmountRange = new int2(3, 5);
        [Tooltip("Maximum number of agents that can spawn.")]
        [SerializeField] private int maxAgents = 30;
        [Tooltip("Minimum and maximum amount of time for agent to spawn. (X-Minimum, Y-Maximum)")]
        [SerializeField] private float2 newAgentSpawnTimeoutRange = new float2(2, 6);
        private float2 areaExtent;
        private List<GameObject> agentList = new List<GameObject>();
        private int idCounter = 0;


        // Initialization //
        public void Initialize_AgentManager()
        {
            Initialize_PlayableArea();

            //Spawn and initialize agents
            int amountToSpawn = UnityEngine.Random.Range(initialAgentAmountRange.x, initialAgentAmountRange.y + 1);
            for (int i = amountToSpawn; i > 0; i--)
                SpawnAgent();

            //Start spawn timer
            StartCoroutine(StartAgentSpawnTimer());
        }
        private IEnumerator StartAgentSpawnTimer()
        {
            float secondsToSpawn = UnityEngine.Random.Range(newAgentSpawnTimeoutRange.x, newAgentSpawnTimeoutRange.y + 1);
            yield return new WaitForSeconds(secondsToSpawn);
            SpawnAgent();
            StartCoroutine(StartAgentSpawnTimer());
        }



        // Playable area //
        private void Initialize_PlayableArea()
        {
            playableAreaSize = math.clamp(playableAreaSize, new float2(1, 1), new float2(100, 100));
            areaExtent.x = playableAreaSize.x / 2;
            areaExtent.y = playableAreaSize.y / 2;
        }
        /// <summary>
        /// Returns point inside area on specified height.
        /// </summary>
        public float3 GetPointInsideArea(float pointY)
        {
            float pointX = UnityEngine.Random.Range(-areaExtent.x, areaExtent.x);
            float pointZ = UnityEngine.Random.Range(-areaExtent.y, areaExtent.y);
            float3 point = new float3(pointX, pointY, pointZ);
            return point;
        }
        /// <summary>
        /// Returns point inside area on height of 0.
        /// </summary>
        public float3 GetPointInsideArea() { return GetPointInsideArea(0); }



        // Agent load/unload //
        private void SpawnAgent()
        {
            if (agentList.Count >= maxAgents) { Debug.LogWarning("Max agent amount reached!"); return; }

            float3 spawnPosition = GetPointInsideArea(0.2f);
            Addressables.InstantiateAsync(agentAddress, spawnPosition, quaternion.identity).Completed += (handle) => InstantiateAgent(handle);
        }
        private void InstantiateAgent(AsyncOperationHandle<GameObject> handle)
        {
            GameObject agent = handle.Result;
            agentList.Add(agent);
            agent.GetComponent<IAgent>().InitializeAgent(this, idCounter);
            idCounter++;
        }
        public void ReleaseAgent(GameObject agent)
        {
            agentList.Remove(agent);
            Addressables.ReleaseInstance(agent);
        }





        // Editor //
#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField] private bool debug = true;
        void OnDrawGizmosSelected()
        {
            if (!debug) return;
            //Draw playable area size.
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new float3(playableAreaSize.x, 2, playableAreaSize.y));
        }
#endif
    }
}
