using LaserControllerApp.Models;
using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class PulseSettingsPage : Page
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly PulseSettingsViewModel _viewModel;

        public PulseSettingsViewModel ViewModel => _viewModel;

        // ✅ Constructor injection
        public PulseSettingsPage(SerialPortManager serialPortManager, PulseSettingsViewModel viewModel)
        {
            _serialPortManager = serialPortManager;
            _viewModel = viewModel;

            this.DataContext = _viewModel;
            this.InitializeComponent();

            _serialPortManager.DataReceived += SerialPortManager_DataReceived;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Page loaded";
            RefreshSettings();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _serialPortManager.DataReceived -= SerialPortManager_DataReceived;
        }

        private async void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPortManager.IsConnected)
            {
                StatusText.Text = "Not connected to laser";
                return;
            }

            StatusText.Text = "Applying pulse settings...";
            bool success = await _viewModel.SendPulseSettingsAsync();

            StatusText.Text = success
                ? "Pulse settings applied successfully"
                : "Failed to apply settings";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshSettings();
        }

        private void RefreshSettings()
        {
            if (!_serialPortManager.IsConnected)
            {
                StatusText.Text = "Not connected to laser";
                return;
            }

            _viewModel.RequestPulseSettings();
            StatusText.Text = "Requested pulse settings from FPGA";
        }

        private void SerialPortManager_DataReceived(object? sender, FpgaCommand e)
        {
            // Only process pulse configuration responses
            if (e.Command == FpgaCommandIds.lcdRxLsrPulseConfig)
            {
                _viewModel.UpdateFromData(e.Data);
                StatusText.Text = $"Last update: {DateTime.Now:T}";
            }
        }
    }
}
