using UnityEngine;

namespace Labust.StatisticsUI
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

        void Start()
        {
            _cameraObject = transform.Find("Panel/TopDownCamera").gameObject;
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
            _cameraObject.SetActive(false);
            _controller = GetComponentInChildren<PathRecordingsVisualization>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (_canvas.enabled)
                {
                    _cameraObject.SetActive(false);
                    _canvas.enabled = false;
                }
                else
                {
                    _cameraObject.SetActive(true);
                    _canvas.enabled = true;
                    _controller.RefreshPaths();
                }
            }
        }
    }
}
