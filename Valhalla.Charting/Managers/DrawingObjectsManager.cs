using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ScottPlot;
using ScottPlot.Avalonia;
using Valhalla.Charting.DrawingObjects;
using Valhalla.Charting.Interfaces;

namespace Valhalla.Charting.Managers
{
    public class DrawingObjectsManager : ReactiveObject
    {
        #region private
        private bool _drawingRectangleMode = false, _drawingTrendLineMode = false, _drawingFibonacciMode = false;
        private List<AvaPlot> _plots = new List<AvaPlot>();
        private List<IPlottableContainer> _IPlottableContainer = new List<IPlottableContainer>();
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

            this.StartDrawingFibonacciCommand = new RelayCommand(() =>
            {
                this._drawingFibonacciMode = true;
            });

            this.ClearDrawingObjectsCommand = new RelayCommand(() =>
            {
                foreach(var plot in this._IPlottableContainer)
                {
                    (plot as IPlottableContainer)?.RemovePlottables();
                }
            });
        }
        public void AddPlot(AvaPlot plot)
        {
            plot.PointerPressed += Plot_PointerPressed;
            this._plots.Add(plot);
        }

        public RelayCommand? StartDrawingRectCommand { get; set; }

        public RelayCommand? StartDrawingTrendLineCommand { get; set; }

        public RelayCommand? ClearDrawingObjectsCommand { get; set; }

        public RelayCommand? StartDrawingFibonacciCommand { get; set; }

        private void Plot_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {  
            var plot = sender as AvaPlot;
            var points = e.GetPosition(plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = plot!.Plot.GetCoordinates(mousePixel);

            if (this._drawingRectangleMode)
            {
                var rectangle = plot!.StartDrawingDraggableRectangle(mouseLocation.X, mouseLocation.Y);
                this._IPlottableContainer.Add(rectangle);

                this._drawingRectangleMode = false;
            }
            else if (this._drawingTrendLineMode)
            {
                var trendLine = plot!.StartDrawingDraggableTrendLine(mouseLocation.X, mouseLocation.Y);
                this._IPlottableContainer.Add(trendLine);

                this._drawingTrendLineMode = false;
            }
            else if (this._drawingFibonacciMode)
            {
                var fibo = plot!.StartDrawingDraggableFibonacci(mouseLocation.X, mouseLocation.Y);
                this._IPlottableContainer.Add(fibo);

                this._drawingFibonacciMode = false;
            }
        }
    }
}
