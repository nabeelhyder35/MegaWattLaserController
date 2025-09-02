using LaserControllerApp.Models;
using LaserControllerApp.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class ShutterPage : Page
    {
        private readonly SerialPortManager _serialPortManager;

        // ✅ Constructor injection
        public ShutterPage(SerialPortManager serialPortManager)
        {
            _serialPortManager = serialPortManager;

            this.InitializeComponent();
            _serialPortManager.DataReceived += SerialPortManager_DataReceived;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateShutterStatus();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _serialPortManager.DataReceived -= SerialPortManager_DataReceived;
        }

        private async void OpenShutter_Click(object sender, RoutedEventArgs e)
        {
            await SetShutterStateAsync(true);
        }

        private async void CloseShutter_Click(object sender, RoutedEventArgs e)
        {
            await SetShutterStateAsync(false);
        }

        private async void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            await RequestShutterStatusAsync();
        }

        private async Task SetShutterStateAsync(bool open)
        {
            if (!_serialPortManager.IsConnected)
            {
                StatusText.Text = "Not connected to laser";
                return;
            }

            byte[] data = new byte[1] { open ? (byte)1 : (byte)0 };
            var command = new FpgaCommand(FpgaCommandIds.lcdTxShutterConfig, data);

            await _serialPortManager.SendCommandAsync(command);
            StatusText.Text = open ? "Opening shutter..." : "Closing shutter...";
        }

        private async Task RequestShutterStatusAsync()
        {
            if (!_serialPortManager.IsConnected)
            {
                StatusText.Text = "Not connected to laser";
                return;
            }

            var command = new FpgaCommand(FpgaCommandIds.lcdTxShutterConfig);
            await _serialPortManager.SendCommandAsync(command);
            StatusText.Text = "Requesting shutter status...";
        }

        private void SerialPortManager_DataReceived(object? sender, FpgaCommand e)
        {
            // Check for shutter status response
            if (e.Command == FpgaCommandIds.lcdRxShutterConfig && e.Data.Length > 0)
            {
                bool isOpen = e.Data[0] == 1;
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    ShutterStatusText.Text = isOpen ? "Open" : "Closed";
                    StatusText.Text = "Shutter status updated";
                });
            }
        }

        private void UpdateShutterStatus()
        {
            ShutterStatusText.Text = "Unknown";
            StatusText.Text = "Idle";
        }
    }
}
