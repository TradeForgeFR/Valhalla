using Avalonia.Controls;
using Vahallah.ViewModels;

namespace Vahallah.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += this.MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await (this.DataContext as MainWindowViewModel)!.FillTheChart();
        }
    }
}