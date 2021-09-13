using Labust.Visualization.Primitives;
using UnityEngine;

namespace Labust.Visualization
{
    public class PathVisualization : MonoBehaviour
    {	
		public Color PointColor = Color.blue;
		[Range(0, 1)]
        public float PointSize = 0.1f;

		public Color LineColor = Color.yellow;
		[Range(0, 0.5f)]
        public float LineThickness = 0.05f;

		public float RefreshRateHz = 2;
		public bool TrackIn3D = false;
		public bool EnableFadeout = true;
		public float FadeOutAfterSecs = 3;
		public float MinimumDistanceDelta = 0.1f;

		private float elapsedTime = 0;
		private Path3D path;
		private Vector3 lastPosition;
		private float lastPointSize;
		private Color lastPointColor;

		private float lastLineThickness;
		private Color lastLineColor;

		void Awake()
		{
			lastPosition = transform.position;
			path = new Path3D(LineThickness, LineColor);
			lastPointSize = PointSize;
			lastPointColor = PointColor;
			lastLineThickness = LineThickness;
			lastLineColor = LineColor;
		}

		void Update()
		{
			if (EnableFadeout)
			{
				path.RefreshAndFade(FadeOutAfterSecs);
			}
			
			elapsedTime += Time.deltaTime;
			if (elapsedTime > 1 / RefreshRateHz)
			{
				elapsedTime = 0;
				if (Vector3.Distance(lastPosition, transform.position) < MinimumDistanceDelta)
				{
					return;
				}

				var pos = transform.position;
				if (!TrackIn3D)
				{
					pos.y = 0;
				}
				path.AddPointToPath(pos, PointSize, PointColor);
				
				lastPosition = transform.position;
			}

			if (lastPointSize != PointSize)
			{
				path.SetPointSize(PointSize);
				lastPointSize = PointSize;
			}

			if (lastPointColor != PointColor)
			{
				path.SetPointColor(PointColor);
				lastPointColor = PointColor;
			}

			if (lastLineThickness != LineThickness)
			{
				path.SetLineThickness(LineThickness);
				lastLineThickness = LineThickness;
			}

			if (lastLineColor != LineColor)
			{
				path.SetLineColor(LineColor);
				lastLineColor = LineColor;
			}
			
			path.Draw();
		}
	}
}