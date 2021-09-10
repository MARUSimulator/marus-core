using UnityEngine;
using System;

namespace Labust.Visualization.Primitives
{
	/// <summary>
	/// Draw point as sphere
	/// If Transform is given in the constructor, follow the transform origin in every frame
	/// </summary>
	public class Line3D : DrawGizmo
	{
		public Vector3 Start;
		public Vector3 End;
		public float Thickness;
		public Color LineColor;
		private GameObject cylinder;

		public Line3D()
		{
		}

		public Line3D(Vector3 start, Vector3 end)
		{
			Start = start;
			End = end;
		}

		public Line3D(Vector3 start, Vector3 end, float width)
		{
			Start = start;
			End = end;
			Thickness = width;
		}

		public Line3D(Vector3 start, Vector3 end, float width, Color _color)
		{
			Start = start;
			End = end;
			Thickness = width;
			LineColor = _color;
		}

		public void Draw()
		{
			if (cylinder != null)
			{
				return;
			}
			
			cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			var offset = End - Start;
			var scale = new Vector3(Thickness, offset.magnitude / 2.0f, Thickness);
			var position = Start + (offset / 2.0f);

			cylinder.transform.position = position;
			cylinder.transform.rotation = Quaternion.identity;
			cylinder.transform.up = offset;
			cylinder.transform.localScale = scale;
			cylinder.GetComponent<Renderer>().material.color = LineColor;
			cylinder.hideFlags = HideFlags.HideInHierarchy;
			UnityEngine.Object.Destroy(cylinder.GetComponent<CapsuleCollider>());
			cylinder.layer = 6;
		}

		public void Destroy()
		{
			if (cylinder == null)
			{
				return;
			}
			UnityEngine.Object.Destroy(cylinder);
		}

		public void SetThickness(float width)
		{
			if (cylinder == null)
			{
				return;
			}
			var oldScale = cylinder.transform.localScale;
			cylinder.transform.localScale = new Vector3(width, oldScale.y, width);
		}

		public void SetColor(Color color)
		{
			if (cylinder == null)
			{
				return;
			}
			cylinder.GetComponent<Renderer>().material.color = color;
		}
	}
}
