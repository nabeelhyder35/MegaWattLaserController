using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LaserControllerApp
{
    public sealed partial class ShutterPage : Page
    {
        private readonly SerialPortManager serialPortManager = SerialPortManager.Instance;

        public ShutterPage()
        {
            this.InitializeComponent();
        }

        private void OpenShutter_Click(object sender, RoutedEventArgs e)
        {
            serialPortManager.SendCommand("SHUTTER_OPEN");
        }

        private void CloseShutter_Click(object sender, RoutedEventArgs e)
        {
            serialPortManager.SendCommand("SHUTTER_CLOSE");
        }
    }
}