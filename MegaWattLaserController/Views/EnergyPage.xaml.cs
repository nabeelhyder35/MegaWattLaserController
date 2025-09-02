using LaserControllerApp.Models;
using LaserControllerApp.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace LaserControllerApp.Views
{
    public sealed partial class EnergyPage : Page
    {
        private readonly SerialPortManager _serialPortManager;

        // ✅ Inject SerialPortManager via constructor
        public EnergyPage(SerialPortManager serialPortManager)
        {
            this.InitializeComponent();
            _serialPortManager = serialPortManager;

            InitializeControls();
        }

        private void InitializeControls()
        {
            // Set initial values
            VoltageSlider.Value = 350;
            UpdateStatusDisplay();
        }

        private async void SetVoltage_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPortManager.IsConnected)
            {
                ShowErrorMessage("Not connected to laser");
                return;
            }

            int voltage = (int)VoltageSlider.Value;

            if (voltage < 0 || voltage > 1000)
            {
                ShowErrorMessage("Voltage must be between 0-1000V");
                return;
            }

            SetVoltageButton.IsEnabled = false;
            SetVoltageButton.Content = "Setting...";

            try
            {
                // Ensure this ID exists in FpgaCommandIds
                ushort cmdId = FpgaCommandIds.SetVoltage;

                // Convert voltage to byte[] (little-endian)
                byte[] data = BitConverter.GetBytes((ushort)voltage);

                var command = new FpgaCommand(cmdId, data);

                await _serialPortManager.SendCommandAsync(command);

                UpdateStatusDisplay();
                ShowSuccessMessage($"Voltage set to {voltage}V");
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

        private void UpdateStatusDisplay()
        {
            VoltageStatusText.Text = $"{VoltageSlider.Value} V";
        }

        private void ShowErrorMessage(string message)
        {
            VoltageStatusText.Text = message;
        }

        private void ShowSuccessMessage(string message)
        {
            VoltageStatusText.Text = message;
        }

        private void VoltageSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            UpdateStatusDisplay();
        }
    }
}
