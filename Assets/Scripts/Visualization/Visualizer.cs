using System.Collections.Generic;
using Labust.Visualization.Primitives;
using Labust.Utils;
using UnityEngine;
using System.IO;

namespace Labust.Visualization
{
	/// <summary>
	/// This is a singleton class used for drawing points and paths for visualizatoin purposes.
	/// You can add points and paths with a string key tag for easy selective destroying of visualization objects.
	/// </summary>
	public class Visualizer : GenericSingleton<Visualizer>
	{
		
		public Color pointColor = Color.yellow;

		[Range(0, 1)]
		public float pointSize = 0.1f;
		
		[Range(0, 1)]
		public float lineThickness = 0.05f;
		public Color lineColor = Color.red;
		private Dictionary<string, List<DrawGizmo>> _gizmos = new Dictionary<string, List<DrawGizmo>>();
		
		void Start()
		{
			/*
			var boat = GameObject.Find("Target");
			AddTransform(boat.transform, "test transform");

			AddPoint(new Vector3(0, 2, 10), "test point", 0.5f);

			List<Vector3> path = new List<Vector3> {new Vector3(0, 1, 0), new Vector3(14, 1, 0), new Vector3(25, 1, 0)};
			//AddPath(path, "test path");
			AddPath3D(path, "test path");
			*/
		}

		public void AddPoint(Vector3 pointInWorld, string key)
		{
			if (!_gizmos.ContainsKey(key))
			{
				_gizmos[key] = new List<DrawGizmo>();
			}
			_gizmos[key].Add((DrawGizmo) new Point(pointInWorld));
		}

		public void AddPoint(Vector3 pointInWorld, string key, float pointSize)
		{
			if (!_gizmos.ContainsKey(key))
			{
				_gizmos[key] = new List<DrawGizmo>();
			}
			_gizmos[key].Add((DrawGizmo) new Point(pointInWorld, pointSize));
		}

		public void AddPoint(Vector3 pointInWorld, string key, float pointSize, Color pointColor)
		{
			if (!_gizmos.ContainsKey(key))
			{
				_gizmos[key] = new List<DrawGizmo>();
			}
			_gizmos[key].Add((DrawGizmo) new Point(pointInWorld, pointSize, pointColor));
		}

		public void AddPath(List<Vector3> pointsInWorld, string key)
		{
			if (!_gizmos.ContainsKey(key))
			{
				_gizmos[key] = new List<DrawGizmo>();
			}
			_gizmos[key].Add((DrawGizmo) new LinearPath(pointsInWorld, pointSize, pointColor, lineThickness, lineColor));
		}

		public void AddPath(List<Vector3> pointsInWorld, string key, Color _pointColor)
		{
			if (!_gizmos.ContainsKey(key))
			{
				_gizmos[key] = new List<DrawGizmo>();
			}
			_gizmos[key].Add((DrawGizmo) new LinearPath(pointsInWorld, pointSize, _pointColor, lineThickness, lineColor));
		}
		
		public void AddPath(LinearPath path, string key)
		{
			if (!_gizmos.ContainsKey(key))
			{
				_gizmos[key] = new List<DrawGizmo>();
			}
			_gizmos[key].Add((DrawGizmo) path);
		}

		public void AddPath3D(List<Vector3> pointsInWorld, string key)
		{
			if (!_gizmos.ContainsKey(key))
			{
				_gizmos[key] = new List<DrawGizmo>();
			}
			_gizmos[key].Add((DrawGizmo) new Path3D(pointsInWorld, pointSize, pointColor, lineThickness, lineColor));
		}

		public void AddPath3D(List<Vector3> pointsInWorld, string key, Color _pointColor)
		{
			if (!_gizmos.ContainsKey(key))
			{
				_gizmos[key] = new List<DrawGizmo>();
			}
			_gizmos[key].Add((DrawGizmo) new Path3D(pointsInWorld, pointSize, _pointColor, lineThickness, lineColor));
		}
		
		public void AddPath3D(Path3D path, string key)
		{
			if (!_gizmos.ContainsKey(key))
			{
				_gizmos[key] = new List<DrawGizmo>();
			}
			_gizmos[key].Add((DrawGizmo) path);
		}

		public void AddTransform(UnityEngine.Transform transform, string key)
		{
			if (!_gizmos.ContainsKey(key))
			{
				_gizmos[key] = new List<DrawGizmo>();
			}
			_gizmos[key].Add((DrawGizmo) new Labust.Visualization.Primitives.Transform(transform));
		}

		public void FlushKey(string key)
		{
			foreach (DrawGizmo gizmo in _gizmos[key])
			{
				gizmo.Destroy();
			}
			_gizmos.Remove(key);
		}
		void Update()
		{
			foreach (var list in _gizmos.Values)
			{
				foreach (var gizmo in list)
				{
					gizmo.Draw();
				}
			}
		}
	}
}
