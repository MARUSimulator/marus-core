using UnityEngine;

namespace Labust.StatisticsUI
{
	public class StatisticsUIController : MonoBehaviour
	{
		private Canvas _canvas;
		
		void Start()
		{
			_canvas = GetComponent<Canvas>();
			_canvas.enabled = false;
		}

		void Update()
		{
			
			if (Input.GetKeyDown(KeyCode.P))
			{
				if (_canvas.enabled)
				{
					_canvas.enabled = false;
				}
				else
				{
					_canvas.enabled = true;
				}
			}
		}

	}
}
