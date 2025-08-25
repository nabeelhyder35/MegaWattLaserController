using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LaserControllerApp
{
    public sealed partial class CustomPage : Page
    {
        private readonly SerialPortManager serialPortManager = SerialPortManager.Instance;

        public CustomPage()
        {
            this.InitializeComponent();
            LogListView.ItemsSource = serialPortManager.GetLogMessages();
        }

        private void SendCommand_Click(object sender, RoutedEventArgs e)
        {
            string command = CommandTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(command))
            {
                serialPortManager.SendCommand(command);
                CommandTextBox.Text = string.Empty;
            }
        }
    }
}