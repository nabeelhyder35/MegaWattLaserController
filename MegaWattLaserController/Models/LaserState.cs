using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LaserControllerApp.Models
{
    public class LaserState : INotifyPropertyChanged
    {
        private double _energy;
        private double _power;
        private double _temperature;
        private bool _isRunning;
        private bool _isShutterOpen;

        public double Energy
        {
            get => _energy;
            set => SetField(ref _energy, value);
        }

        public double Power
        {
            get => _power;
            set => SetField(ref _power, value);
        }

        public double Temperature
        {
            get => _temperature;
            set => SetField(ref _temperature, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetField(ref _isRunning, value);
        }

        public bool IsShutterOpen
        {
            get => _isShutterOpen;
            set => SetField(ref _isShutterOpen, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
