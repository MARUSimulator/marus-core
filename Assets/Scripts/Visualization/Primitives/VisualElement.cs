namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Defines required properties of any visual element object
    /// such as Line, Point, Path...
    /// </summary>
    public interface VisualElement
    {
        void Draw();
        void Destroy();
    }
}
