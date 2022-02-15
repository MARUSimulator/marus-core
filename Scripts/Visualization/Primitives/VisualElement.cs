using System;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Defines required properties of any visual element object
    /// such as Line, Point, Path...
    /// </summary>
    public abstract class VisualElement
    {
        public float Lifetime;
        public DateTime Timestamp;
        public string? Id = null;

        public abstract void Draw();
        public abstract void Destroy();
    }
}
