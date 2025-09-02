using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LaserControllerApp.Models;
using LaserControllerApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LaserControllerApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly SerialPortManager _serialPortManager;

        [ObservableProperty]
        private LaserSettings _settings = new LaserSettings();

        [ObservableProperty]
        private double _voltage = 300;

        [ObservableProperty]
        private double _frequency = 100;

        [ObservableProperty]
        private double _pulseWidth = 100;

        [ObservableProperty]
        private LaserState _currentState = LaserState.Idle;

        [ObservableProperty]
        private string _connectionStatus = "Disconnected";

        [ObservableProperty]
        private bool _isConnected = false;

        [ObservableProperty]
        private string _voltageStatusText = "Disconnected from laser controller";

        [ObservableProperty]
        private bool _isVoltageEnabled = false;

        [ObservableProperty]
        private double _currentEnergy = 0.0;

        [ObservableProperty]
        private double _currentTemperature = 0.0;

        [ObservableProperty]
        private int _shotCount = 0;

        [ObservableProperty]
        private double _lampHours = 0.0;

        [ObservableProperty]
        private double _capacitorVoltage = 0.0;

        [ObservableProperty]
        private string _systemStatus = "Initializing";

        [ObservableProperty]
        private bool _isSystemReady = false;

        [ObservableProperty]
        private bool _isBusy = false;

        private string _voltageDisplayText = "Selected Voltage: 0 V";
        public string VoltageDisplayText
        {
            get => _voltageDisplayText;
            set => SetProperty(ref _voltageDisplayText, value);
        }

        public ObservableCollection<StatusItem> StatusItems { get; } = new()
        {
            new StatusItem("Laser State", "GOOD", "IDLE"),
            new StatusItem("System Shots", "GOOD", "0"),
            new StatusItem("Lamp Hours", "GOOD", "0.0"),
            new StatusItem("Temperature", "GOOD", "0.0°C"),
            new StatusItem("Interlocks", "GOOD", "OK"),
            new StatusItem("Energy", "GOOD", "0.0 mJ"),
            new StatusItem("Capacitor Voltage", "GOOD", "0.0 V"),
            new StatusItem("System Status", "GOOD", "Initializing")
        };

        public MainViewModel(SerialPortManager serialPortManager)
        {
            _serialPortManager = serialPortManager;

            // Subscribe to events
            _serialPortManager.ConnectionStatusChanged += SerialPortManager_ConnectionStatusChanged;
            _serialPortManager.DataReceived += SerialPortManager_DataReceived;

            // Subscribe to settings changes
            _settings.PropertyChanged += Settings_PropertyChanged;

            // Initialize voltage display text
            UpdateVoltageDisplayText();
        }

        private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LaserSettings.Voltage))
            {
                UpdateVoltageDisplayText();
            }
        }

        private void OnVoltageChanged()
        {
            UpdateVoltageDisplayText();
        }

        private void UpdateVoltageDisplayText()
        {
            VoltageDisplayText = $"Selected Voltage: {Settings.Voltage} V";
        }

        private void SerialPortManager_ConnectionStatusChanged(object? sender, bool isConnected)
        {
            IsConnected = isConnected;
            ConnectionStatus = isConnected ? "Connected" : "Disconnected";
            UpdateVoltageStatusText(isConnected ? "Connected to laser controller" : "Disconnected from laser controller");

            if (isConnected)
            {
                SystemStatus = "Connected";
                IsSystemReady = true;
                UpdateStatusItem("System Status", "Connected");
            }
            else
            {
                SystemStatus = "Disconnected";
                IsSystemReady = false;
                UpdateStatusItem("System Status", "Disconnected");
            }
        }

        private void SerialPortManager_DataReceived(object? sender, FpgaCommand command)
        {
            ProcessIncomingCommand(command);
        }

        public void ProcessIncomingCommand(FpgaCommand command)
        {
            switch (command.Command)
            {
                case FpgaCommandIds.lcdRxLsrState:
                    if (command.Data.Length >= 1)
                    {
                        CurrentState = (LaserState)command.Data[0];
                        UpdateStatusItem("Laser State", CurrentState.ToString());
                    }
                    break;

                case FpgaCommandIds.lcdRxLsrVolts:
                    if (command.Data.Length >= 2)
                    {
                        Voltage = BitConverter.ToInt16(command.Data, 0);
                        Settings.Voltage = (int)Voltage;
                        UpdateStatusItem("Voltage", $"{Voltage}V");
                        IsVoltageEnabled = Voltage > 0;
                        UpdateVoltageStatusText($"Voltage set to {Voltage}V successfully");
                        OnVoltageChanged();
                    }
                    break;

                case FpgaCommandIds.lcdRxEnergyValue:
                    if (command.Data.Length >= 4)
                    {
                        CurrentEnergy = BitConverter.ToSingle(command.Data, 0);
                        UpdateStatusItem("Energy", $"{CurrentEnergy:F2} mJ");
                        WeakReferenceMessenger.Default.Send(new EnergyUpdateMessage(CurrentEnergy));
                    }
                    break;

                case FpgaCommandIds.lcdRxReadTemperature:
                    if (command.Data.Length >= 4)
                    {
                        CurrentTemperature = BitConverter.ToSingle(command.Data, 0);
                        UpdateStatusItem("Temperature", $"{CurrentTemperature:F1}°C");
                        WeakReferenceMessenger.Default.Send(new TemperatureUpdateMessage(CurrentTemperature));
                    }
                    break;

                case FpgaCommandIds.lcdRxShotCount:
                    if (command.Data.Length >= 4)
                    {
                        ShotCount = BitConverter.ToInt32(command.Data, 0);
                        UpdateStatusItem("System Shots", ShotCount.ToString());
                    }
                    break;

                case FpgaCommandIds.lcdRxLampHours:
                    if (command.Data.Length >= 4)
                    {
                        LampHours = BitConverter.ToSingle(command.Data, 0);
                        UpdateStatusItem("Lamp Hours", $"{LampHours:F1}");
                    }
                    break;

                case FpgaCommandIds.lcdRxCapacitorVoltage:
                    if (command.Data.Length >= 4)
                    {
                        CapacitorVoltage = BitConverter.ToSingle(command.Data, 0);
                        UpdateStatusItem("Capacitor Voltage", $"{CapacitorVoltage:F1} V");
                    }
                    break;

                case FpgaCommandIds.lcdRxInterlockStatus:
                    if (command.Data.Length >= 4)
                    {
                        int interlockStatus = BitConverter.ToInt32(command.Data, 0);
                        string statusText = interlockStatus == 0 ? "OK" : "FAULT";
                        UpdateStatusItem("Interlocks", statusText);
                    }
                    break;
            }
        }

        private void UpdateStatusItem(string name, string value)
        {
            foreach (var item in StatusItems)
            {
                if (item.Name == name)
                {
                    item.Value = value;
                    break;
                }
            }
        }

        private void UpdateVoltageStatusText(string status)
        {
            var factorySettings = new FactorySettings();
            if (Voltage > factorySettings.MaxVoltage)
            {
                VoltageStatusText = $"Warning: Voltage ({Voltage}V) exceeds maximum limit ({factorySettings.MaxVoltage}V)";
            }
            else
            {
                VoltageStatusText = status;
            }
        }

        [RelayCommand]
        public async Task SetVoltage(double voltage)
        {
            var factorySettings = new FactorySettings();
            var validation = LaserSettingsValidator.ValidateVoltage(voltage, factorySettings);
            if (!validation.isValid)
            {
                VoltageStatusText = validation.errorMessage;
                return;
            }

            Settings.Voltage = (int)voltage;
            Voltage = voltage;
            var command = new FpgaCommand(FpgaCommandIds.SetVoltage, BitConverter.GetBytes((int)voltage));
            await _serialPortManager.SendCommandAsync(command);
            OnVoltageChanged();
        }

        [RelayCommand]
        public async Task DisableVoltage()
        {
            await SetVoltage(0);
        }

        [RelayCommand]
        public async Task RequestEnergyReading()
        {
            await _serialPortManager.RequestEnergyReadingAsync();
        }

        [RelayCommand]
        public async Task RequestTemperatureReading()
        {
            await _serialPortManager.RequestTemperatureReadingAsync();
        }

        [RelayCommand]
        public async Task OpenShutter()
        {
            await _serialPortManager.SendShutterConfigCommand(ShutterMode.Manual, ShutterState.Open);
        }

        [RelayCommand]
        public async Task CloseShutter()
        {
            await _serialPortManager.SendShutterConfigCommand(ShutterMode.Manual, ShutterState.Closed);
        }
    }

    public class EnergyUpdateMessage : ValueChangedMessage<double>
    {
        public EnergyUpdateMessage(double value) : base(value) { }
    }

    public class TemperatureUpdateMessage : ValueChangedMessage<double>
    {
        public TemperatureUpdateMessage(double value) : base(value) { }
    }
}