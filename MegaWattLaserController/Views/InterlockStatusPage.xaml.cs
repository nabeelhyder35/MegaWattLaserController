using LaserControllerApp.Models;
using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class InterlockStatusPage : Page
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly MainViewModel _mainViewModel;
        private readonly DispatcherTimer _updateTimer;

        public InterlockStatusPage()
            : this(App.Services.GetRequiredService<SerialPortManager>(),
                   App.Services.GetRequiredService<MainViewModel>())
        {
        }

        public InterlockStatusPage(SerialPortManager serialPortManager, MainViewModel mainViewModel)
        {
            this.InitializeComponent();

            _serialPortManager = serialPortManager ?? throw new ArgumentNullException(nameof(serialPortManager));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2);
            _updateTimer.Tick += UpdateTimer_Tick;

            this.Loaded += Page_Loaded;
            this.Unloaded += Page_Unloaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_serialPortManager.IsConnected)
            {
                _updateTimer.Start();
                LoadInterlockStatus();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer.Stop();
        }

        private async void UpdateTimer_Tick(object sender, object e)
        {
            if (_serialPortManager.IsConnected)
            {
                await LoadInterlockStatusAsync();
            }
            else
            {
                _updateTimer.Stop();
            }
        }

        private async Task LoadInterlockStatusAsync()
        {
            try
            {
                // Request interlock status from FPGA using FpgaCommand
                await _serialPortManager.SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdTxInterlockStatus));

                // In a real implementation, parse the response and update UI accordingly
                // For now, simulate status updates
                SimulateStatusUpdates();
            }
            catch (Exception)
            {
                // Handle error (log or display)
            }
        }

        private void LoadInterlockStatus()
        {
            // Initial load of interlock status
            SimulateStatusUpdates();
        }

        private void SimulateStatusUpdates()
        {
            var random = new Random();

            SetInterlockStatus("CoolantFlow", random.Next(100) > 90 ? "FAULT" : "GOOD");
            SetInterlockStatus("CoolantTemp", random.Next(100) > 85 ? "FAULT" : "GOOD");
            SetInterlockStatus("DoorCover", random.Next(100) > 95 ? "FAULT" : "GOOD");
            SetInterlockStatus("ChargerVoltage", "GOOD");
            SetInterlockStatus("ChargerTemp", "GOOD");
            SetInterlockStatus("Interlock6", "DISABLED");
            SetInterlockStatus("Interlock7", "DISABLED");
            SetInterlockStatus("Interlock8", "DISABLED");
        }

        private void SetInterlockStatus(string interlockName, string status)
        {
            var indicator = FindName($"{interlockName}Indicator") as Border;
            var statusText = FindName($"{interlockName}Status") as TextBlock;

            if (indicator != null && statusText != null)
            {
                switch (status)
                {
                    case "GOOD":
                        indicator.Style = (Style)Resources["GoodStatusStyle"];
                        statusText.Text = "GOOD";
                        statusText.Foreground = new SolidColorBrush(Colors.Green);
                        break;
                    case "FAULT":
                        indicator.Style = (Style)Resources["FaultStatusStyle"];
                        statusText.Text = "FAULT";
                        statusText.Foreground = new SolidColorBrush(Colors.Red);
                        break;
                    case "RECOVERED":
                        indicator.Style = (Style)Resources["RecoveredStatusStyle"];
                        statusText.Text = "RECOVERED";
                        statusText.Foreground = new SolidColorBrush(Colors.Orange);
                        break;
                    case "DISABLED":
                        indicator.Style = (Style)Resources["DisabledStatusStyle"];
                        statusText.Text = "DISABLED";
                        statusText.Foreground = new SolidColorBrush(Colors.Gray);
                        break;
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.IsEnabled = false;
                try
                {
                    await LoadInterlockStatusAsync();
                }
                finally
                {
                    btn.IsEnabled = true;
                }
            }
            else
            {
                await LoadInterlockStatusAsync();
            }
        }

        private async void ResetFaultsButton_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            try
            {
                if (btn != null) btn.IsEnabled = false;

                // Use appropriate command to reset faults; here using lcdTxReset as example
                await _serialPortManager.SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdTxReset));

                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Historical faults have been reset",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to reset faults: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }
    }
}