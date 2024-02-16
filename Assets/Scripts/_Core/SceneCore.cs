using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class SceneCore : MonoBehaviour
    {
        public static SceneCore Instance { get; private set; }

        public virtual void Initialize_SceneCore()
        {
            Debug.Log("<color=white><b>Initializing scene core...</b></color>");
            Instance = this;
        }
        public virtual void StartScene() { Debug.Log("<color=white><b>Starting scene...</b></color>"); }
        public virtual void UnloadScene() { Debug.Log("<color=white><b>Unloading scene...</b></color>"); }
    }
}
