
using UnityEditor;
using UnityEngine;

namespace Labust.Visualization.Primitives
{
	public class Transform : DrawGizmo
	{
		UnityEngine.Transform _transform;
		private Line _xLine;
		private Line _yLine;
		private Line _zLine;
		public Transform(UnityEngine.Transform transform)
		{
			_transform = transform;
			var origin = _transform.TransformPoint(Vector3.zero);
			var x = _transform.TransformPoint(Vector3.right);
			var y = _transform.TransformPoint(Vector3.up);
			var z = _transform.TransformPoint(Vector3.forward);

			_xLine = new Line(origin, x);
			_xLine.LineColor = Color.red;

			_yLine = new Line(origin, y);
			_yLine.LineColor = Color.green;

			_zLine = new Line(origin, z);
			_zLine.LineColor = Color.blue;
		}

		public void Draw()
		{
			var origin = _transform.TransformPoint(Vector3.zero);
			var x = _transform.TransformPoint(Vector3.right);
			var y = _transform.TransformPoint(Vector3.up);
			var z = _transform.TransformPoint(Vector3.forward);
			
			_xLine.StartPoint = origin;
			_xLine.EndPoint = x;

			_yLine.StartPoint = origin;
			_yLine.EndPoint = y;

			_zLine.StartPoint = origin;
			_zLine.EndPoint = z;

			_xLine.Draw();
			_yLine.Draw();
			_zLine.Draw();
		}

		public void Destroy()
		{
			_xLine.Destroy();
			_yLine.Destroy();
			_zLine.Destroy();
		}
	}
}
