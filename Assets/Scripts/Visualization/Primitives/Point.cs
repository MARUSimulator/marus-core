using UnityEngine;
using System;

namespace Labust.Visualization.Primitives
{
	/// <summary>
	/// Draw point as sphere
	/// If Transform is given in the constructor, follow the transform origin in every frame
	/// </summary>
	public class Point : DrawGizmo
	{
		public Vector3 Position;
		
		UnityEngine.Transform _pointTransform;
		public DateTime Timestamp;
		public float PointSize = 0.1f;
		public Color PointColor = Color.blue;
		private GameObject sphere = null;

		public Point(Vector3 pointInWorld)
		{
			Position = pointInWorld;
			Timestamp = DateTime.UtcNow;
		}

		public Point(Vector3 pointInWorld, float pointSize)
		{
			Position = pointInWorld;
			Timestamp = DateTime.UtcNow;
			PointSize = pointSize;
		}

		public Point(Vector3 pointInWorld, float pointSize, Color pointColor)
		{
			Position = pointInWorld;
			Timestamp = DateTime.UtcNow;
			PointSize = pointSize;
			PointColor = pointColor;
		}

		public Point(UnityEngine.Transform pointInWorld)
		{
			_pointTransform = pointInWorld;
			Timestamp = DateTime.UtcNow;
		}

		public Point(UnityEngine.Transform pointInWorld, float pointSize)
		{
			_pointTransform = pointInWorld;
			Timestamp = DateTime.UtcNow;
			PointSize = pointSize;
		}

		public Point(UnityEngine.Transform pointInWorld, float pointSize, Color pointColor)
		{
			_pointTransform = pointInWorld;
			Timestamp = DateTime.UtcNow;
			PointSize = pointSize;
			PointColor = pointColor;
		}

		public void Draw()
		{
			if (sphere == null)
			{
				sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				sphere.hideFlags = HideFlags.HideInHierarchy;
				sphere.GetComponent<SphereCollider>().enabled = false;
				sphere.transform.localScale = new Vector3(PointSize, PointSize, PointSize);
				sphere.transform.position = Position;
				sphere.GetComponent<Renderer>().material.color = PointColor;
				sphere.layer = 6;
			}
			if (sphere != null && _pointTransform != null)
			{
				sphere.transform.position = _pointTransform.position;
			}
		}

		public void Destroy()
		{
			if (sphere == null)
			{
				return;
			}
			UnityEngine.Object.Destroy(sphere.gameObject);
			sphere = null;
		}

		public void SetSize(float size)
		{
			if (sphere == null)
			{
				return;
			}
			sphere.transform.localScale = new Vector3(size, size, size);
		}

		public void SetColor(Color color)
		{
			if (sphere == null)
			{
				return;
			}
			sphere.GetComponent<Renderer>().material.color = color;
		}
	}
}
