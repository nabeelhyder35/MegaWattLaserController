using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaserControllerApp.Services;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI;

namespace LaserControllerApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly DispatcherQueue _dispatcherQueue;

        [ObservableProperty]
        private string _currentState = "Disconnected";

        [ObservableProperty]
        private bool _isConnected;

        [ObservableProperty]
        private ObservableCollection<string> _availablePorts;

        [ObservableProperty]
        private string _selectedPort;

        [ObservableProperty]
        private ObservableCollection<int> _baudRates = new ObservableCollection<int> { 9600, 19200, 38400, 57600, 115200 };

        [ObservableProperty]
        private int _selectedBaudRate = 9600;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        // Computed properties for XAML binding
        public bool IsNotConnected => !IsConnected;

        public bool CanConnect => !IsConnected && !string.IsNullOrEmpty(SelectedPort);

        public SolidColorBrush ConnectionStatusColor => IsConnected ?
            new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);

        public event EventHandler<bool> ConnectionStatusChanged;

        public MainViewModel(SerialPortManager serialPortManager)
        {
            _serialPortManager = serialPortManager;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _availablePorts = new ObservableCollection<string>(_serialPortManager.GetAvailablePorts());

            // Subscribe to SerialPortManager events
            _serialPortManager.ConnectionStatusChanged += OnConnectionStatusChanged;
            _serialPortManager.ErrorOccurred += OnErrorOccurred;

            UpdateConnectionStatus(_serialPortManager.IsConnected);
        }

        private void OnConnectionStatusChanged(object sender, bool isConnected)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateConnectionStatus(isConnected);
                ConnectionStatusChanged?.Invoke(this, isConnected);
            });
        }

        private void OnErrorOccurred(object sender, string error)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                ErrorMessage = error;
                HasError = true;
            });
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            IsConnected = isConnected;
            CurrentState = isConnected ? "Connected" : "Disconnected";
            System.Diagnostics.Debug.WriteLine($"MainViewModel: Connection status updated to {CurrentState}");

            // Notify that computed properties have changed
            OnPropertyChanged(nameof(IsNotConnected));
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(ConnectionStatusColor));
        }

        // This method is called when any ObservableProperty changes
        partial void OnSelectedPortChanged(string value)
        {
            // Update CanConnect when SelectedPort changes
            OnPropertyChanged(nameof(CanConnect));
        }

        [RelayCommand]
        private async Task Connect()
        {
            if (string.IsNullOrEmpty(SelectedPort))
            {
                ErrorMessage = "Please select a port.";
                HasError = true;
                return;
            }

            HasError = false;
            bool success = await _serialPortManager.ConnectAsync(SelectedPort, SelectedBaudRate);
            if (!success)
            {
                ErrorMessage = $"Failed to connect to {SelectedPort}.";
                HasError = true;
            }
        }

        [RelayCommand]
        private async Task Disconnect()
        {
            HasError = false;
            await _serialPortManager.DisconnectAsync();
        }

        [RelayCommand]
        private void RefreshPorts()
        {
            AvailablePorts.Clear();
            foreach (var port in _serialPortManager.GetAvailablePorts())
            {
                AvailablePorts.Add(port);
            }
            HasError = false;
        }
    }
}