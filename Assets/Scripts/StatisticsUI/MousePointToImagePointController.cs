using UnityEngine;
using UnityEngine.EventSystems;

namespace Labust.StatisticsUI
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
            topDownCamera.transform.position = new Vector3(position.x, 0, position.z);
            topDownCamera.orthographicSize = 30f;
        }
    }
}
