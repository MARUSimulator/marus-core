using UnityEngine;
using UnityEngine.EventSystems;
 
namespace Labust.StatisticsUI
{
	public class MousePointToImagePointController : MonoBehaviour
	{
		public Camera topDownCamera;
		public float MovementSpeed = 0.1f;
		public float ZoomScale = 5f;
		private RectTransform cameraRectTransform;
		private Canvas _statisticsUI;
		private Vector3 delta = Vector3.zero;
		private Vector3 lastPos = Vector3.zero;

		private void Awake() {
			cameraRectTransform = GetComponent<RectTransform>();
			_statisticsUI = GameObject.Find("StatisticsCanvas").GetComponent<Canvas>();
		}

		void Update()
		{
			Vector2 localMousePosition = cameraRectTransform.InverseTransformPoint(Input.mousePosition);
			if (!(cameraRectTransform.rect.Contains(localMousePosition) && _statisticsUI.enabled))
			{
				return;
			}

			topDownCamera.transform.position -= new Vector3(0, Input.mouseScrollDelta.y * ZoomScale, 0) * 10;

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
	}
}
