using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

using Components.Agents;
using GameInput;
using Core;

namespace Agents
{
    [SelectionBase]
    public class Agent : MonoBehaviour, IAgent, ISelectableObject
    {
        private IAgentManager agentManager;
        private enum AgentState { init, idle, moving, despawning }
#pragma warning disable 0649
        private AgentState agentState = AgentState.init;
#pragma warning disable 0649

        public void InitializeAgent(IAgentManager agentManagerP)
        {
            Debug.Log("<color=white><b>Initializing agent...</b></color>");
            agentManager = agentManagerP;
            navPath = new NavMeshPath();

            moveSpeed = UnityEngine.Random.Range(1.0f, 3.0f);
            currentHp = maxHp;

            agentState = AgentState.idle;
            GameCore.Instance.Event_Pause += PauseEvent;
            paused = GameCore.Instance.paused;
        }

        private bool paused = true;
        private void PauseEvent(bool b)
        {
            paused = b;
        }





        // Stats //
        [SerializeField] private int maxHp = 3;
        private int currentHp;
        public void Damage(int amount) 
        {
            currentHp -= amount;
            if(currentHp<= 0)
            {
                rb.isKinematic = true;
                col.enabled = false;
                agentState = AgentState.despawning;
                StartCoroutine(EraseAndRelease());
            }
        }




        // Logic //
        private float waitTimeout = 1.0f, waitTimeoutDelta =1.0f;

        private void Update()
        {
            if (paused) return;

            switch(agentState)
            {
                default: break;
                case AgentState.idle:
                    if (waitTimeoutDelta > 0.0f)
                    {
                        waitTimeoutDelta -= Time.deltaTime;
                        return;
                    }
                    SelectDestination();
                    waitTimeoutDelta = waitTimeout;
                    break;
                case AgentState.moving: Move(); break;
            }
        }





        // Navigation //
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 1.0f;
        private NavMeshPath navPath;
        private float3 destinationPoint;
        private float destinationReachedThreshold = 0.5f;
        private int currentWaypointIndex = 1;

        public float GetMoveSpeed() { return moveSpeed; }
        private void SelectDestination()
        {
            bool foundDestination = false;
            destinationPoint = agentManager.GetPointInsideArea();

            //Sample position
            if(NavMesh.SamplePosition(destinationPoint, out NavMeshHit hit, 100.0f, NavMesh.AllAreas))
            {
                foundDestination = true;
                NavMesh.CalculatePath(transform.position, destinationPoint, NavMesh.AllAreas, navPath);
            }

            if (foundDestination)
                agentState = AgentState.moving;
            else
                agentState = AgentState.idle;
        }
        private void Move()
        {
            if (currentWaypointIndex >= navPath.corners.Length)
            {
                currentWaypointIndex = 1;
                agentState = AgentState.idle;
                return;
            }

            if (math.distance(transform.position, navPath.corners[currentWaypointIndex]) <= destinationReachedThreshold)
            {
                currentWaypointIndex++;
            }
            else
                transform.position = Vector3.MoveTowards(transform.position, navPath.corners[currentWaypointIndex], moveSpeed * Time.deltaTime);
        }





        [Header("Physics")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider col;

        private void OnCollisionEnter(Collision collision)
        {
            switch (collision.gameObject.tag)
            {
                default: break;
                case "Agent":
                    float3 contactNormal = collision.contacts[0].normal;
                    float knockBackForce;
                    if (collision.gameObject.TryGetComponent(out IAgent agent))
                    {
                        knockBackForce = agent.GetMoveSpeed() * 2;
                        agent.Damage(1);
                    }
                    else
                        knockBackForce = 4;

                    contactNormal.y += 1.0f;
                    rb.AddForce(contactNormal * knockBackForce, ForceMode.Impulse);
                    break;
            }
        }

        // Graphic //
        [SerializeField] private Renderer _renderer;
        private IEnumerator EraseAndRelease()
        {
            float intensity = 0.0f;
            while (intensity < 1.0f)
            {
                intensity += 1 * Time.deltaTime;
                _renderer.material.SetFloat("_DE_Value", intensity);
                //yield return new WaitForEndOfFrame(); //Does not work for some reason...
                yield return new WaitForFixedUpdate();
            }
            agentManager.ReleaseAgent(gameObject);
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





        // Editor //
#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField] private bool debug = true;
        void OnDrawGizmosSelected()
        {
            if (!debug) return;
            //Draw path
            if (navPath == null || navPath.corners.Length < 2) return;
            Gizmos.color = Color.red;
            for (int i = 1; i< navPath.corners.Length; i++)
            {
                float3 pointA = navPath.corners[i - 1];
                float3 pointB = navPath.corners[i];
                Gizmos.DrawLine(pointA, pointB);
            }
        }
#endif
    }
}