using UnityEngine;
using Labust.Visualization.Primitives;

namespace Labust.Visualization
{
	/// <summary>
	/// Used for path appearance customization
	/// </summary>
	public class PathVisualController : MonoBehaviour {

		public Path MyPath;

		public Color PointColor = Color.white;
		[Range(0.1f, 4f)]
		public float PointSize = 0.8f;
		public Color LineColor = Color.red;
		[Range(0f, 2f)]
		public float LineThickness = 0.55f;

		private Color _lastPointColor;
		private Color _lastLineColor;
		private float _lastPointSize;
		private float _lastLineThickness;

		private void Start() {
			PointColor = MyPath.PointColor;
			PointSize = MyPath.PointSize;
			LineColor = MyPath.LineColor;
			LineThickness = MyPath.LineThickness;
		}

		void Update() {
			if (_lastPointSize != PointSize)
			{
				MyPath.SetPointSize(PointSize);
				_lastPointSize = PointSize;
			}

			if (_lastPointColor != PointColor)
			{
				MyPath.SetPointColor(PointColor);
				_lastPointColor = PointColor;
			}

			if (_lastLineThickness != LineThickness)
			{
				MyPath.SetLineThickness(LineThickness);
				_lastLineThickness = LineThickness;
			}

			if (_lastLineColor != LineColor)
			{
				MyPath.SetLineColor(LineColor);
				_lastLineColor = LineColor;
			}
		}

		void OnDestroy() {
			MyPath.Destroy();
		}
	}
}
