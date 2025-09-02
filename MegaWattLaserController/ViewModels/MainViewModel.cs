using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LaserControllerApp.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows.Input;

namespace LaserControllerApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly DispatcherTimer _trialTimer;

        [ObservableProperty] private ObservableCollection<string> availablePorts = new();
        [ObservableProperty] private string? selectedPort;
        [ObservableProperty] private ObservableCollection<int> baudRates = new() { 9600, 19200, 38400, 57600, 115200, 230400 };
        [ObservableProperty] private int selectedBaudRate = 115200;
        [ObservableProperty] private string currentState = "Disconnected";
        [ObservableProperty] private bool hasError;
        [ObservableProperty] private string errorMessage = string.Empty;
        [ObservableProperty] private double currentEnergy;

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand RefreshPortsCommand { get; }

        // ✅ Inject SerialPortManager via constructor
        public MainViewModel(SerialPortManager serialPortManager)
        {
            _serialPortManager = serialPortManager;

            ConnectCommand = new AsyncRelayCommand(ConnectAsync);
            DisconnectCommand = new AsyncRelayCommand(DisconnectAsync);
            RefreshPortsCommand = new RelayCommand(RefreshPorts);

            RefreshPorts();

            _trialTimer = new DispatcherTimer();
            _trialTimer.Interval = TimeSpan.FromMinutes(2);
            _trialTimer.Tick += (s, e) =>
            {
                _trialTimer.Stop();
                ShowError("⚠ Trial Version: Please contact developers to get Pro version.");
            };
            _trialTimer.Start();

            WeakReferenceMessenger.Default.Register<EnergyUpdateMessage>(this, (r, m) =>
            {
                CurrentEnergy = m.Energy;
            });
        }

        private async System.Threading.Tasks.Task ConnectAsync()
        {
            if (string.IsNullOrEmpty(SelectedPort)) { ShowError("Select a COM port."); return; }

            bool result = await _serialPortManager.ConnectAsync(SelectedPort, SelectedBaudRate);
            CurrentState = result ? "Connected" : "Failed";
            if (!result) ShowError("Connection failed.");
        }

        private async System.Threading.Tasks.Task DisconnectAsync()
        {
            await _serialPortManager.DisconnectAsync();
            CurrentState = "Disconnected";
        }

        private void RefreshPorts()
        {
            AvailablePorts.Clear();
            foreach (var port in SerialPort.GetPortNames().OrderBy(p => p))
                AvailablePorts.Add(port);

            if (AvailablePorts.Count == 0)
                ShowError("No COM ports detected.");
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }
    }
}
