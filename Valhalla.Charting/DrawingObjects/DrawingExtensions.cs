using ScottPlot.Avalonia;

namespace Valhalla.Charting.DrawingObjects
{
    public static class DrawingExtensions
    {
        public static DraggableRectangle StartDrawingDraggableRectangle(this AvaPlot plot, double x, double y)
        {
           plot.UserInputProcessor.Disable();
            var rectandle = new DraggableRectangle(plot, x, x, y, y);
            return rectandle;
        }

        public static DraggableTrendLine StartDrawingDraggableTrendLine(this AvaPlot plot, double x, double y)
        {
            plot.UserInputProcessor.Disable();
            var line = new DraggableTrendLine(plot, x, x, y, y);
            return line;
        }

        public static DraggableFibonacci StartDrawingDraggableFibonacci(this AvaPlot plot, double x, double y)
        {
            plot.UserInputProcessor.Disable();
            var fiblo = new DraggableFibonacci(plot, x, x, y, y);
            return fiblo;
        }
    }
}
