using UnityEngine;
using Labust.Visualization.Primitives;

namespace Labust.Visualization
{
	/// <summary>
	/// Used for point appearance customization
	/// </summary>
	public class PointVisualController : MonoBehaviour {

		public Point MyPoint;

		public Color PointColor = Color.white;
		[Range(0.1f, 4f)]
		public float PointSize = 0.7f;

		private Color _lastPointColor;
		private float _lastPointSize;

		void Start()
		{
			PointSize = MyPoint.PointSize;
			PointColor = MyPoint.PointColor;
		}

		void Update() {
			if (_lastPointSize != PointSize)
			{
				MyPoint.SetSize(PointSize);
				_lastPointSize = PointSize;
			}

			if (_lastPointColor != PointColor)
			{
				MyPoint.SetColor(PointColor);
				_lastPointColor = PointColor;
			}
		}

		void OnDestroy() {
			MyPoint.Destroy();
		}
	}
}
