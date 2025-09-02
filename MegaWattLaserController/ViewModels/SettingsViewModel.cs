using CommunityToolkit.Mvvm.ComponentModel;
using LaserControllerApp.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace LaserControllerApp.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly SerialPortManager _serialPortManager;

        // ✅ Inject SerialPortManager via constructor
        public SettingsViewModel(SerialPortManager serialPortManager)
        {
            _serialPortManager = serialPortManager;
        }

        [ObservableProperty] private ObservableCollection<string> _availablePorts = new();
        [ObservableProperty] private string? _selectedPort;
        [ObservableProperty] private int _selectedBaudRate = 9600;

        public void RefreshPorts()
        {
            var ports = _serialPortManager.GetAvailablePorts();
            AvailablePorts = new ObservableCollection<string>(ports.OrderBy(p => p));
            SelectedPort = AvailablePorts.FirstOrDefault();
        }
    }
}
