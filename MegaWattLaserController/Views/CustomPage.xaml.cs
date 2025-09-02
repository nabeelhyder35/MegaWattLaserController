using LaserControllerApp.Models;
using LaserControllerApp.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace LaserControllerApp.Views
{
    public sealed partial class CustomPage : Page
    {
        private readonly SerialPortManager _serialPortManager;

        // ✅ Inject SerialPortManager via constructor
        public CustomPage(SerialPortManager serialPortManager)
        {
            this.InitializeComponent();
            _serialPortManager = serialPortManager;

            // Bind log messages to the ListView
            LogListView.ItemsSource = _serialPortManager.LogMessages;
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ushort.TryParse(CommandIdTextBox.Text, out ushort cmdId))
                {
                    _serialPortManager.LogMessages.Add("Invalid Command ID");
                    return;
                }

                byte[] payload = Array.Empty<byte>();
                if (!string.IsNullOrWhiteSpace(DataTextBox.Text))
                {
                    try
                    {
                        payload = ParseHexString(DataTextBox.Text);
                    }
                    catch
                    {
                        _serialPortManager.LogMessages.Add("Invalid data format. Use hex bytes like: 01 0A FF");
                        return;
                    }
                }

                var command = new FpgaCommand(cmdId, payload);
                await _serialPortManager.SendCommandAsync(command);

                _serialPortManager.LogMessages.Add($"Sent: Command=0x{command.Command:X4}, Data={BitConverter.ToString(payload)}");
            }
            catch (Exception ex)
            {
                _serialPortManager.LogMessages.Add($"Error sending command: {ex.Message}");
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            _serialPortManager.LogMessages.Clear();
        }

        private static byte[] ParseHexString(string hexString)
        {
            string[] parts = hexString.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] bytes = new byte[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                bytes[i] = Convert.ToByte(parts[i], 16);
            }

            return bytes;
        }
    }
}
