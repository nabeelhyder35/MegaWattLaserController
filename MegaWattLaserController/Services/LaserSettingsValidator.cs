using LaserControllerApp.Models;

namespace LaserControllerApp.Services
{
    public static class LaserSettingsValidator
    {
        public static (bool isValid, string errorMessage) ValidateVoltage(double voltage, FactorySettings factorySettings)
        {
            if (voltage < 0)
                return (false, "Voltage cannot be negative");

            if (voltage > factorySettings.MaxVoltage)
                return (false, $"Voltage exceeds maximum limit of {factorySettings.MaxVoltage}V");

            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateFrequency(double frequency)
        {
            if (frequency < 1)
                return (false, "Frequency must be at least 1 Hz");

            if (frequency > 2000)
                return (false, "Frequency cannot exceed 2000 Hz");

            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidatePulseWidth(double pulseWidth)
        {
            if (pulseWidth < 25)
                return (false, "Pulse width must be at least 25 μs");

            if (pulseWidth > 25000)
                return (false, "Pulse width cannot exceed 25000 μs");

            return (true, string.Empty);
        }
    }
}