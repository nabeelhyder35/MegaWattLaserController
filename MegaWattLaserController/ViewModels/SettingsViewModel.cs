using CommunityToolkit.Mvvm.ComponentModel;
using LaserControllerApp.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace LaserControllerApp.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly SerialPortManager _serialPortManager;

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
            var ordered = ports.OrderBy(p => p).ToList();
            AvailablePorts = new ObservableCollection<string>(ordered);

            // Preserve previous selection if still present, else select first
            if (SelectedPort is null || !ordered.Contains(SelectedPort))
            {
                SelectedPort = AvailablePorts.FirstOrDefault();
            }
        }
    }
}
