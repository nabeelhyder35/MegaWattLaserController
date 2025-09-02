using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using LaserControllerApp.ViewModels;
using LaserControllerApp.Views;

namespace LaserControllerApp
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();

            // Resolve ViewModel from DI container
            try
            {
                ViewModel = App.Services.GetRequiredService<MainViewModel>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to resolve MainViewModel: {ex.Message}");
                throw;
            }

            // Bind the RootGrid to the ViewModel for x:Bind
            RootGrid.DataContext = ViewModel;

            // Load default page
            try
            {
                ContentFrame.Content = App.Services.GetRequiredService<EnergyMonitorPage>();
                NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load EnergyMonitorPage: {ex.Message}");
            }
        }

        private void NavigationViewControl_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem nvi)
            {
                string tag = nvi.Tag as string ?? "";
                Page page = tag switch
                {
                    "EnergyMonitorPage" => App.Services.GetRequiredService<EnergyMonitorPage>(),
                    "PulseSettingsPage" => App.Services.GetRequiredService<PulseSettingsPage>(),
                    "VoltagePage" => App.Services.GetRequiredService<VoltagePage>(),
                    "ShutterPage" => App.Services.GetRequiredService<ShutterPage>(),
                    "WaveformPage" => App.Services.GetRequiredService<WaveformPage>(),
                    "InterlockStatusPage" => App.Services.GetRequiredService<InterlockStatusPage>(),
                    "CustomPage" => App.Services.GetRequiredService<CustomPage>(),
                    _ => App.Services.GetRequiredService<EnergyMonitorPage>()
                };

                ContentFrame.Content = page;
            }
        }
    }
}