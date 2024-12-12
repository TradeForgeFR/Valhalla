using ScottPlot.Avalonia;

namespace Vahallah.Charting.ScottPlotExtensions
{
    public static class DrawingExtensions
    {
        public static DragableRectangle StartDrawingDragableRectangle(this AvaPlot plot, double x, double y)
        {
            var rectandle = new DragableRectangle(plot, x, x, y, y);
            return rectandle;
        }
    }
}
