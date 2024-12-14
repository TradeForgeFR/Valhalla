using ScottPlot.Avalonia;

namespace Valhalla.Charting.DrawingObjects
{
    public static class DrawingExtensions
    {
        public static DraggableRectangle StartDrawingDragableRectangle(this AvaPlot plot, double x, double y)
        {
            var rectandle = new DraggableRectangle(plot, x, x, y, y);
            return rectandle;
        }
    }
}
