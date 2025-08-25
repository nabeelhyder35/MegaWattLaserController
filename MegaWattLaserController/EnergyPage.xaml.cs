using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LaserControllerApp
{
    public sealed partial class EnergyPage : Page
    {
        private readonly SerialPortManager serialPortManager = SerialPortManager.Instance;

        public EnergyPage()
        {
            this.InitializeComponent();
        }

        private void SetVoltage_Click(object sender, RoutedEventArgs e)
        {
            int voltage = (int)VoltageSlider.Value;
            string command = $"SET_VOLTAGE {voltage}\n";
            serialPortManager.SendCommand(command);
            VoltageStatus.Text = $"Current Voltage: {voltage}V";
        }
    }
}