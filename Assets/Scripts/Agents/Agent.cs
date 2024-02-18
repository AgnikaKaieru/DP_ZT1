using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.VFX;

using Components.Agents;
using GameInput;
using Core;
using Features;


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
        private int id;
        private string agentName;
        public event Action Event_OnAgentRelease;

        public string GetAgentName() { return agentName; }

        public void InitializeAgent(IAgentManager agentManagerP, int idp)
        {
            Debug.Log("<color=white><b>Initializing agent...</b></color>");
            agentManager = agentManagerP;
            id = idp;
            agentName = id < 10 ? "Entity 0" + id : "Entity " + id;
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
        [Header("Stats")]
        [SerializeField] private int maxHp = 3;
        private int currentHp;
        public event Action<int> Event_OnHpValueChange;

        public int GetAgentMaxHp() { return maxHp; }
        public int GetAgentCurrentHp() { return currentHp; }
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

            if (Event_OnHpValueChange != null)
                Event_OnHpValueChange.Invoke(currentHp);
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
                vfx_WalkTrail.SetInt("SpawnRate", 0);
                return;
            }

            if (math.distance(transform.position, navPath.corners[currentWaypointIndex]) <= destinationReachedThreshold)
            {
                currentWaypointIndex++;
                return;
            }
            
            if(GroundCheck())
            {
                transform.position = Vector3.MoveTowards(transform.position, navPath.corners[currentWaypointIndex], moveSpeed * Time.deltaTime);
                vfx_WalkTrail.SetInt("SpawnRate", 16);
            }
            else
                vfx_WalkTrail.SetInt("SpawnRate", 0);
        }





        [Space(20)]
        [Header("Physics")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider col;

        [SerializeField]  private float groundCheckRadius = 0.4f;
        [SerializeField]  private float groundCheckOffset = 0.0f;
        [SerializeField]  private LayerMask groundCheckLayers;
        private bool GroundCheck()
        {
            if (Physics.CheckSphere(transform.position + new Vector3(0, groundCheckOffset, 0), groundCheckRadius, groundCheckLayers)) return true;
            else return false;
        }
        private void OnCollisionEnter(Collision collision)
        {
            switch (collision.gameObject.tag)
            {
                default: break;
                case "Agent":
                    if (!collision.gameObject.TryGetComponent(out IAgent otherAgent)) { Debug.LogWarning("No 'IAgent' interface on [" + collision.gameObject.name + "]"); return; }
                    otherAgent.Damage(1);

                    //Knock back
                    float3 contactNormal = collision.contacts[0].normal;
                    float knockBackForce = otherAgent.GetMoveSpeed() * 2;
                    contactNormal.y += 1.0f;
                    rb.AddForce(contactNormal * knockBackForce, ForceMode.Impulse);

                    //Agent with higher movementSpeed spawns fx to prevent doubling.
                    if(moveSpeed > otherAgent.GetMoveSpeed())
                    {
                        //FX
                        float fxScale = otherAgent.GetMoveSpeed() * collisionVfxBaseScale;
                        Addressables.InstantiateAsync(collisionVfxAddress, collision.contacts[0].point, quaternion.identity).Completed += (handle) => SpawnCollisionFx(handle, fxScale);

                        ShakeCamera(collision.contacts[0].point);
                    }
                    break;
            }
        }





        // Graphic //
        [Space(20)]
        [Header("Graphic")]
        [SerializeField] private Renderer _renderer;
        private IEnumerator EraseAndRelease()
        {
            vfx_WalkTrail.SetInt("SpawnRate", 0);

            //Erase mesh
            float intensity = 0.0f;
            while (intensity < 1.2f)
            {
                intensity += 1 * Time.deltaTime;
                _renderer.material.SetFloat("_DE_Value", intensity);
                //yield return new WaitForEndOfFrame(); //Does not work for some reason...
                yield return new WaitForFixedUpdate();
            }
            _renderer.material.SetFloat("_DE_Value", 1.4f);

            //Release
            ToggleSelectionCircle(false);
            if (Event_OnAgentRelease != null)
                Event_OnAgentRelease.Invoke();
            yield return new WaitForSeconds(3); //Wait for vfx to diappear.
            agentManager.ReleaseAgent(gameObject);
        }





        // FX //
        [Space(10)]
        [Header("FX")]
        [SerializeField] private VisualEffect vfx_WalkTrail;
        [SerializeField] private string collisionVfxAddress = "VFX_AgentCollision";
        [SerializeField] private float collisionVfxBaseScale = 3, collisionVfxBaseLifetime = 0.05f;
        [SerializeField] private int cameraShakeMaxDistance = 20;
        [SerializeField] private float cameraShakeBaseIntensity = 3;
        [SerializeField] private float cameraShakeBaseDuration = 0.2f;
        private bool lockfx;

        /// <summary>
        /// Prevents fx from doubling
        /// </summary>
        public void LockVFX() { lockfx = true; }
        private void SpawnCollisionFx(AsyncOperationHandle<GameObject> handle, float scale)
        {
            if (lockfx) return;
            GameObject fxObject = handle.Result;
            VisualEffect vfx = fxObject.GetComponent<VisualEffect>();
            vfx.SetFloat("Scale", scale);
            vfx.SendEvent("Play");
        }
        private void ShakeCamera(Vector3 originPoint)
        {
            float distanceFromCamera = math.distance(originPoint, Camera.main.transform.position);
            //Debug.Log("Distance of hit: " + distanceFromCamera);
            if (distanceFromCamera < cameraShakeMaxDistance)
            {
                float distanveMod = (cameraShakeMaxDistance - distanceFromCamera) * 0.1f;
                float intensity = cameraShakeBaseIntensity * distanveMod;
                float duration = cameraShakeBaseDuration * distanveMod;
                CinemachineShake.Instance.ShakeCamera(intensity, duration);
            }
        }




        // Selection //
        // ISelectableObject //
        public void OnHoverEnter() { ToggleSelectionArrow(true); }
        public void OnHoverExit() { ToggleSelectionArrow(false); }
        public void OnSelect()
        {
            UI_AgentInfo.Instance.SetAgent(this);
            ToggleSelectionCircle(true);
        }
        public void OnRelease()
        {
            UI_AgentInfo.Instance.SetAgent(null);
            ToggleSelectionCircle(false);
        }

        // Selection arrow //
        [Space(20)]
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

            //Draw ground check sphere
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + new Vector3(0, groundCheckOffset, 0), groundCheckRadius);

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