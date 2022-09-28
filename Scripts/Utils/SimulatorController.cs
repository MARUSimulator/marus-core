using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Labust.Core
{

    /// <summary>
    /// Class used to control simulation flow
    ///
    /// Play, pause, restart, quit, OnSave, etc.
    /// </summary>
    public class SimulatorController : MonoBehaviour
    {
        public GameObject PauseMenuUi;
        bool _isRunning;
        float timeScaleBeforePause;
        public bool LockCursor;

        void Awake()
        {
            _isRunning = true;
            Pause();
            PauseMenuUi.SetActive(true);
            if (LockCursor)
                Cursor.lockState = CursorLockMode.Locked;
        }

        void LateUpdate()
        {
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_isRunning)
                {
                    Pause();
                }
                else
                {
                    Resume();
                }
            }
        }

        /// <summary>
        /// Called from UI button. Not to be called directly
        /// </summary>
        public void Pause()
        {
            Cursor.lockState = CursorLockMode.Confined;
            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0;
            PauseMenuUi.SetActive(true);
            _isRunning = false;
        }

        /// <summary>
        /// Called from UI button. Not to be called directly
        /// </summary>
        public void Resume()
        {
            if (LockCursor)
                Cursor.lockState = CursorLockMode.Locked;
            PauseMenuUi.SetActive(false);
            Time.timeScale = timeScaleBeforePause;
            _isRunning = true;
        }

        /// <summary>
        /// Called from UI button. Not to be called directly
        /// </summary>
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Resume();
        }

        /// <summary>
        /// Called from UI button. Not to be called directly
        /// </summary>
        public void Exit()
        {
            #if UNITY_EDITOR
            if (Application.isEditor)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            #endif
            Application.Quit();
        }
    }
}