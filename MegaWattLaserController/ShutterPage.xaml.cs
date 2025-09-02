using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using System.Threading.Tasks;

namespace LaserControllerApp
{
    public sealed partial class ShutterPage : Page
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;

        public ShutterPage()
        {
            this.InitializeComponent();
        }

        private async void OpenShutter_Click(object sender, RoutedEventArgs e)
        {
            await _serialPortManager.SendCommandAsync("SHUTTER_OPEN");
        }

        private async void CloseShutter_Click(object sender, RoutedEventArgs e)
        {
            await _serialPortManager.SendCommandAsync("SHUTTER_CLOSE");
        }
    }
}