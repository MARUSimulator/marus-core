using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Labust.Logger;
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
    public class PauseMenu : MonoBehaviour
    {
        public bool SaveOnExit = true;

        public GameObject PauseMenuUi;
        bool _isRunning;
        float timeScaleBeforePause;
        public bool LockCursor;

        public string scenarioName;
        public string description;

        public string SavesPath => Path.Combine(Application.dataPath, "Saves");

        void Awake()
        {
            _isRunning = true;
            PauseMenuUi.SetActive(false);
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
            if (SaveOnExit)
            {
                DataLoggerUtilities.SaveAllLogs();
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Resume();
        }

        /// <summary>
        /// Called from UI button. Not to be called directly
        /// </summary>
        public void Exit()
        {
            if (Application.isEditor)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            Application.Quit();
        }

        public void Save()
        {
            DataLoggerUtilities.SaveAllLogs(scenarioName, description);
        }

        void OnApplicationQuit()
        {
            if (SaveOnExit)
            {
                Save();
            }
        }
    }
}