using GameInput;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class GameCore : MonoBehaviour
    {
        public static GameCore Instance { get; private set; }
        [SerializeField] private InputManager inputManager;

        private void Awake()
        {
            Instance = this;
            Initialize_Core();
        }

        // Initialization //
        private void Initialize_Core()
        {
            //Initialize basic components
            Debug.Log("<color=white><b>Initializing core...</b></color>");
            inputManager.Initialize_Input();

            //Initialize scene core
            GameObject.FindGameObjectWithTag("SceneCore").GetComponent<SceneCore>().Initialize_SceneCore();
            SceneCore.Instance.StartScene();
        }

        // Pause event //
        public bool paused { get; private set; }
        public event Action<bool> Event_Pause;
        public void Pause()
        {
            if (paused) return;
            paused = true;
            Event_Pause.Invoke(true);
        }
        public void Resume()
        {
            if (!paused) return;
            paused = false;
            Event_Pause.Invoke(false);
        }

        // UI //
        [Header("UI")]
        public Transform mainCanvas;
    }
}