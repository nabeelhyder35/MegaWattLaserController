using LaserControllerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace LaserControllerApp.Views
{
    public sealed partial class LaserControlDashboard : Page
    {
        public LaserControlDashboardViewModel ViewModel { get; }

        public LaserControlDashboard()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<LaserControlDashboardViewModel>();
            DataContext = ViewModel;
        }

        private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Optional: start automatic refresh or timers
        }

        private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Cleanup if needed
        }

        private void LogScrollViewer_ViewChanged(object sender, Microsoft.UI.Xaml.Controls.ScrollViewerViewChangedEventArgs e)
        {
            // Optional: autoscroll to bottom
            LogScrollViewer.ChangeView(null, LogScrollViewer.ExtentHeight, null, true);
        }
    }
}
