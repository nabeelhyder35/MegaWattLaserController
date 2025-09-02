using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using LaserControllerApp.Models;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class EnergyPage : Page
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;

        public EnergyPage()
        {
            this.InitializeComponent();

            // Subscribe to response events
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

        private void ProcessResponse(FpgaCommand response)
        {
            switch (response.Command)
            {
                case FpgaCommandIds.lcdRxEnergyValue:
                    float energy = FpgaCommandParser.ParseEnergy(response.Data);
                    UpdateEnergyDisplay(energy);
                    break;

                case FpgaCommandIds.lcdRxReadTemperature:
                    float temperature = FpgaCommandParser.ParseTemperature(response.Data);
                    UpdateTemperatureDisplay(temperature);
                    break;

                case FpgaCommandIds.lcdRxLsrVolts:
                    ShowSuccessMessage("Voltage set successfully");
                    break;

                case FpgaCommandIds.lcdRxBadCmd:
                    ShowErrorMessage("FPGA rejected voltage command");
                    break;

                default:
                    // Ignore other commands
                    break;
            }
        }

        private void UpdateEnergyDisplay(float energy)
        {
            // Update UI with energy value
            EnergyValueText.Text = $"{energy:F2} mJ";
            ShowInfoMessage($"Energy reading: {energy:F2} mJ");
        }

        private void UpdateTemperatureDisplay(float temperature)
        {
            // Update UI with temperature value
            TemperatureValueText.Text = $"{temperature:F1} °C";
            ShowInfoMessage($"Temperature: {temperature:F1} °C");
        }

        private async void SetVoltage_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPortManager.IsConnected)
            {
                ShowErrorMessage("Not connected to laser");
                return;
            }

            try
            {
                SetVoltageButton.IsEnabled = false;
                SetVoltageButton.Content = "Setting...";

                // Get voltage value from slider or textbox
                int voltage = (int)VoltageSlider.Value;

                // Send voltage command
                var command = new FpgaCommand
                {
                    Command = FpgaCommandIds.lcdTxLsrVolts,
                    Data = BitConverter.GetBytes((ushort)voltage)
                };

                await _serialPortManager.SendCommandAsync(command);
                StatusTextBlock.Text = $"Setting voltage to {voltage}V...";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                SetVoltageButton.IsEnabled = true;
                SetVoltageButton.Content = "Set Voltage";
            }
        }

        private async void ReadEnergy_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPortManager.IsConnected)
            {
                ShowErrorMessage("Not connected to laser");
                return;
            }

            try
            {
                ReadEnergyButton.IsEnabled = false;
                ReadEnergyButton.Content = "Reading...";

                // Request energy reading
                await _serialPortManager.RequestEnergyReadingAsync();
                StatusTextBlock.Text = "Requesting energy reading...";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ReadEnergyButton.IsEnabled = true;
                ReadEnergyButton.Content = "Read Energy";
            }
        }

        private async void ReadTemperature_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPortManager.IsConnected)
            {
                ShowErrorMessage("Not connected to laser");
                return;
            }

            try
            {
                ReadTemperatureButton.IsEnabled = false;
                ReadTemperatureButton.Content = "Reading...";

                // Request temperature reading
                await _serialPortManager.RequestTemperatureReadingAsync();
                StatusTextBlock.Text = "Requesting temperature reading...";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ReadTemperatureButton.IsEnabled = true;
                ReadTemperatureButton.Content = "Read Temperature";
            }
        }

        private void ShowErrorMessage(string message)
        {
            StatusTextBlock.Text = message;
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = InfoBarSeverity.Error;
            StatusInfoBar.IsOpen = true;
        }

        private void ShowSuccessMessage(string message)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = InfoBarSeverity.Success;
            StatusInfoBar.IsOpen = true;
        }

        private void ShowInfoMessage(string message)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = InfoBarSeverity.Informational;
            StatusInfoBar.IsOpen = true;
        }
        private void VoltageSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            VoltageValueText.Text = $"{e.NewValue} V";
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