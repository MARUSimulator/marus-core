using UnityEngine;
using Labust.Visualization.Primitives;

namespace Labust.Visualization
{
	/// <summary>
	/// Used for line appearance customization
	/// </summary>
	public class LineVisualController : MonoBehaviour {

		public Line MyLine;

		public Color LineColor = Color.red;

		[Range(0f, 4f)]
		public float LineThickness = 0.8f;

		private Color _lastLineColor;
		private float _lastLineThickness;

		private void Start() {
			LineThickness = MyLine.Thickness;
			LineColor = MyLine.LineColor;
		}
		void Update() {
			if (_lastLineThickness != LineThickness)
			{
				MyLine.SetThickness(LineThickness);
				_lastLineThickness = LineThickness;
			}

			if (_lastLineColor != LineColor)
			{
				MyLine.SetColor(LineColor);
				_lastLineColor = LineColor;
			}
		}

		void OnDestroy() {
			MyLine.Destroy();
		}
	}
}
