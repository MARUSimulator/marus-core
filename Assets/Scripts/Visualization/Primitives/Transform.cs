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
        public UnityEngine.Transform MyTransform;

        private Line _xLine;
        private Line _yLine;
        private Line _zLine;

        private bool destroyed = false;

        /// <summary>
        /// Constructor which initializes visual element with gameobject's transform reference.
        /// </summary>
        /// <param name="transform"></param>
        public Transform(UnityEngine.Transform transform)
        {
            MyTransform = transform;
            var origin = MyTransform.TransformPoint(Vector3.zero);
            var x = MyTransform.TransformPoint(Vector3.right);
            var y = MyTransform.TransformPoint(Vector3.up);
            var z = MyTransform.TransformPoint(Vector3.forward);

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
            if (destroyed)
            {
                return;
            }
            var origin = MyTransform.TransformPoint(Vector3.zero);
            var x = MyTransform.TransformPoint(Vector3.right);
            var y = MyTransform.TransformPoint(Vector3.up);
            var z = MyTransform.TransformPoint(Vector3.forward);

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
            destroyed = true;
            _xLine.Destroy();
            _yLine.Destroy();
            _zLine.Destroy();
        }

        public void SetParent(GameObject parent)
        {
            _xLine.SetParent(parent);
            _yLine.SetParent(parent);
            _zLine.SetParent(parent);
        }
    }
}
