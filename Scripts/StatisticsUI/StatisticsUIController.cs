// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
#if CREST_OCEAN
using Crest;
#endif

namespace Marus.StatisticsUI
{
    /// <summary>
    /// Enable statistics canvas on P button press
    /// Also refresh path recordings menu
    /// </summary>
    public class StatisticsUIController : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _cameraObject;
        private PathRecordingsVisualization _controller;

        private GameObject _ocean;
        private Camera _camera;

        void Start()
        {
            _cameraObject = transform.Find("Panel/TopDownCamera").gameObject;
            _camera = _cameraObject.transform.Find("TopDownView").gameObject.GetComponent<Camera>();
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
            _cameraObject.SetActive(false);
            _controller = GetComponentInChildren<PathRecordingsVisualization>();
            _ocean = GameObject.Find("Environment/Ocean");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (_canvas.enabled)
                {
                    _cameraObject.SetActive(false);
                    _canvas.enabled = false;
                    Cursor.lockState = CursorLockMode.Locked;

#if CREST_OCEAN
                    _ocean.GetComponent<OceanRenderer>().ViewCamera = null;
#endif
                }
                else
                {
                    _cameraObject.SetActive(true);
                    _canvas.enabled = true;
                    _controller.RefreshPaths();
                    Cursor.lockState = CursorLockMode.Confined;
#if CREST_OCEAN
                    _ocean.GetComponent<OceanRenderer>().ViewCamera = _camera;
#endif
                }
            }
        }
    }
}
