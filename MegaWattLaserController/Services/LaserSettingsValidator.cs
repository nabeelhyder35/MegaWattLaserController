// File: MegaWattLaserController/Services/LaserSettingsValidator.cs
using System;

namespace LaserControllerApp.Services
{
    public class LaserSettingsValidator
    {
        // Validation ranges based on typical laser specs and KALD.pdf context
        private const double MinVoltage = 50.0;
        private const double MaxVoltage = 300.0;
        private const double MinPulseWidth = 1.0; // in microseconds
        private const double MaxPulseWidth = 1000.0;
        private const double MinFrequency = 1.0; // in Hz
        private const double MaxFrequency = 1000.0;

        public bool ValidateVoltage(double voltage)
        {
            return voltage >= MinVoltage && voltage <= MaxVoltage;
        }

        public bool ValidatePulseWidth(double pulseWidth)
        {
            return pulseWidth >= MinPulseWidth && pulseWidth <= MaxPulseWidth;
        }

        public bool ValidateFrequency(double frequency)
        {
            return frequency >= MinFrequency && frequency <= MaxFrequency;
        }

        public (bool IsValid, string ErrorMessage) ValidateSettings(double voltage, double pulseWidth, double frequency)
        {
            if (!ValidateVoltage(voltage))
                return (false, $"Voltage must be between {MinVoltage} and {MaxVoltage} V.");
            if (!ValidatePulseWidth(pulseWidth))
                return (false, $"Pulse width must be between {MinPulseWidth} and {MaxPulseWidth} µs.");
            if (!ValidateFrequency(frequency))
                return (false, $"Frequency must be between {MinFrequency} and {MaxFrequency} Hz.");
            return (true, string.Empty);
        }
    }
}