using System;
using System.Collections.Generic;
using Labust.Visualization.Primitives;
using UnityEngine;


namespace Labust.Visualization
{
    public class PathVisualization : MonoBehaviour
    {	
		public Color pointColor = Color.blue;
		[Range(0, 1)]
        public float pointSize = 0.1f;

		public Color lineColor = Color.yellow;
		[Range(0, 0.5f)]
        public float lineThickness = 0.05f;

		public float RefreshRateHz = 2;
		public bool TrackIn3D = false;
		public bool EnableFadeout = true;
		public float FadeOutAfterSecs = 3;

		private float elapsedTime = 0;
		private LinearPath path;
		public float MinimumTranslationDistance = 0.1f;
		private Vector3 lastPosition;
		void Awake()
		{
			lastPosition = transform.position;
			path = new LinearPath(lineThickness, lineColor);
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
				if (Vector3.Distance(lastPosition, transform.position) < MinimumTranslationDistance)
				{
					return;
				}

				var pos = transform.position;
				if (!TrackIn3D)
				{
					pos.y = 0;
				}
				path.AddPointToPath(pos, pointSize, pointColor);
				
				lastPosition = transform.position;
			}
			path.Draw();
		}
	}
}