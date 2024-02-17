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
            pauseIndicator.SetActive(false);
            Initialize_Core();
        }

        // Initialization //
        private void Initialize_Core()
        {
            //Initialize basic components
            Debug.Log("<color=white><b>Initializing core...</b></color>");
            inputManager.Initialize_Input();
            inputManager.Event_PauseButtonPressed += PauseInput;
            inputManager.Event_HelpButtonPressed += HelpInput;

            //Initialize scene core
            GameObject.FindGameObjectWithTag("SceneCore").GetComponent<SceneCore>().Initialize_SceneCore();
            SceneCore.Instance.StartScene();
        }

        // Pause event //
        [Header("PauseEvent")]
        [SerializeField] private GameObject pauseIndicator;
        public bool paused { get; private set; }
        public event Action<bool> Event_Pause;
        public void Pause()
        {
            if (paused) return;
            paused = true;
            if (Event_Pause != null)
                Event_Pause.Invoke(true);

            pauseIndicator.SetActive(true);
        }
        public void Resume()
        {
            if (!paused) return;
            paused = false;
            if (Event_Pause != null)
                Event_Pause.Invoke(false);

            pauseIndicator.SetActive(false);
        }
        private void PauseInput()
        {
            if (!paused) Pause();
            else Resume();
        }

        // UI //
        [Header("UI")]
        public Transform mainCanvas;
        [SerializeField] private GameObject helpDialogue;

        private void HelpInput() { helpDialogue.SetActive(!helpDialogue.activeInHierarchy); }
    }
}