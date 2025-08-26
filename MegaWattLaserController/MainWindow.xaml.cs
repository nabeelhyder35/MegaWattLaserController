using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using LaserControllerApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;

namespace LaserControllerApp
{
    public sealed partial class MainWindow : Window
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;
        private bool _isNavigating;

        public ObservableCollection<string> AvailablePorts { get; } = new ObservableCollection<string>();
        public ObservableCollection<int> BaudRates { get; } = new ObservableCollection<int> { 9600, 19200, 38400, 57600, 115200 };

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeAsync();
            SubscribeToEvents();
        }

        private async void InitializeAsync()
        {
            // Set initial baud rate
            BaudRateComboBox.SelectedItem = 9600;

            // Load available ports
            await LoadAvailablePortsAsync();

            // Navigate to first page
            NavigateToPage(typeof(ShutterPage));
        }

        private void SubscribeToEvents()
        {
            _serialPortManager.ConnectionStatusChanged += SerialPortManager_ConnectionStatusChanged;
            _serialPortManager.DataReceived += SerialPortManager_DataReceived;
            _serialPortManager.ErrorOccurred += SerialPortManager_ErrorOccurred;
        }

        private void UnsubscribeFromEvents()
        {
            _serialPortManager.ConnectionStatusChanged -= SerialPortManager_ConnectionStatusChanged;
            _serialPortManager.DataReceived -= SerialPortManager_DataReceived;
            _serialPortManager.ErrorOccurred -= SerialPortManager_ErrorOccurred;
        }

        private void SerialPortManager_ConnectionStatusChanged(object sender, string status)
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                StatusTextBlock.Text = status;
                UpdateConnectionStatus(_serialPortManager.IsConnected);
            });
        }

        private void SerialPortManager_DataReceived(object sender, string data)
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                if (data.Length > 100)
                    data = data.Substring(0, 100) + "...";
                StatusTextBlock.Text = $"Received: {data.Trim()}";
            });
        }

        private void SerialPortManager_ErrorOccurred(object sender, string errorMessage)
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                ShowErrorMessage(errorMessage);
            });
        }

        private async Task LoadAvailablePortsAsync()
        {
            try
            {
                AvailablePorts.Clear();
                var ports = _serialPortManager.GetAvailablePorts();

                foreach (var port in ports)
                {
                    AvailablePorts.Add(port);
                }

                if (AvailablePorts.Any() && PortComboBox.SelectedItem == null)
                {
                    PortComboBox.SelectedItem = AvailablePorts.First();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Failed to load ports: {ex.Message}");
            }
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            ConnectionStatusEllipse.Fill = isConnected ?
                new SolidColorBrush(Microsoft.UI.Colors.Green) :
                new SolidColorBrush(Microsoft.UI.Colors.Red);

            ConnectButton.IsEnabled = !isConnected;
            DisconnectButton.IsEnabled = isConnected;
            RefreshPortsButton.IsEnabled = !isConnected;
            PortComboBox.IsEnabled = !isConnected;
            BaudRateComboBox.IsEnabled = !isConnected;
        }

        private void ShowErrorMessage(string message)
        {
            StatusTextBlock.Text = $"Error: {message}";
            StatusTextBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (PortComboBox.SelectedItem is string selectedPort &&
                BaudRateComboBox.SelectedItem is int selectedBaudRate)
            {
                ConnectButton.IsEnabled = false;
                StatusTextBlock.Text = $"Connecting to {selectedPort}...";

                var success = await _serialPortManager.ConnectAsync(selectedPort, selectedBaudRate);

                if (!success)
                {
                    ConnectButton.IsEnabled = true;
                }
            }
            else
            {
                ShowErrorMessage("Please select a valid port and baud rate");
            }
        }

        private async void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            DisconnectButton.IsEnabled = false;
            StatusTextBlock.Text = "Disconnecting...";
            await _serialPortManager.DisconnectAsync();
        }

        private async void RefreshPorts_Click(object sender, RoutedEventArgs e)
        {
            RefreshPortsButton.IsEnabled = false;
            await LoadAvailablePortsAsync();
            RefreshPortsButton.IsEnabled = true;
        }

        private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (_isNavigating) return;

            if (args.SelectedItem is NavigationViewItem item)
            {
                _isNavigating = true;
                try
                {
                    switch (item.Tag?.ToString())
                    {
                        case "ShutterPage":
                            NavigateToPage(typeof(ShutterPage));
                            break;
                        case "EnergyPage":
                            NavigateToPage(typeof(EnergyPage));
                            break;
                        case "CustomCommandsPage":
                            NavigateToPage(typeof(CustomPage));
                            break;
                        default:
                            NavigateToPage(typeof(ShutterPage));
                            break;
                    }
                }
                finally
                {
                    _isNavigating = false;
                }
            }
        }

        private void NavigateToPage(Type pageType)
        {
            try
            {
                ContentFrame.Navigate(pageType);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Failed to navigate: {ex.Message}");
            }
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            ShowErrorMessage($"Navigation failed: {e.Exception.Message}");
            e.Handled = true;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            UnsubscribeFromEvents();
            _serialPortManager.Dispose();
        }

        private void BaudRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BaudRateComboBox.SelectedItem is int selectedBaudRate)
            {
                _serialPortManager.UpdateBaudRate(selectedBaudRate);
            }
        }
    }
}