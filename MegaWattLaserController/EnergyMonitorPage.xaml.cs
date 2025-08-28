using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp
{
    public sealed partial class EnergyMonitorPage : Page
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;
        private DispatcherTimer _updateTimer;

        public EnergyMonitorPage()
        {
            this.InitializeComponent();
            InitializeEnergyMonitoring();
        }

        private void InitializeEnergyMonitoring()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2); // Update every 2 seconds
            _updateTimer.Tick += async (s, e) => await UpdateEnergyDisplayAsync();

            if (_serialPortManager.IsConnected)
            {
                _updateTimer.Start();
            }
        }

        private async Task UpdateEnergyDisplayAsync()
        {
            if (!_serialPortManager.IsConnected)
                return;

            try
            {
                EnergyUpdateRing.Visibility = Visibility.Visible;

                // Send command to request energy reading
                bool success = await _serialPortManager.SendCommandAsync("READ_ENERGY");

                if (success)
                {
                    // In a real implementation, you would parse the response
                    // For now, we'll simulate a value
                    double simulatedEnergy = new Random().NextDouble() * 100;
                    EnergyValueText.Text = simulatedEnergy.ToString("F2");
                }
            }
            catch (Exception ex)
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
            if (_serialPortManager.IsConnected)
            {
                _updateTimer.Start();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer.Stop();
        }
    }
}