using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp
{
    public sealed partial class EnergyPage : Page
    {
        private readonly SerialPortManager _serialPortManager;

        public EnergyPage()
        {
            this.InitializeComponent();

            // Use the singleton instance directly
            _serialPortManager = SerialPortManager.Instance;

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
                string command = $"SET_VOLTAGE {voltage}";
                bool success = await _serialPortManager.SendCommandAsync(command);

                if (success)
                {
                    UpdateStatusDisplay();
                    ShowSuccessMessage($"Voltage set to {voltage}V");
                }
                else
                {
                    ShowErrorMessage("Failed to set voltage");
                }
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