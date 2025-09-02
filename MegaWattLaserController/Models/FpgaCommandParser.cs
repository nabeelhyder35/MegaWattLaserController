using System;

namespace LaserControllerApp.Models
{
    public static class FpgaCommandParser
    {
        public static float ParseEnergy(byte[] data)
        {
            if (data.Length >= 4)
            {
                return BitConverter.ToSingle(data, 0);
            }
            return 0.0f;
        }

        public static float ParseTemperature(byte[] data)
        {
            if (data.Length >= 4)
            {
                return BitConverter.ToSingle(data, 0);
            }
            return 0.0f;
        }

        public static SystemInfo ParseSystemInfo(byte[] data)
        {
            // Placeholder implementation for parsing system info
            // Replace with actual parsing logic based on your protocol
            return new SystemInfo
            {
                LaserModelNumber = "Model XYZ",
                LaserSerialNumber = "12345",
                FpgaVersion = "1.0",
                DaqFirmwareVersion = "2.0",
                SystemId = "ID-ABC",
                LcdFirmwareVersion = "1.1",
                TestMode = false,
                LampHours = "123.45",
                TotalLampShots = 5678,
                TotalSystemShots = 9012
            };
        }

        public static FactorySettings ParseFactorySettings(byte[] data)
        {
            // Placeholder implementation for parsing factory settings
            // Replace with actual parsing logic based on your protocol
            return new FactorySettings
            {
                MaxVoltage = 1000,
                MaxFrequency = 2000,
                MinPulseWidth = 25,
                MaxPulseWidth = 25000,
                MaxCurrent = 100,
                MaxDelay1 = 50,
                MaxDelay2 = 100,
                SystemCapacitance = 2000
            };
        }
    }
}