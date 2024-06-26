using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameInput;

namespace Core
{
    public class GameCore : MonoBehaviour
    {
        public static GameCore Instance { get; private set; }
        [SerializeField] private InputManager inputManager;

        private void Awake()
        {
            if(GameCore.Instance != null) { Debug.LogWarning("'GameGore' instance already exists."); Destroy(gameObject); return; }
            Instance = this;
            pauseIndicator.SetActive(false);
            Initialize_Core();
        }

        // Initialization //
        #region Initialization
        private void Initialize_Core()
        {
            //Initialize basic components
            Debug.Log("<color=white><b>Initializing core...</b></color>");
            inputManager.Initialize_Input();
            inputManager.Event_PauseButtonPressed += PauseInput;
            inputManager.Event_HelpButtonPressed += HelpInput;
        }
        public void AssignSceneCore()
        {
            //Initialize scene Core
            SceneCore.Instance.Initialize_SceneCore();
            SceneCore.Instance.StartScene();
        }
        #endregion

        // Pause event //
        #region Pause
        [Header("PauseEvent")]
        [SerializeField] private GameObject pauseIndicator;
        public bool paused { get; private set; }
        public event Action<bool> Event_Pause;
        public void Pause()
        {
            if (paused) return;
            paused = true;
            Time.timeScale = 0.0f;
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
            Time.timeScale = 1.0f;

            pauseIndicator.SetActive(false);
        }
        private void PauseInput()
        {
            if (!paused) Pause();
            else Resume();
        }
        #endregion

        // UI //
        #region UI
        [Header("UI")]
        public Transform mainCanvas;
        [SerializeField] private GameObject helpDialogue;

        private void HelpInput() { helpDialogue.SetActive(!helpDialogue.activeInHierarchy); }
        #endregion
    }
}