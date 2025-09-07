using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaserControllerApp.Services;
using LaserControllerApp.Views;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI;

namespace LaserControllerApp.ViewModels
{
    public partial class LaserControlDashboardViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<StatusItem> statusItems = new ObservableCollection<StatusItem>();
        [ObservableProperty] private ObservableCollection<string> logMessages = new ObservableCollection<string>();
        [ObservableProperty] private bool isConnected = false;
        [ObservableProperty] private bool isArmed = false;
        [ObservableProperty] private bool isRunning = false;
        [ObservableProperty] private bool isPaused = false;
        [ObservableProperty] private bool isLoading = false;
        [ObservableProperty] private string systemStatus = "Disconnected";
        [ObservableProperty] private string connectionStatus = "Disconnected";
        [ObservableProperty] private SolidColorBrush connectionStatusColor = new SolidColorBrush(Colors.Gray);

        private readonly SerialPortManager _serialPortManager;
        private readonly MainViewModel _mainViewModel;
        private readonly IDialogService _dialogService;
        private DispatcherQueue _dispatcherQueue;
        private Random random = new Random();

        public IAsyncRelayCommand ToggleArmLaserCommand { get; }
        public IAsyncRelayCommand RunLaserCommand { get; }
        public IAsyncRelayCommand PauseLaserCommand { get; }
        public IAsyncRelayCommand ResumeLaserCommand { get; }
        public IAsyncRelayCommand StartChargingCommand { get; }
        public IAsyncRelayCommand StopChargingCommand { get; }
        public IAsyncRelayCommand OpenShutterCommand { get; }
        public IAsyncRelayCommand CloseShutterCommand { get; }
        public IAsyncRelayCommand OpenPulseSettingsCommand { get; }
        public IAsyncRelayCommand ResetSystemCommand { get; }
        public IAsyncRelayCommand EmergencyStopCommand { get; }
        public IAsyncRelayCommand RefreshStatusCommand { get; }

        public LaserControlDashboardViewModel(
            SerialPortManager serialPortManager,
            MainViewModel mainViewModel,
            IDialogService dialogService)
        {
            _serialPortManager = serialPortManager;
            _mainViewModel = mainViewModel;
            _dialogService = dialogService;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Initialize 6 status items
            statusItems.Add(new StatusItem { Name = "ENERGY" });
            statusItems.Add(new StatusItem { Name = "VOLTAGE" });
            statusItems.Add(new StatusItem { Name = "SHOTS" });
            statusItems.Add(new StatusItem { Name = "TEMP" });
            statusItems.Add(new StatusItem { Name = "INTERLOCKS" });
            statusItems.Add(new StatusItem { Name = "SYSTEM" });

            // Initialize commands
            ToggleArmLaserCommand = new AsyncRelayCommand(ToggleArmLaserAsync);
            RunLaserCommand = new AsyncRelayCommand(RunLaserAsync);
            PauseLaserCommand = new AsyncRelayCommand(PauseLaserAsync);
            ResumeLaserCommand = new AsyncRelayCommand(ResumeLaserAsync);
            StartChargingCommand = new AsyncRelayCommand(StartChargingAsync);
            StopChargingCommand = new AsyncRelayCommand(StopChargingAsync);
            OpenShutterCommand = new AsyncRelayCommand(OpenShutterAsync);
            CloseShutterCommand = new AsyncRelayCommand(CloseShutterAsync);
            OpenPulseSettingsCommand = new AsyncRelayCommand(OpenPulseSettingsAsync);
            ResetSystemCommand = new AsyncRelayCommand(ResetSystemAsync);
            EmergencyStopCommand = new AsyncRelayCommand(EmergencyStopAsync);
            RefreshStatusCommand = new AsyncRelayCommand(RefreshStatusAsync);

            // Subscribe to connection status changes
            _mainViewModel.ConnectionStatusChanged += OnMainConnectionStatusChanged;

            // Subscribe to serial port events
            _serialPortManager.CommandResponseReceived += OnCommandResponseReceived;
            _serialPortManager.DataReceived += OnDataReceived;
            _serialPortManager.LogMessageAdded += OnLogMessageAdded;

            // Initialize connection status
            UpdateConnectionStatus(_mainViewModel.IsConnected);
        }

        private void OnMainConnectionStatusChanged(object sender, bool isConnected)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateConnectionStatus(isConnected);
            });
        }

        private void OnLogMessageAdded(object sender, string message)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                AddLog(message);
            });
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            IsConnected = isConnected;
            ConnectionStatus = isConnected ? "Connected" : "Disconnected";
            ConnectionStatusColor = isConnected ?
                new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        }

        private void OnCommandResponseReceived(object sender, Models.FpgaCommand command)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                // Handle specific command responses here
                AddLog($"Received response: {command.Command:X4}");
            });
        }

        private void OnDataReceived(object sender, Models.FpgaCommand command)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                // Process incoming data and update UI
                ProcessIncomingData(command);
            });
        }

        private void ProcessIncomingData(Models.FpgaCommand command)
        {
            // Handle different types of incoming data
            switch (command.Command)
            {
                case Models.FpgaCommandIds.lcdTxEnergyValue:
                    if (command.Data.Length >= 2)
                    {
                        ushort energyValue = (ushort)((command.Data[0] << 8) | command.Data[1]);
                        StatusItems[0].Value = energyValue.ToString();
                        AddLog($"Energy reading: {energyValue}J");
                    }
                    break;

                case Models.FpgaCommandIds.lcdTxCapacitorVoltage:
                    if (command.Data.Length >= 2)
                    {
                        ushort voltageValue = (ushort)((command.Data[0] << 8) | command.Data[1]);
                        StatusItems[1].Value = voltageValue.ToString();
                        AddLog($"Voltage reading: {voltageValue}V");
                    }
                    break;

                case Models.FpgaCommandIds.lcdTxShotCount:
                    if (command.Data.Length >= 4)
                    {
                        uint shotCount = (uint)((command.Data[0] << 24) | (command.Data[1] << 16) | (command.Data[2] << 8) | command.Data[3]);
                        StatusItems[2].Value = shotCount.ToString();
                        AddLog($"Shot count: {shotCount}");
                    }
                    break;

                case Models.FpgaCommandIds.lcdTxTemperatureValue:
                    if (command.Data.Length >= 2)
                    {
                        ushort temperature = (ushort)((command.Data[0] << 8) | command.Data[1]);
                        StatusItems[3].Value = temperature.ToString();
                        AddLog($"Temperature: {temperature}°C");
                    }
                    break;

                case Models.FpgaCommandIds.lcdTxInterlockStatus:
                    if (command.Data.Length >= 1)
                    {
                        byte interlockStatus = command.Data[0];
                        StatusItems[4].Value = interlockStatus == 0 ? "OK" : "ERROR";
                        StatusItems[4].IsWarning = interlockStatus != 0;
                        AddLog($"Interlock status: {(interlockStatus == 0 ? "OK" : "ERROR")}");
                    }
                    break;

                case Models.FpgaCommandIds.lcdTxLsrState:
                    if (command.Data.Length >= 1)
                    {
                        byte systemState = command.Data[0];
                        string stateText = systemState switch
                        {
                            0 => "Idle",
                            1 => "Charging",
                            2 => "Armed",
                            3 => "Running",
                            4 => "Paused",
                            _ => "Unknown"
                        };
                        StatusItems[5].Value = stateText;
                        AddLog($"System state: {stateText}");
                    }
                    break;

                    // Add cases for other data types...
            }
        }

        private async Task OpenPulseSettingsAsync()
        {
            if (!IsConnected) return;

            try
            {
                // Use default values for the dialog
                ushort defaultVoltage = 1000;
                ushort defaultEnergy = 5;
                ushort defaultPulseWidth = 100;
                uint defaultShots = 10;
                ushort defaultFrequency = 1;

                // Use the dialog service to show the pulse settings dialog
                var result = await _dialogService.ShowPulseSettingsDialogWithResultAsync(
                    defaultVoltage, defaultEnergy, defaultPulseWidth, defaultShots, defaultFrequency);

                if (result.HasValue)
                {
                    var (voltage, energy, pulseWidth, shots, frequency) = result.Value;

                    // Log the command being sent
                    AddLog($"Sending pulse settings: V={voltage}V, E={energy}J, PW={pulseWidth}µs, Shots={shots}, Freq={frequency}Hz");

                    // Send the pulse settings to the laser
                    bool success = await _serialPortManager.SetPulseSettingsAsync(
                        voltage, energy, pulseWidth, shots, frequency
                    );

                    if (success)
                    {
                        AddLog("Pulse settings updated successfully");
                    }
                    else
                    {
                        AddLog("Failed to update pulse settings");
                    }
                }
                else
                {
                    AddLog("Pulse settings dialog cancelled");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error opening pulse settings: {ex.Message}");
            }
        }
        private async Task ToggleArmLaserAsync()
        {
            if (!IsConnected) return;

            try
            {
                bool newArmState = !IsArmed;
                AddLog($"Sending {(newArmState ? "Arm" : "Disarm")} command");

                bool success = await _serialPortManager.SetLaserStateAsync(newArmState);

                if (success)
                {
                    IsArmed = newArmState;
                    AddLog($"Laser {(IsArmed ? "Armed" : "Disarmed")}");
                }
                else
                {
                    AddLog("Failed to change laser arm state");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private async Task RunLaserAsync()
        {
            if (!IsConnected || !IsArmed) return;

            try
            {
                AddLog("Sending Run command");
                bool success = await _serialPortManager.SetLaserRunningStateAsync(true);

                if (success)
                {
                    IsRunning = true;
                    IsPaused = false;
                    SystemStatus = "Firing";
                    AddLog("Laser Fired");
                }
                else
                {
                    AddLog("Failed to start laser");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private async Task PauseLaserAsync()
        {
            if (!IsConnected || !IsRunning) return;

            try
            {
                AddLog("Sending Pause command");
                bool success = await _serialPortManager.SetLaserRunningStateAsync(false);

                if (success)
                {
                    IsRunning = false;
                    IsPaused = true;
                    SystemStatus = "Paused";
                    AddLog("Laser Paused");
                }
                else
                {
                    AddLog("Failed to pause laser");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private async Task ResumeLaserAsync()
        {
            if (!IsConnected || !IsPaused) return;

            try
            {
                AddLog("Sending Resume command");
                bool success = await _serialPortManager.SetLaserRunningStateAsync(true);

                if (success)
                {
                    IsRunning = true;
                    IsPaused = false;
                    SystemStatus = "Firing";
                    AddLog("Laser Resumed");
                }
                else
                {
                    AddLog("Failed to resume laser");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private async Task StartChargingAsync()
        {
            if (!IsConnected) return;

            try
            {
                // Use a default voltage for charging
                ushort chargeVoltage = 1000;
                AddLog($"Sending Start Charging command: {chargeVoltage}V");

                bool success = await _serialPortManager.StartChargingAsync(chargeVoltage);

                if (success)
                {
                    AddLog("Charging Started");
                }
                else
                {
                    AddLog("Failed to start charging");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private async Task StopChargingAsync()
        {
            if (!IsConnected) return;

            try
            {
                AddLog("Sending Stop Charging command");
                bool success = await _serialPortManager.StopChargingAsync();

                if (success)
                {
                    AddLog("Charging Stopped");
                }
                else
                {
                    AddLog("Failed to stop charging");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private async Task OpenShutterAsync()
        {
            if (!IsConnected) return;

            try
            {
                AddLog("Sending Open Shutter command");
                bool success = await _serialPortManager.SetShutterStateAsync(true);

                if (success)
                {
                    AddLog("Shutter Opened");
                }
                else
                {
                    AddLog("Failed to open shutter");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private async Task CloseShutterAsync()
        {
            if (!IsConnected) return;

            try
            {
                AddLog("Sending Close Shutter command");
                bool success = await _serialPortManager.SetShutterStateAsync(false);

                if (success)
                {
                    AddLog("Shutter Closed");
                }
                else
                {
                    AddLog("Failed to close shutter");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private async Task ResetSystemAsync()
        {
            if (!IsConnected) return;

            try
            {
                AddLog("Sending System Reset command");
                bool success = await _serialPortManager.ResetSystemAsync();

                if (success)
                {
                    // Reset local state
                    IsArmed = false;
                    IsRunning = false;
                    IsPaused = false;
                    SystemStatus = "Reset";
                    AddLog("System Reset");
                }
                else
                {
                    AddLog("Failed to reset system");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private async Task EmergencyStopAsync()
        {
            if (!IsConnected) return;

            try
            {
                AddLog("Sending Emergency Stop command");
                // Emergency stop by disarming the laser
                bool success = await _serialPortManager.SetLaserStateAsync(false);

                if (success)
                {
                    IsArmed = false;
                    IsRunning = false;
                    IsPaused = false;
                    SystemStatus = "Emergency Stop!";
                    AddLog("Emergency Stop Triggered");
                }
                else
                {
                    AddLog("Failed to execute emergency stop");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
            }
        }

        private async Task RefreshStatusAsync()
        {
            if (!IsConnected) return;

            IsLoading = true;

            try
            {
                AddLog("Sending Status Refresh commands");

                // Request all status information
                await Task.WhenAll(
                    _serialPortManager.RequestEnergyReadingAsync(),
                    _serialPortManager.RequestVoltageReadingAsync(),
                    _serialPortManager.RequestShotCountAsync(),
                    _serialPortManager.RequestTemperatureReadingAsync(),
                    _serialPortManager.RequestInterlockStatusAsync(),
                    _serialPortManager.RequestSystemStatusAsync()
                );

                AddLog("Status Refreshed");
            }
            catch (Exception ex)
            {
                AddLog($"Error refreshing status: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddLog(string message)
        {
            LogMessages.Insert(0, $"{DateTime.Now:HH:mm:ss} - {message}");
        }
    }

    public partial class StatusItem : ObservableObject
    {
        [ObservableProperty] private string name = "";
        [ObservableProperty] private string value = "--";
        [ObservableProperty] private bool isWarning = false;
        [ObservableProperty] private double progressValue = 0;
    }
}