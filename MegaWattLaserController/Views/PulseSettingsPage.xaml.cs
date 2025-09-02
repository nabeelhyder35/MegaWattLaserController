using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Models;
using LaserControllerApp.Services;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class PulseSettingsPage : Page
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;

        public PulseSettingsPage()
        {
            this.InitializeComponent();
            InitializeControls();

            // Subscribe to response events if available
            if (_serialPortManager is ICommandResponseHandler responseHandler)
            {
                responseHandler.CommandResponseReceived += ResponseHandler_CommandResponseReceived;
            }
        }

        private void ResponseHandler_CommandResponseReceived(object sender, FpgaCommand e)
        {
            // Handle incoming responses on UI thread
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                ProcessResponse(e);
            });
        }

        private void InitializeControls()
        {
            ModeComboBox.SelectedIndex = 0;
            RepRateSlider.Value = 10;
            PulseWidthSlider.Value = 100;
        }

        private void ProcessResponse(FpgaCommand response)
        {
            switch (response.Command)
            {
                case FpgaCommandIds.lcdRxLsrPulseConfig:
                    ShowSuccessMessage("Pulse configuration applied successfully");
                    DisplayPulseConfigResponse(response.Data);
                    break;

                case FpgaCommandIds.lcdRxBadCmd:
                    ShowErrorMessage("FPGA rejected command: Bad command format");
                    break;

                case FpgaCommandIds.lcdRxNoCmd:
                    ShowErrorMessage("FPGA response: No command received");
                    break;

                case FpgaCommandIds.lcdRxLsrState:
                    DisplayLaserStateResponse(response.Data);
                    break;

                case FpgaCommandIds.lcdRxEnergyValue:
                    DisplayEnergyResponse(response.Data);
                    break;

                case FpgaCommandIds.lcdRxReadTemperature:
                    DisplayTemperatureResponse(response.Data);
                    break;

                default:
                    ShowInfoMessage($"Received response: Command 0x{response.Command:X4}, Data: {BitConverter.ToString(response.Data)}");
                    break;
            }
        }

        private void DisplayPulseConfigResponse(byte[] data)
        {
            if (data.Length >= 6)
            {
                int frequency = (data[0] << 8) | data[1];
                int pulseWidth = (data[2] << 8) | data[3];
                TriggerMode triggerMode = (TriggerMode)data[4];
                FireMode fireMode = (FireMode)data[5];

                string responseMessage = $"Confirmed: Freq={frequency}Hz, Width={pulseWidth}μs, " +
                                       $"Trigger={triggerMode}, Mode={fireMode}";

                if (data.Length >= 8 && fireMode == FireMode.Burst)
                {
                    int burstCount = (data[6] << 8) | data[7];
                    responseMessage += $", BurstCount={burstCount}";
                }

                ResponseTextBlock.Text = responseMessage;
                ResponseInfoBar.Message = responseMessage;
                ResponseInfoBar.Severity = InfoBarSeverity.Success;
                ResponseInfoBar.IsOpen = true;
            }
        }

        private void DisplayLaserStateResponse(byte[] data)
        {
            if (data.Length > 0)
            {
                LaserState state = (LaserState)data[0];
                ResponseTextBlock.Text = $"Laser State: {state}";
                ResponseInfoBar.Message = $"Laser State: {state}";
                ResponseInfoBar.Severity = InfoBarSeverity.Informational;
                ResponseInfoBar.IsOpen = true;
            }
        }

        private void DisplayEnergyResponse(byte[] data)
        {
            float energy = FpgaCommandParser.ParseEnergy(data);
            ResponseTextBlock.Text = $"Energy Reading: {energy:F2} mJ";
            ResponseInfoBar.Message = $"Energy: {energy:F2} mJ";
            ResponseInfoBar.Severity = InfoBarSeverity.Informational;
            ResponseInfoBar.IsOpen = true;
        }

        private void DisplayTemperatureResponse(byte[] data)
        {
            float temperature = FpgaCommandParser.ParseTemperature(data);
            ResponseTextBlock.Text = $"Temperature: {temperature:F1}°C";
            ResponseInfoBar.Message = $"Temperature: {temperature:F1}°C";
            ResponseInfoBar.Severity = InfoBarSeverity.Informational;
            ResponseInfoBar.IsOpen = true;
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModeComboBox.SelectedItem as ComboBoxItem is ComboBoxItem selectedItem)
            {
                bool isBurstMode = selectedItem.Tag?.ToString() == "BURST";
                BurstSettingsPanel.Visibility = isBurstMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void RepRateSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            FrequencyValueText.Text = $"{e.NewValue} Hz";
        }

        private void PulseWidthSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            PulseWidthValueText.Text = $"{e.NewValue} μs";
        }

        private async void ApplyPulseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPortManager.IsConnected)
            {
                ShowErrorMessage("Not connected to laser");
                return;
            }

            try
            {
                ApplyPulseSettingsButton.IsEnabled = false;
                ApplyPulseSettingsButton.Content = "Applying...";
                ClearResponseDisplay();

                // Build FpgaCommand based on settings
                var command = BuildPulseCommand();

                // This method returns void, not bool
                await _serialPortManager.SendCommandAsync(command);

                // Response will be handled by the CommandResponseReceived event
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ApplyPulseSettingsButton.IsEnabled = true;
                ApplyPulseSettingsButton.Content = "Apply Settings";
            }
        }

        private FpgaCommand BuildPulseCommand()
        {
            int frequency = (int)RepRateSlider.Value;
            int pulseWidth = (int)PulseWidthSlider.Value;

            byte[] data = new byte[6];

            // Frequency (Hz) - 2 bytes
            data[0] = (byte)((frequency >> 8) & 0xFF);
            data[1] = (byte)(frequency & 0xFF);

            // Pulse Width (μs) - 2 bytes
            data[2] = (byte)((pulseWidth >> 8) & 0xFF);
            data[3] = (byte)(pulseWidth & 0xFF);

            // Trigger Mode (0 = Internal, 1 = External) - 1 byte
            bool isInternalTrigger = InternalTriggerRadio?.IsChecked == true;
            data[4] = (byte)(isInternalTrigger ? 0 : 1);

            // Fire Mode (0 = Continuous, 1 = Burst) - 1 byte
            bool isBurstMode = (ModeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "BURST";
            data[5] = (byte)(isBurstMode ? 1 : 0);

            if (isBurstMode && int.TryParse(BurstCountTextBox.Text, out int burstCount))
            {
                byte[] burstData = new byte[8];
                Array.Copy(data, 0, burstData, 0, 6);
                burstData[6] = (byte)((burstCount >> 8) & 0xFF);
                burstData[7] = (byte)(burstCount & 0xFF);
                data = burstData;
            }

            return new FpgaCommand(FpgaCommandIds.lcdTxLsrPulseConfig, data);
        }

        private void ClearResponseDisplay()
        {
            ResponseTextBlock.Text = string.Empty;
            ResponseInfoBar.IsOpen = false;
        }

        private void ShowErrorMessage(string message)
        {
            ResponseTextBlock.Text = message;
            ResponseInfoBar.Message = message;
            ResponseInfoBar.Severity = InfoBarSeverity.Error;
            ResponseInfoBar.IsOpen = true;
        }

        private void ShowSuccessMessage(string message)
        {
            ResponseTextBlock.Text = message;
            ResponseInfoBar.Message = message;
            ResponseInfoBar.Severity = InfoBarSeverity.Success;
            ResponseInfoBar.IsOpen = true;
        }

        private void ShowInfoMessage(string message)
        {
            ResponseTextBlock.Text = message;
            ResponseInfoBar.Message = message;
            ResponseInfoBar.Severity = InfoBarSeverity.Informational;
            ResponseInfoBar.IsOpen = true;
        }

        // Clean up event handler when page is unloaded
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_serialPortManager is ICommandResponseHandler responseHandler)
            {
                responseHandler.CommandResponseReceived -= ResponseHandler_CommandResponseReceived;
            }
        }
    }
}