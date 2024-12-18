using Valhalla.TechnicalAnalysis.Interfaces;

namespace Valhalla.TechnicalAnalysis.DrawingObjects
{
    public abstract class Rectangle : IDrawingObject, IDoubleCoordinateDrawingObject
    {
        public abstract DateTime X1 { get; set; }
        public abstract double Y1 { get; set; }
        public abstract DateTime X2 { get; set; }
        public abstract double Y2 { get; set; }
        public abstract string Name { get; set; }
        public abstract bool IsVisible { get; set; }
        public abstract bool IsDraggable { get; set; }
        public abstract void Refresh();
    }
}
