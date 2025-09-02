using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using System.Linq;
using System.Collections.ObjectModel;
using System;

namespace LaserControllerApp
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;
        private string _selectedPort = null;

        public SettingsPage()
        {
            this.InitializeComponent();
            PopulatePortComboBox();
            UpdateConnectionStatus();
        }

        private void PopulatePortComboBox()
        {
            var ports = _serialPortManager.GetAvailablePorts();
            PortComboBox.Items.Clear();

            if (ports != null && ports.Length > 0)
            {
                foreach (var port in ports.OrderBy(p => p))
                {
                    PortComboBox.Items.Add(port);
                }
            }
            else
            {
                PortComboBox.Items.Add("No ports available");
                PortComboBox.IsEnabled = false;
            }
        }

        private void PortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PortComboBox.SelectedItem is string portName && !string.IsNullOrEmpty(portName))
            {
                _selectedPort = portName;
                UpdateButtonStates();
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedPort))
            {
                await _serialPortManager.ConnectAsync(_selectedPort);
                UpdateButtonStates();
                UpdateConnectionStatus();
            }
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await _serialPortManager.DisconnectAsync();
            UpdateButtonStates();
            UpdateConnectionStatus();
        }

        private void UpdateButtonStates()
        {
            ConnectButton.IsEnabled = !_serialPortManager.IsConnected && !string.IsNullOrEmpty(_selectedPort);
            DisconnectButton.IsEnabled = _serialPortManager.IsConnected;
        }

        private void UpdateConnectionStatus()
        {
            ConnectionStatusText.Text = _serialPortManager.IsConnected ?
                $"Connected to {_serialPortManager.PortName}" : "Disconnected";
        }
    }
}