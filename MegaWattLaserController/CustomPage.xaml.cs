using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using System;

namespace LaserControllerApp
{
    public sealed partial class CustomPage : Page
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;

        public CustomPage()
        {
            this.InitializeComponent();
            LogListView.ItemsSource = _serialPortManager.LogMessages;
        }

        private async void SendCommand_Click(object sender, RoutedEventArgs e)
        {
            string command = CommandTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(command))
            {
                await _serialPortManager.SendCommandAsync(command);
                CommandTextBox.Text = string.Empty;
            }
        }
    }
}