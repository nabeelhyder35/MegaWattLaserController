namespace LaserControllerApp.Models
{
    public static class FpgaCommandIds
    {
        // Rx Commands (from FPGA to LCD)
        public const ushort lcdRxBadCmd = 0xFFFE; // -2
        public const ushort lcdRxNoCmd = 0xFFFF; // -1
        public const ushort lcdFPGABadCmd = 0;
        public const ushort lcdRxLsrPulseConfig = 1;
        public const ushort lcdRxLsrState = 2;
        public const ushort lcdRxIntMask = 3;
        public const ushort lcdRxWaveState = 4;
        public const ushort lcdRxLsrDelays = 5;
        public const ushort lcdRxLsrCal = 6;
        public const ushort lcdRxLsrVolts = 7;
        public const ushort lcdRxLsrChargeCancel = 8;
        public const ushort lcdRxShutterConfig = 9;
        public const ushort lcdRxSoftStartConfig = 10;
        public const ushort lcdRxEnergyValue = 11;      // Response with energy value
        public const ushort lcdRxReadTemperature = 12;  // Response with temperature value

        // Tx Commands (from LCD to FPGA)
        public const ushort lcdTxTestResp = 0;
        public const ushort lcdTxBadCmd = 1;
        public const ushort lcdTxLsrState = 2;
        public const ushort lcdTxLsrPulseConfig = 3;
        public const ushort lcdTxLsrCount = 4;
        public const ushort lcdTxLsrRunStatus = 5;
        public const ushort lcdTxLsrIntStatus = 6;
        public const ushort lcdTxLsrIntMask = 7;
        public const ushort lcdTxLsrWaveform = 8;
        public const ushort lcdTxDiscovery = 9;
        public const ushort lcdTxLsrVolts = 10;
        public const ushort lcdTxLsrChargeState = 11;
        public const ushort lcdTxLsrChargeVolts = 12;
        public const ushort lcdTxShutterConfig = 13;
        public const ushort lcdTxSoftStartConfig = 14;
        public const ushort lcdTxReadEnergy = 15;       // Command to request energy reading
        public const ushort lcdTxReadTemperature = 16;  // Command to request temperature reading

        // System Info Commands
        public const ushort lcdTxSystemInfo = 17;
        public const ushort lcdRxSystemInfo = 18;

        // Factory Settings Commands
        public const ushort lcdTxFactorySettings = 19;
        public const ushort lcdRxFactorySettings = 20;

        // Interlock Commands
        public const ushort lcdTxInterlockStatus = 21;
        public const ushort lcdRxInterlockStatus = 22;

        // Shot Count Commands
        public const ushort lcdTxShotCount = 23;
        public const ushort lcdRxShotCount = 24;

        // Lamp Hours Commands
        public const ushort lcdTxLampHours = 25;
        public const ushort lcdRxLampHours = 26;

        // Capacitor Voltage Commands
        public const ushort lcdTxCapacitorVoltage = 27;
        public const ushort lcdRxCapacitorVoltage = 28;

        // Error Status Commands
        public const ushort lcdTxErrorStatus = 29;
        public const ushort lcdRxErrorStatus = 30;

        // Reset Commands
        public const ushort lcdTxReset = 31;
        public const ushort lcdRxResetAck = 32;

        // Password/Protection Commands
        public const ushort lcdTxPassword = 33;
        public const ushort lcdRxPasswordStatus = 34;

        // Firmware Update Commands
        public const ushort lcdTxFirmwareUpdate = 35;
        public const ushort lcdRxFirmwareStatus = 36;

        // Test Mode Commands
        public const ushort lcdTxTestMode = 37;
        public const ushort lcdRxTestModeStatus = 38;

        // Calibration Commands
        public const ushort lcdTxCalibration = 39;
        public const ushort lcdRxCalibrationData = 40;

        // Diagnostic Commands
        public const ushort lcdTxDiagnostic = 41;
        public const ushort lcdRxDiagnosticData = 42;

        // Aliases for easier use
        public const ushort SetVoltage = lcdTxLsrVolts;
        public const ushort SetFrequency = lcdTxLsrPulseConfig;
        public const ushort SetPulseWidth = lcdTxLsrPulseConfig;
        public const ushort SetShutterState = lcdTxShutterConfig;
        public const ushort LaserState = lcdTxLsrState;
        public const ushort PulseConfig = lcdTxLsrPulseConfig;
        public const ushort ShutterConfig = lcdTxShutterConfig;
        public const ushort GetInterlockStatus = lcdTxLsrIntStatus;
        public const ushort GetSystemInfo = lcdTxSystemInfo;
        public const ushort GetFactorySettings = lcdTxFactorySettings;
        public const ushort GetEnergyReading = lcdTxReadEnergy;
        public const ushort GetTemperatureReading = lcdTxReadTemperature;
        public const ushort GetShotCount = lcdTxShotCount;
        public const ushort GetLampHours = lcdTxLampHours;
        public const ushort GetCapacitorVoltage = lcdTxCapacitorVoltage;
        public const ushort GetErrorStatus = lcdTxErrorStatus;
        public const ushort SystemReset = lcdTxReset;
    }
}