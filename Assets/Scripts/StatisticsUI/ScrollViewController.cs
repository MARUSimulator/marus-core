using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Labust.StatisticsUI
{
	/// <summary>
	/// This class disables scrollview when dragging.
	/// </summary>
	public class ScrollViewController : MonoBehaviour, IBeginDragHandler, IEndDragHandler {
		public void OnBeginDrag(PointerEventData data)
		{
			GetComponent<ScrollRect>().enabled = false;
		}

		public void OnEndDrag(PointerEventData data)
		{
			GetComponent<ScrollRect>().enabled = true;
		}
	}
}
