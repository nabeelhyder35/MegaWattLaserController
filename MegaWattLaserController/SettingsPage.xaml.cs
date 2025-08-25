using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Microsoft.UI.Dispatching;

namespace LaserControllerApp
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SerialPortManager serialPortManager = SerialPortManager.Instance;
        private string selectedPort = null;
        private readonly DispatcherQueue dispatcherQueue;

        public SettingsPage()
        {
            this.InitializeComponent();
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            PopulatePortComboBox();
            serialPortManager.ConnectionStatusChanged += SerialPortManager_ConnectionStatusChanged;
            UpdateConnectionStatus();
        }

        private void PopulatePortComboBox()
        {
            var ports = serialPortManager.GetAvailablePorts();
            if (ports != null && ports.Length > 0)
            {
                dispatcherQueue.TryEnqueue(() =>
                {
                    PortComboBox.Items.Clear();
                    PortComboBox.Items.Add(new ComboBoxItem { IsEnabled = false, Content = "Select a port..." });
                    foreach (var port in ports.OrderBy(p => p))
                    {
                        PortComboBox.Items.Add(new ComboBoxItem { Content = port });
                    }
                    PortComboBox.SelectedIndex = 0;
                });
            }
            else
            {
                dispatcherQueue.TryEnqueue(() =>
                {
                    PortComboBox.Items.Clear();
                    PortComboBox.Items.Add(new ComboBoxItem { IsEnabled = false, Content = "No ports available" });
                });
            }
        }

        private void PortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PortComboBox.SelectedItem is ComboBoxItem item && item.Content is string portName && !string.IsNullOrEmpty(portName) && portName != "Select a port..." && portName != "No ports available")
            {
                selectedPort = portName;
                UpdateButtonStates();
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedPort) && serialPortManager.Connect(selectedPort))
            {
                UpdateButtonStates();
                UpdateConnectionStatus();
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            serialPortManager.Disconnect();
            UpdateButtonStates();
            UpdateConnectionStatus();
        }

        private void SerialPortManager_ConnectionStatusChanged(object sender, string status)
        {
            dispatcherQueue.TryEnqueue(() => UpdateConnectionStatus());
        }

        private void UpdateButtonStates()
        {
            bool isConnected = serialPortManager.GetLogMessages().Any(m => m.Contains("Connected"));
            dispatcherQueue.TryEnqueue(() =>
            {
                ConnectButton.IsEnabled = !isConnected && !string.IsNullOrEmpty(selectedPort);
                DisconnectButton.IsEnabled = isConnected;
            });
        }

        private void UpdateConnectionStatus()
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                ConnectionStatusText.Text = serialPortManager.GetLogMessages().LastOrDefault(m => m.Contains("Connected") || m.Contains("Disconnected") || m.Contains("failed")) ?? "Not connected";
            });
        }
    }
}