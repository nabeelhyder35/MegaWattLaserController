using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;

namespace LaserControllerApp.Views
{
    public sealed partial class VoltagePage : Page
    {
        public VoltagePage()
        {
            this.InitializeComponent();
        }

        private void PresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string voltageString)
            {
                if (int.TryParse(voltageString, out int voltage))
                {
                    VoltageSlider.Value = voltage;
                    VoltageTextBox.Text = voltage.ToString();
                }
            }
        }

        private void ApplyVoltageButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement voltage application logic here
            VoltageOutputStatus.Text = "Voltage Output: Enabled";
            VoltageStatusIndicator.Fill = new SolidColorBrush(Microsoft.UI.Colors.Green);
        }

        private void DisableVoltageButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement voltage disable logic here
            VoltageOutputStatus.Text = "Voltage Output: Disabled";
            VoltageStatusIndicator.Fill = new SolidColorBrush(Microsoft.UI.Colors.Red);
            VoltageSlider.Value = 0;
            VoltageTextBox.Text = "0";
        }

        private void VoltageSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (VoltageTextBox != null)
            {
                VoltageTextBox.Text = ((int)e.NewValue).ToString();
            }
        }

        private void VoltageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VoltageTextBox != null && int.TryParse(VoltageTextBox.Text, out int value))
            {
                if (value >= VoltageSlider.Minimum && value <= VoltageSlider.Maximum)
                {
                    VoltageSlider.Value = value;
                }
                else if (value > VoltageSlider.Maximum)
                {
                    VoltageTextBox.Text = VoltageSlider.Maximum.ToString();
                    VoltageSlider.Value = VoltageSlider.Maximum;
                }
            }
        }
    }
}