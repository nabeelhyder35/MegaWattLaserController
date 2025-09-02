using LaserControllerApp.Models;
using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace LaserControllerApp.Views
{
    public sealed partial class InterlockStatusPage : Page
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly InterlockStatusViewModel _viewModel;

        public InterlockStatusViewModel ViewModel => _viewModel;

        // ✅ Constructor injection
        public InterlockStatusPage(SerialPortManager serialPortManager, InterlockStatusViewModel viewModel)
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
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _serialPortManager.DataReceived -= SerialPortManager_DataReceived;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPortManager.IsConnected)
            {
                StatusText.Text = "Not connected to laser";
                return;
            }

            StatusText.Text = "Requesting interlock status...";
            bool result = await _serialPortManager.SendCommandAsync(new FpgaCommand(FpgaCommandIds.GetInterlockStatus));

            StatusText.Text = result
                ? "Requested interlock status successfully"
                : "Failed to request interlock status";
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.InterlockItems.Clear();
            StatusText.Text = "Log cleared";
        }

        private void SerialPortManager_DataReceived(object? sender, FpgaCommand e)
        {
            // Only process interlock status responses
            if (e.Command == FpgaCommandIds.lcdRxInterlockStatus)
            {
                _viewModel.UpdateInterlockStatus(e.Data);

                // Use static DispatcherQueue call
                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                {
                    StatusText.Text = $"Last update: {DateTime.Now:T}";
                });
            }
        }
    }
}
