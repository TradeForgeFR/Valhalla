using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Valhalla.ViewModels.Documents;

using Dock.Model.Controls;
using Dock.Model.Core;

namespace Valhalla.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IFactory? _factory;
        private IRootDock? _layout;
        

        public IRootDock? Layout
        {
            get => _layout;
            set => _layout = value;
        }

        public MainWindowViewModel()
        {
            _factory = new ValhallaFactory();

            Layout = _factory?.CreateLayout();
            if (Layout is { })
            {
                _factory?.InitLayout(Layout);
            }
            
        }

        private ChartViewModel OpenChartViewModel(string symbol)
        {
            string title = "Chart - " + symbol;
            
            return new ChartViewModel()
            {
                Title = title,
            };
        }
        
        private void UpdateChartViewModel(ChartViewModel chartViewModel, string symbol)
        {
            chartViewModel.Title = "Chart - " + symbol;
        }

        private void AddchartViewModel(ChartViewModel chartViewModel)
        {
            var charts = _factory?.GetDockable<IDocumentDock>("Charts");
            if (Layout is { } && charts is { })
            {
                _factory?.AddDockable(charts, chartViewModel);
                _factory?.SetActiveDockable(chartViewModel);
                _factory?.SetFocusedDockable(Layout, chartViewModel);
            }
        }

        private ChartViewModel? GetChartViewModel()
        {
            var files = _factory?.GetDockable<IDocumentDock>("Charts");
            return files?.ActiveDockable as ChartViewModel;
        }

        private ChartViewModel GetUntitledChartViewModel()
        {
            return new ChartViewModel()
            {
                Title = "Chart - Untitled"
            };
        }

        public void CloseLayout()
        {
            if (Layout is IDock dock)
            {
                if (dock.Close.CanExecute(null))
                {
                    dock.Close.Execute(null);
                }
            }
        }

        public void ChartNew()
        {
            var untitledchartViewModel = GetUntitledChartViewModel();
            AddchartViewModel(untitledchartViewModel);
        }

        public void ChartExit()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Shutdown();
            }
        }

        private Window? GetWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return desktopLifetime.MainWindow;
            }
            return null;
        }        
        
    }
}
