using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LaserControllerApp.Views
{
    public sealed partial class HomeScreen : UserControl
    {
        public MainViewModel ViewModel { get; }

        public HomeScreen()
        {
            this.InitializeComponent();
            ViewModel = (MainViewModel)App.Services.GetService(typeof(MainViewModel));

            // Set the DataContext for this UserControl
            this.DataContext = ViewModel;
        }
    }
}