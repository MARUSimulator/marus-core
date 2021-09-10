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
	public class Path3D : DrawGizmo
	{
		List<Point> _points;
		float lineThickness = 0.05f;
		Color lineColor = Color.yellow;

		public Color pointColor = Color.yellow;

        [Range(0, 1)]
        public float pointSize = 0.1f;
		public float FadeOutRate = 0.01f;

		private Dictionary<Point, Line3D> _startPointLineDict;

		public Path3D()
		{
			_points = new List<Point>();
			_startPointLineDict = new Dictionary<Point, Line3D>();

		}
		public Path3D(float lineThickness, Color lc)
		{
			this.lineThickness = lineThickness;
			_points = new List<Point>();
			_startPointLineDict = new Dictionary<Point, Line3D>();
			lineColor = lc;
		}


		public Path3D(List<Vector3> pointsInWorld)
		{
			_points = new List<Point>();
			_startPointLineDict = new Dictionary<Point, Line3D>();
			foreach (Vector3 pointInWorld in pointsInWorld)
			{
				_points.Add(new Point(pointInWorld));
			}
		}

		public Path3D(List<Vector3> pointsInWorld, float _pointSize, Color _pointColor, float _lineThickness, Color _lineColor)
		{
			_points = new List<Point>();
			_startPointLineDict = new Dictionary<Point, Line3D>();
			pointSize = _pointSize;
			pointColor = _pointColor;
			lineThickness = _lineThickness;
			lineColor = _lineColor;
			foreach (Vector3 pointInWorld in pointsInWorld)
			{
				_points.Add(new Point(pointInWorld, _pointSize, _pointColor));
			}
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
					_startPointLineDict[_points[i]].Destroy();
					_startPointLineDict.Remove(_points[i]);
					_points[i].Destroy();
					_points.Remove(_points[i]);
					i--;
				}
			}
		}

		public void Draw()
		{	
			for (var i = 0; i < _points.Count-1; i++)
			{
				_points[i].Draw();
				if (_points.Count > 2 && !_startPointLineDict.ContainsKey(_points[i]))
				{
					_startPointLineDict.Add(_points[i], new Line3D(_points[i].Position, _points[i+1].Position, lineThickness, lineColor));
				}
			}
			if (_points.Count > 0)
			{
				_points[_points.Count - 1].Draw();
			}
			
			foreach (Line3D line in _startPointLineDict.Values)
			{
				line.Draw();
			}
		}

		public void Destroy()
		{
			foreach (Point p in _points)
			{
				p.Destroy();
			}
			_points.Clear();
			
			foreach (Line3D line in _startPointLineDict.Values)
			{
				line.Destroy();
			}
			_startPointLineDict.Clear();
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
			foreach (Line3D line in _startPointLineDict.Values)
			{
				line.SetColor(color);
			}
		}

		public void SetLineThickness(float lineThickness)
		{
			foreach (Line3D line in _startPointLineDict.Values)
			{
				line.SetThickness(lineThickness);
			}
		}
	}
}
