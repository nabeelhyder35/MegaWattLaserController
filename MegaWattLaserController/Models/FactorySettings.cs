namespace LaserControllerApp.Models
{
    public class FactorySettings
    {
        public int MaxVoltage { get; set; } = 1000;
        public int MinPulseWidth { get; set; } = 25;
        public int MaxPulseWidth { get; set; } = 25000;
        public int MaxFrequency { get; set; } = 2000;
        public int MaxCurrent { get; set; } = 100;
        public int MaxDelay1 { get; set; } = 50;
        public int MaxDelay2 { get; set; } = 100;
        public int SystemCapacitance { get; set; } = 2000; // in pF

        // Additional properties from the documentation
        public int ModelNumber { get; set; }
        public int SerialNumber { get; set; }
        public string? FpgaVersion { get; set; }
        public string? DaqFirmwareVersion { get; set; }
        public string? LcdFirmwareVersion { get; set; }
    }
}