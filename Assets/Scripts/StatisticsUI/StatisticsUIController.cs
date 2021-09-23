using UnityEngine;
#if CREST_AVAILABLE
using Crest;
#endif

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

                    _ocean.GetComponent<OceanRenderer>().ViewCamera = null;
                }
                else
                {
                    _cameraObject.SetActive(true);
                    _canvas.enabled = true;
                    _controller.RefreshPaths();
                    Cursor.lockState = CursorLockMode.Confined;
                    _ocean.GetComponent<OceanRenderer>().ViewCamera = _camera;
                }
            }
        }
    }
}
