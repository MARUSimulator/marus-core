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

namespace Marus.StatisticsUI
{
    /// <summary>
    /// This script controls camera view in Statistics panel.
    /// This enabled dragging and zooming on camera render texture.
    /// </summary>
    public class MousePointToImagePointController : MonoBehaviour
    {
        /// <summary>
        /// Reference to camera
        /// </summary>
        public Camera topDownCamera;

        /// <summary>
        /// Speed of movement when dragging
        /// </summary>
        public float MovementSpeed = 0.1f;

        /// <summary>
        /// Zoom scale for controlling zoom sensitivity
        /// </summary>
        public float ZoomScale = 5f;

        private RectTransform cameraRectTransform;
        private Canvas _statisticsUI;
        private Vector3 delta = Vector3.zero;
        private Vector3 lastPos = Vector3.zero;

        private void Awake() {
            cameraRectTransform = GetComponent<RectTransform>();
            _statisticsUI = transform.parent.parent.parent.GetComponent<Canvas>();
        }

        void Update()
        {
            Vector2 localMousePosition = cameraRectTransform.InverseTransformPoint(Input.mousePosition);
            if (!(cameraRectTransform.rect.Contains(localMousePosition) && _statisticsUI.enabled))
            {
                return;
            }
            var scrollDelta = Input.mouseScrollDelta.y;
            var currentZoomLevel = topDownCamera.orthographicSize;
            currentZoomLevel -= scrollDelta * ZoomScale;

            ZoomScale = currentZoomLevel / 10f;
            MovementSpeed = currentZoomLevel / 500f;
            if (currentZoomLevel > 0.5)
            {
                topDownCamera.orthographicSize = currentZoomLevel;
            }

            if ( Input.GetMouseButtonDown(0) )
            {
                lastPos = Input.mousePosition;
            }
            else if ( Input.GetMouseButton(0) )
            {
                delta = Input.mousePosition - lastPos;
                topDownCamera.transform.position -= new Vector3 (delta.x, 0, delta.y) * MovementSpeed;
                lastPos = Input.mousePosition;
            }
        }

        public void Focus(Vector3 position)
        {
            topDownCamera.transform.position = new Vector3(position.x, topDownCamera.transform.position.y, position.z);
            topDownCamera.orthographicSize = 30f;
        }
        public void Focus(Vector3 position, float orthographicSize)
        {
            topDownCamera.transform.position = new Vector3(position.x, topDownCamera.transform.position.y, position.z);
            topDownCamera.orthographicSize = orthographicSize;
        }
    }
}
