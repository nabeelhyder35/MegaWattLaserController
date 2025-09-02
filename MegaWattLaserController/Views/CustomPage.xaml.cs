using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using LaserControllerApp.Models;
using System;

namespace LaserControllerApp.Views
{
    public sealed partial class CustomPage : Page
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;

        public CustomPage()
        {
            this.InitializeComponent();
            // Assuming LogMessages is an ObservableCollection<string> in SerialPortManager
            if (_serialPortManager.LogMessages != null && LogListView != null)
            {
                LogListView.ItemsSource = _serialPortManager.LogMessages;
            }
        }

        private async void SendCommand_Click(object sender, RoutedEventArgs e)
        {
            if (CommandTextBox != null)
            {
                string input = CommandTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(input))
                {
                    // Parse input to create an FpgaCommand
                    FpgaCommand command = ParseInputToFpgaCommand(input);
                    if (command != null)
                    {
                        await _serialPortManager.SendCommandAsync(command);
                        CommandTextBox.Text = string.Empty;
                    }
                    else
                    {
                        // Optionally log invalid command
                        _serialPortManager.LogMessages?.Add($"Invalid command: {input}");
                    }
                }
            }
        }

        private FpgaCommand ParseInputToFpgaCommand(string input)
        {
            // Example parsing logic; adjust based on expected input format
            try
            {
                // Assume input format: "commandId:payload" (e.g., "SetVoltage:300")
                string[] parts = input.Split(':');
                if (parts.Length < 1)
                    return null;

                ushort commandId = parts[0] switch
                {
                    "SetVoltage" => FpgaCommandIds.SetVoltage,
                    "SetFrequency" => FpgaCommandIds.SetFrequency,
                    "SetPulseWidth" => FpgaCommandIds.SetPulseWidth,
                    "SetShutterState" => FpgaCommandIds.SetShutterState,
                    _ => throw new ArgumentException("Unknown command")
                };

                byte[] data = Array.Empty<byte>();
                if (parts.Length > 1)
                {
                    if (commandId == FpgaCommandIds.SetShutterState)
                    {
                        data = new byte[] { byte.Parse(parts[1]) }; // e.g., 0 for Closed, 1 for Open
                    }
                    else
                    {
                        data = BitConverter.GetBytes(int.Parse(parts[1])); // e.g., Voltage or Frequency value
                    }
                }

                return new FpgaCommand(commandId, data);
            }
            catch (Exception ex)
            {
                _serialPortManager.LogMessages?.Add($"Error parsing command: {ex.Message}");
                return null;
            }
        }
    }
}