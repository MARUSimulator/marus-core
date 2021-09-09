using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;

namespace Labust.Visualization.Primitives
{
	/// <summary>
	/// Draw path as by-part linear.
	/// 
	/// If Transform[] is given in the constructor, follow the transform origins of every point in every frame
	/// </summary>
	public class LinearPath : DrawGizmo
	{
		List<Point> _points;
		float lineThickness = 0.05f;
		Color lineColor = Color.yellow;

		public Color pointColor = Color.yellow;

        [Range(0, 1)]
        public float pointSize = 0.1f;
		public float FadeOutRate = 0.01f;

		private GameObject line;
		private LineRenderer lr;

		public LinearPath()
		{
			_points = new List<Point>();
			InitLineRenderer();
		}
		public LinearPath(float lineThickness, Color lc)
		{
			this.lineThickness = lineThickness;
			_points = new List<Point>();
			lineColor = lc;
			InitLineRenderer();
		}


		public LinearPath(List<Vector3> pointsInWorld)
		{
			_points = new List<Point>();
			foreach (Vector3 pointInWorld in pointsInWorld)
			{
				_points.Add(new Point(pointInWorld));
			}
			InitLineRenderer();
		}

		public LinearPath(List<Vector3> pointsInWorld, float _pointSize, Color _pointColor, float _lineThickness, Color _lineColor)
		{
			_points = new List<Point>();
			pointSize = _pointSize;
			pointColor = _pointColor;
			lineThickness = _lineThickness;
			lineColor = _lineColor;
			foreach (Vector3 pointInWorld in pointsInWorld)
			{
				_points.Add(new Point(pointInWorld, _pointSize, _pointColor));
			}
			InitLineRenderer();
		}


		private void InitLineRenderer()
		{
			line = new GameObject();
			line.hideFlags = HideFlags.HideInHierarchy;
			line.AddComponent<LineRenderer>();
			lr = line.GetComponent<LineRenderer>();
			lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
			
			float alpha = 1.0f;
			Gradient gradient = new Gradient();
			gradient.SetKeys(
				new GradientColorKey[] { new GradientColorKey(lineColor, 0.0f), new GradientColorKey(lineColor, 1.0f) },
				new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
			);
			lr.colorGradient = gradient;
	
			// set width of the renderer
			lr.startWidth = lineThickness;
			lr.endWidth = lineThickness;
			// set layer to visualisation initially
			line.layer = 6;
		}

		public void AddPointToPath(Vector3 point)
		{
			_points.Add(new Point(point));
		}

		public void AddPointToPath(Vector3 point, float pointSize, Color pointColor)
		{
			_points.Add(new Point(point, pointSize, pointColor));
		}
		
		public void RefreshAndFade(float limit)
		{
			
			for (int i = 0; i < _points.Count; i++)
			{
				if (DateTime.UtcNow.Subtract(_points[i].Timestamp).TotalSeconds > limit)
				{
					_points[i].Destroy();
					_points.Remove(_points[i]);
					i--;
				}
			}
		}

		public void Draw()
		{	
			for (var i = 0; i < _points.Count; i++)
			{
				lr.positionCount = i + 1;
				_points[i].Draw();
				
				lr.SetPosition(i, _points[i].Position);
			}
		}

		public void Destroy()
		{
			foreach (Point p in _points)
			{
				p.Destroy();
			}
			_points.Clear();
			
			if (lr != null)
			{
				UnityEngine.Object.Destroy(lr.gameObject);
			}
		}

		public void SetPointSize(float size)
		{
			foreach (Point p in _points)
			{
				p.SetSize(size);
			}
		}

		public void SetPointColor(Color color)
		{
			foreach (Point p in _points)
			{
				p.SetColor(color);
			}
		}

		public void SetLineColor(Color color)
		{
			float alpha = 1.0f;
			Gradient gradient = new Gradient();
			gradient.SetKeys(
				new GradientColorKey[] { new GradientColorKey(lineColor, 0.0f), new GradientColorKey(lineColor, 1.0f) },
				new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
			);
			lr.colorGradient = gradient;
		}

		public void SetLineThickness(float lineThickness)
		{
			lr.startWidth = lineThickness;
			lr.endWidth = lineThickness;
		}
	}
}
