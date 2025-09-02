namespace LaserControllerApp.Models
{
    public class SystemInfo
    {
        public string? LaserModelNumber { get; set; }
        public string? LaserSerialNumber { get; set; }
        public string? FpgaVersion { get; set; }
        public string? DaqFirmwareVersion { get; set; }
        public string? SystemId { get; set; }
        public string? LcdFirmwareVersion { get; set; }
        public bool LcdPresent { get; set; }
        public bool TestMode { get; set; }

        // Lamp usage statistics
        public long TotalLampShots { get; set; }
        public long TotalSystemShots { get; set; }
        public string? LampHours { get; set; }
    }
}