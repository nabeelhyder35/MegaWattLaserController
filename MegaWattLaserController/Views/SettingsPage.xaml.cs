using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly SettingsViewModel _viewModel;

        public SettingsViewModel ViewModel => _viewModel;

        // ✅ Constructor injection
        public SettingsPage(SerialPortManager serialPortManager, SettingsViewModel viewModel)
        {
            _serialPortManager = serialPortManager;
            _viewModel = viewModel;

            this.DataContext = _viewModel;
            this.InitializeComponent();

            _serialPortManager.ConnectionStatusChanged += SerialPortManager_ConnectionStatusChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.RefreshPorts();
            StatusText.Text = _serialPortManager.IsConnected ? "Connected" : "Disconnected";
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _serialPortManager.ConnectionStatusChanged -= SerialPortManager_ConnectionStatusChanged;
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_viewModel.SelectedPort))
            {
                StatusText.Text = "Select a port first";
                return;
            }

            StatusText.Text = "Connecting...";
            bool success = await _serialPortManager.ConnectAsync(_viewModel.SelectedPort, _viewModel.SelectedBaudRate);
            StatusText.Text = success ? $"Connected to {_viewModel.SelectedPort}" : "Failed to connect";
        }

        private async void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            await _serialPortManager.DisconnectAsync();
            StatusText.Text = "Disconnected";
        }

        private void SerialPortManager_ConnectionStatusChanged(object? sender, bool isConnected)
        {
            StatusText.Text = isConnected ? "Connected" : "Disconnected";
        }
    }
}
