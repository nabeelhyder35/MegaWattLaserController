using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace LaserControllerApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private ObservableCollection<string> _availablePorts;
        private string? _selectedPort;
        private ObservableCollection<string> _baudRates;
        private string _selectedBaudRate;

        public MainViewModel()
        {
            _availablePorts = new ObservableCollection<string>();
            _baudRates = new ObservableCollection<string> { "9600", "19200", "38400", "57600", "115200" };
            _selectedBaudRate = "9600"; // Default baud rate
        }

        public ObservableCollection<string> AvailablePorts
        {
            get => _availablePorts;
            set => SetProperty(ref _availablePorts, value);
        }

        public string? SelectedPort
        {
            get => _selectedPort;
            set => SetProperty(ref _selectedPort, value);
        }

        public ObservableCollection<string> BaudRates
        {
            get => _baudRates;
            set => SetProperty(ref _baudRates, value);
        }

        public string SelectedBaudRate
        {
            get => _selectedBaudRate;
            set => SetProperty(ref _selectedBaudRate, value);
        }

        // Laser State enum
        public enum LaserState
        {
            Idle,
            Active,
            Fault,
            Standby
        }

        private double _currentEnergy;
        public double CurrentEnergy
        {
            get => _currentEnergy;
            set => SetProperty(ref _currentEnergy, value);
        }

        private LaserState _currentState = LaserState.Idle;
        public LaserState CurrentState
        {
            get => _currentState;
            set => SetProperty(ref _currentState, value);
        }
    }
}
