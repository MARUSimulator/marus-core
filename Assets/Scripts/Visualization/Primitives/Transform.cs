using Labust.Utils;
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
        public float LengthScale = 0.5f;
        public float LineThickness = 0.03f;

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
            (var x, var y, var z) = CalculateAxisPoints();

            _xLine = new Line(origin, x, LineThickness);
            _xLine.LineColor = Color.red;
            _xLine.SetParent(transform.gameObject);

            _yLine = new Line(origin, y, LineThickness);
            _yLine.LineColor = Color.green;
            _yLine.SetParent(transform.gameObject);

            _zLine = new Line(origin, z, LineThickness);
            _zLine.LineColor = Color.blue;
            _zLine.SetParent(transform.gameObject);
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
            (var x, var y, var z) = CalculateAxisPoints();

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

        private (Vector3, Vector3, Vector3) CalculateAxisPoints()
        {
            var totalScale = Helpers.GetObjectScale(MyTransform);
            var x_scale = 1 / totalScale.x;
            var y_scale = 1 / totalScale.y;
            var z_scale = 1 / totalScale.z;

            var origin = MyTransform.TransformPoint(Vector3.zero);
            var x = MyTransform.TransformPoint(Vector3.right * LengthScale * x_scale);
            var y = MyTransform.TransformPoint(Vector3.up * LengthScale * y_scale);
            var z = MyTransform.TransformPoint(Vector3.forward * LengthScale * z_scale);

            return (x, y, z);
        }
    }
}
