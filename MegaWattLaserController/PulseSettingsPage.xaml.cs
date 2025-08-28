using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp
{
    public sealed partial class PulseSettingsPage : Page
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;

        public PulseSettingsPage()
        {
            this.InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            ModeComboBox.SelectedIndex = 0;
            RepRateSlider.Value = 10;
            PulseWidthSlider.Value = 100;
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                bool isBurstMode = selectedItem.Tag?.ToString() == "BURST";
                BurstSettingsPanel.Visibility = isBurstMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void RepRateSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            // Optional: Add validation logic
        }

        private void PulseWidthSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            // Optional: Add validation logic
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

                // Build command based on settings
                string command = BuildPulseCommand();

                bool success = await _serialPortManager.SendCommandAsync(command);

                if (success)
                {
                    ShowSuccessMessage("Pulse settings applied successfully");
                }
                else
                {
                    ShowErrorMessage("Failed to apply pulse settings");
                }
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

        private string BuildPulseCommand()
        {
            var selectedMode = (ModeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            int repRate = (int)RepRateSlider.Value;
            int pulseWidth = (int)PulseWidthSlider.Value;

            string command = $"SET_PULSE MODE={selectedMode} RATE={repRate} WIDTH={pulseWidth}";

            if (selectedMode == "BURST")
            {
                if (int.TryParse(BurstCountTextBox.Text, out int burstCount))
                {
                    command += $" BURST={burstCount}";
                }

                string triggerType = InternalTriggerRadio.IsChecked == true ? "INTERNAL" : "EXTERNAL";
                command += $" TRIGGER={triggerType}";
            }

            return command;
        }

        private void ShowErrorMessage(string message)
        {
            // Implement error display
        }

        private void ShowSuccessMessage(string message)
        {
            // Implement success display
        }
    }
}