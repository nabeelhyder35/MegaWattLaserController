using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LaserControllerApp.Models
{
    public partial class LaserSettings : INotifyPropertyChanged
    {
        private int _voltage = 300;
        private int _frequency = 100;
        private int _pulseWidth = 100;
        private int _delay1 = 50;
        private int _delay2 = 100;
        private TriggerMode _triggerMode = TriggerMode.External;
        private FireMode _fireMode = FireMode.Continuous;
        private long _shotCount;
        private ShutterMode _shutterMode = ShutterMode.Manual;
        private ShutterState _shutterState = ShutterState.Closed;
        private SoftStartMode _softStartMode = SoftStartMode.Off;
        private int _idleVoltage = 50;
        private long _rampCount;

        public int Voltage
        {
            get => _voltage;
            set => SetField(ref _voltage, ValidateVoltage(value));
        }

        public int Frequency
        {
            get => _frequency;
            set => SetField(ref _frequency, ValidateFrequency(value));
        }

        public int PulseWidth
        {
            get => _pulseWidth;
            set => SetField(ref _pulseWidth, ValidatePulseWidth(value));
        }

        public int Delay1
        {
            get => _delay1;
            set => SetField(ref _delay1, ValidateDelay(value, 50));
        }

        public int Delay2
        {
            get => _delay2;
            set => SetField(ref _delay2, ValidateDelay(value, 100));
        }

        public TriggerMode TriggerMode { get => _triggerMode; set => SetField(ref _triggerMode, value); }
        public FireMode FireMode { get => _fireMode; set => SetField(ref _fireMode, value); }
        public long ShotCount { get => _shotCount; set => SetField(ref _shotCount, value); }
        public ShutterMode ShutterMode { get => _shutterMode; set => SetField(ref _shutterMode, value); }
        public ShutterState ShutterState { get => _shutterState; set => SetField(ref _shutterState, value); }
        public SoftStartMode SoftStartMode { get => _softStartMode; set => SetField(ref _softStartMode, value); }
        public int IdleVoltage { get => _idleVoltage; set => SetField(ref _idleVoltage, ValidateIdleVoltage(value)); }
        public long RampCount { get => _rampCount; set => SetField(ref _rampCount, value); }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private int ValidateVoltage(int value)
        {
            return Math.Clamp(value, 0, 1000); // Factory max voltage
        }

        private int ValidateFrequency(int value)
        {
            return Math.Clamp(value, 0, 2000); // Factory max frequency
        }

        private int ValidatePulseWidth(int value)
        {
            return Math.Clamp(value, 25, 25000); // Factory min/max pulse width
        }

        private int ValidateDelay(int value, int maxValue)
        {
            return Math.Clamp(value, 0, maxValue);
        }

        private int ValidateIdleVoltage(int value)
        {
            return Math.Clamp(value, 0, Voltage); // Idle voltage can't exceed main voltage
        }
    }
}