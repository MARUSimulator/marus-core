using UnityEditor;
using UnityEngine;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Implements visual representation of GameObject's transform.
    /// Lines are drawn from center of the object in each of the axes' direction.
    /// </summary>
    public class Transform : VisualElement
    {
        UnityEngine.Transform _transform;

        private Line _xLine;
        private Line _yLine;
        private Line _zLine;

        /// <summary>
        /// Constructor which initializes visual element with gameobject's transform reference.
        /// </summary>
        /// <param name="transform"></param>
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

        /// <summary>
        /// Draws transform
        /// </summary>
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

        /// <summary>
        /// Destroys and removes transform
        /// </summary>
        public void Destroy()
        {
            _xLine.Destroy();
            _yLine.Destroy();
            _zLine.Destroy();
        }
    }
}
