using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ScottPlot;
using ScottPlot.Avalonia;
using Valhalla.Charting.DrawingObjects;

namespace Valhalla.Charting.Managers
{
    public class DrawingObjectsManager : ReactiveObject
    {
        #region private
        private bool _drawingRectangleMode = false, _drawingTrendLineMode = false;
        private List<AvaPlot> _plots = new List<AvaPlot>();
        #endregion

        public DrawingObjectsManager() 
        {
            this.StartDrawingRectCommand = new RelayCommand(() =>
            {
                this._drawingRectangleMode = true;
            });

            this.StartDrawingTrendLineCommand = new RelayCommand(() =>
            {
                this._drawingTrendLineMode = true;
            });
        }
        public void AddPlot(AvaPlot plot)
        {
            plot.PointerPressed += Plot_PointerPressed;
        }

        public RelayCommand? StartDrawingRectCommand { get; set; }

        public RelayCommand? StartDrawingTrendLineCommand { get; set; }

        private void Plot_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (!this._drawingRectangleMode && !this._drawingTrendLineMode)
                return;

            var plot = sender as AvaPlot;
            var points = e.GetPosition(plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = plot!.Plot.GetCoordinates(mousePixel);

            if (this._drawingRectangleMode)
            {
                plot!.StartDrawingDraggableRectangle(mouseLocation.X, mouseLocation.Y);

                this._drawingRectangleMode = false;
            }
            else if (this._drawingTrendLineMode)
            {
                plot!.StartDrawingDraggableTrendLine(mouseLocation.X, mouseLocation.Y);

                this._drawingTrendLineMode = false;
            }
        }
    }
}
