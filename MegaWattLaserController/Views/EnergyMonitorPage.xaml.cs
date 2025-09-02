using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class EnergyMonitorPage : Page
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly MainViewModel _viewModel;
        private DispatcherTimer _updateTimer;

        public EnergyMonitorPage()
        {
            _serialPortManager = App.Services.GetRequiredService<SerialPortManager>();
            _viewModel = App.Services.GetRequiredService<MainViewModel>();

            this.InitializeComponent();
            InitializeEnergyMonitoring();
        }

        private void InitializeEnergyMonitoring()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2);
            _updateTimer.Tick += async (s, e) => await UpdateEnergyDisplayAsync();
        }

        private async Task UpdateEnergyDisplayAsync()
        {
            if (!_serialPortManager.IsConnected) return;

            try
            {
                EnergyUpdateRing.Visibility = Visibility.Visible;
                bool success = await _serialPortManager.RequestEnergyReadingAsync();
                if (!success)
                    EnergyValueText.Text = "Comm Error";
            }
            catch
            {
                EnergyValueText.Text = "Error";
            }
            finally
            {
                EnergyUpdateRing.Visibility = Visibility.Collapsed;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _updateTimer?.Start();
            EnergyValueText.Text = _viewModel.CurrentEnergy.ToString("F2");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer?.Stop();
        }
    }
}