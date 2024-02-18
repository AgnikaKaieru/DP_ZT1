using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Features
{
    public class ObjectAutoDespawn : MonoBehaviour
    {
        [Tooltip("Time for object to despawn in 's'.")]
        [SerializeField] private float timeToDespawn = 2.0f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(timeToDespawn);
            Addressables.ReleaseInstance(gameObject);
        }
    }
}
