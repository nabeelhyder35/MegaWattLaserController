using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaserControllerApp.Models
{
    public static class FpgaCommandIds
    {
        // NOTE: Rx = FPGA Receives (from LCD), Tx = FPGA Transmits (to LCD)
        // This aligns with the 'klad_lcd_fpga_cmds.xlsx' convention.

        // --------------------------------------------------------------------
        // Rx Commands (Sent from LCD to FPGA)
        // --------------------------------------------------------------------

        /// <summary> (Rx) Checksum mismatch on received command. </summary>
        public const ushort lcdRxBadCmd = 0xFFFE; // -2
        /// <summary> (Rx) No command received yet. </summary>
        public const ushort lcdRxNoCmd = 0xFFFF; // -1
        /// <summary> (Rx) LCD reported FPGA sent a bad command. </summary>
        public const ushort lcdFPGABadCmd = 0;
        /// <summary> (Rx) Laser Pulse Configuration (Freq, Width, Shots, Delays, Mode). Data Length: 14 </summary>
        public const ushort lcdRxLsrPulseConfig = 1;
        /// <summary> (Rx) Change Laser State (e.g., Arm, Disarm, Run, Pause). Data Length: 1 </summary>
        public const ushort lcdRxLsrState = 2;
        /// <summary> (Rx) Change Interlock Mask. Data Length: 1 </summary>
        public const ushort lcdRxIntMask = 3;
        /// <summary> (Rx) Enable/Disable Waveform Messaging. Data Length: 1 </summary>
        public const ushort lcdRxWaveState = 4;
        /// <summary> (Rx) Laser Fire Setup (Delays). Data Length: 4 </summary>
        public const ushort lcdRxLsrDelays = 5;
        /// <summary> (Rx) Laser Calibration (Cap Voltage Range). Data Length: 2 </summary>
        public const ushort lcdRxLsrCal = 6;
        /// <summary> (Rx) Laser voltage setting. Data Length: 2 </summary>
        public const ushort lcdRxLsrVolts = 7;
        /// <summary> (Rx) Cancel a (dis)charge. Data Length: 1 </summary>
        public const ushort lcdRxLsrChargeCancel = 8;
        /// <summary> (Rx) Shutter State Config (Mode, State). Data Length: 2 </summary>
        public const ushort lcdRxShutterConfig = 9;
        /// <summary> (Rx) Soft Start Configuration (Enable, Idle Voltage, Ramp Count). Data Length: 7 </summary>
        public const ushort lcdRxSoftStartConfig = 10;
        /// <summary> (Rx) Command to request energy reading. Data Length: ? </summary>
        public const ushort lcdRxReadEnergy = 11;
        /// <summary> (Rx) Command to request temperature reading. Data Length: ? </summary>
        public const ushort lcdRxReadTemperature = 12;
        /// <summary> (Rx) Command to request system information. Data Length: ? </summary>
        public const ushort lcdRxSystemInfo = 18;
        /// <summary> (Rx) Command to request factory settings. Data Length: ? </summary>
        public const ushort lcdRxFactorySettings = 20;
        /// <summary> (Rx) Command to request interlock status. Data Length: ? </summary>
        public const ushort lcdRxInterlockStatus = 22;
        /// <summary> (Rx) Command to request shot count. Data Length: ? </summary>
        public const ushort lcdRxShotCount = 24;
        /// <summary> (Rx) Command to request lamp hours. Data Length: ? </summary>
        public const ushort lcdRxLampHours = 26;
        /// <summary> (Rx) Command to request capacitor voltage. Data Length: ? </summary>
        public const ushort lcdRxCapacitorVoltage = 28;
        /// <summary> (Rx) Command to request error status. Data Length: ? </summary>
        public const ushort lcdRxErrorStatus = 30;
        /// <summary> (Rx) Command to reset the system. Data Length: ? </summary>
        public const ushort lcdRxReset = 31;
        /// <summary> (Rx) Command to send a password. Data Length: ? </summary>
        public const ushort lcdRxPassword = 33;
        /// <summary> (Rx) Command to initiate firmware update. Data Length: ? </summary>
        public const ushort lcdRxFirmwareUpdate = 35;
        /// <summary> (Rx) Command to enter test mode. Data Length: ? </summary>
        public const ushort lcdRxTestMode = 37;
        /// <summary> (Rx) Command to initiate calibration. Data Length: ? </summary>
        public const ushort lcdRxCalibration = 39;
        /// <summary> (Rx) Command to run diagnostics. Data Length: ? </summary>
        public const ushort lcdRxDiagnostic = 41;
        /// <summary> (Rx) FPGA sends discovery request to UI. Data Length: ? </summary>
        public const ushort lcdRxDiscoverUI = 50;

        // --------------------------------------------------------------------
        // Tx Commands (Sent from FPGA to LCD)
        // --------------------------------------------------------------------

        /// <summary> (Tx) Response to lcdTestCmd. Data Length: variable </summary>
        public const ushort lcdTxTestResp = 0;
        /// <summary> (Tx) Tell LCD a bad cmd was received. Data Length: same as received cmd </summary>
        public const ushort lcdTxBadCmd = 1;
        /// <summary> (Tx) Laser State (e.g., Armed, Disarmed, Running). Data Length: 1 </summary>
        public const ushort lcdTxLsrState = 2;
        /// <summary> (Tx) Laser Pulse Configuration (Echo/Status). Data Length: 14 </summary>
        public const ushort lcdTxLsrPulseConfig = 3;
        /// <summary> (Tx) Shot Count. Data Length: 4 </summary>
        public const ushort lcdTxLsrCount = 4;
        /// <summary> (Tx) Laser Run Status (Count, State, Energy, Power, etc.). Data Length: 14 </summary>
        public const ushort lcdTxLsrRunStatus = 5;
        /// <summary> (Tx) Interlock Status. Data Length: 1 </summary>
        public const ushort lcdTxLsrIntStatus = 6;
        /// <summary> (Tx) Interlock Mask. Data Length: 1 </summary>
        public const ushort lcdTxLsrIntMask = 7;
        /// <summary> (Tx) Waveform Data (32 samples @ 2 bytes each). Data Length: 64 </summary>
        public const ushort lcdTxLsrWaveform = 8;
        /// <summary> (Tx) Discovery Message / Laser State. Data Length: 1 </summary>
        public const ushort lcdTxDiscovery = 9;
        /// <summary> (Tx) Laser voltage setting (Echo/Status). Data Length: 2 </summary>
        public const ushort lcdTxLsrVolts = 10;
        /// <summary> (Tx) Capacitor Voltage and Charging State. Data Length: 3 </summary>
        public const ushort lcdTxLsrChargeState = 11;
        /// <summary> (Tx) Charge Volts (Setpoint?). Data Length: 2 </summary>
        public const ushort lcdTxLsrChargeVolts = 12;
        /// <summary> (Tx) Shutter Configuration (Echo/Status). Data Length: 2 </summary>
        public const ushort lcdTxShutterConfig = 13;
        /// <summary> (Tx) Soft Start Configuration (Echo/Status). Data Length: 7 </summary>
        public const ushort lcdTxSoftStartConfig = 14;
        /// <summary> (Tx) Response with energy value. Data Length: ? </summary>
        public const ushort lcdTxEnergyValue = 15;
        /// <summary> (Tx) Response with temperature value. Data Length: ? </summary>
        public const ushort lcdTxTemperatureValue = 16;
        /// <summary> (Tx) Response with system information. Data Length: ? </summary>
        public const ushort lcdTxSystemInfo = 17;
        /// <summary> (Tx) Response with factory settings. Data Length: ? </summary>
        public const ushort lcdTxFactorySettings = 19;
        /// <summary> (Tx) Response with interlock status. Data Length: ? </summary>
        public const ushort lcdTxInterlockStatus = 21;
        /// <summary> (Tx) Response with shot count. Data Length: ? </summary>
        public const ushort lcdTxShotCount = 23;
        /// <summary> (Tx) Response with lamp hours. Data Length: ? </summary>
        public const ushort lcdTxLampHours = 25;
        /// <summary> (Tx) Response with capacitor voltage. Data Length: ? </summary>
        public const ushort lcdTxCapacitorVoltage = 27;
        /// <summary> (Tx) Response with error status. Data Length: ? </summary>
        public const ushort lcdTxErrorStatus = 29;
        /// <summary> (Tx) Acknowledge reset command. Data Length: ? </summary>
        public const ushort lcdTxResetAck = 32;
        /// <summary> (Tx) Response with password verification status. Data Length: ? </summary>
        public const ushort lcdTxPasswordStatus = 34;
        /// <summary> (Tx) Response with firmware update status. Data Length: ? </summary>
        public const ushort lcdTxFirmwareStatus = 36;
        /// <summary> (Tx) Response with test mode status. Data Length: ? </summary>
        public const ushort lcdTxTestModeStatus = 38;
        /// <summary> (Tx) Response with calibration data. Data Length: ? </summary>
        public const ushort lcdTxCalibrationData = 40;
        /// <summary> (Tx) Response with diagnostic data. Data Length: ? </summary>
        public const ushort lcdTxDiagnosticData = 42;
        /// <summary> (Tx) UI responds to FPGA discovery request. Data Length: ? </summary>
        public const ushort lcdTxDiscoveryResponse = 51;

        // --------------------------------------------------------------------
        // Aliases for easier use (Direction is from LCD's perspective: Set, Get)
        // --------------------------------------------------------------------
        /// <summary> (Rx) Alias for lcdRxLsrVolts. </summary>
        public const ushort SetVoltage = lcdRxLsrVolts;
        /// <summary> (Rx) Part of lcdRxLsrPulseConfig. </summary>
        public const ushort SetFrequency = lcdRxLsrPulseConfig;
        /// <summary> (Rx) Part of lcdRxLsrPulseConfig. </summary>
        public const ushort SetPulseWidth = lcdRxLsrPulseConfig;
        /// <summary> (Rx) Alias for lcdRxShutterConfig. </summary>
        public const ushort SetShutterState = lcdRxShutterConfig;
        /// <summary> (Tx) Alias for lcdTxLsrState. </summary>
        public const ushort LaserState = lcdTxLsrState;
        /// <summary> (Tx/Rx) Alias for config command/response. </summary>
        public const ushort PulseConfig = lcdRxLsrPulseConfig;
        /// <summary> (Tx/Rx) Alias for config command/response. </summary>
        public const ushort ShutterConfig = lcdRxShutterConfig;
        /// <summary> (Tx) Alias for lcdTxLsrIntStatus. </summary>
        public const ushort GetInterlockStatus = lcdTxLsrIntStatus;
        /// <summary> (Tx) Alias for lcdTxSystemInfo. </summary>
        public const ushort GetSystemInfo = lcdTxSystemInfo;
        /// <summary> (Tx) Alias for lcdTxFactorySettings. </summary>
        public const ushort GetFactorySettings = lcdTxFactorySettings;
        /// <summary> (Tx) Alias for lcdTxEnergyValue. </summary>
        public const ushort GetEnergyReading = lcdTxEnergyValue;
        /// <summary> (Tx) Alias for lcdTxTemperatureValue. </summary>
        public const ushort GetTemperatureReading = lcdTxTemperatureValue;
        /// <summary> (Tx) Alias for lcdTxShotCount. </summary>
        public const ushort GetShotCount = lcdTxShotCount;
        /// <summary> (Tx) Alias for lcdTxLampHours. </summary>
        public const ushort GetLampHours = lcdTxLampHours;
        /// <summary> (Tx) Alias for lcdTxCapacitorVoltage. </summary>
        public const ushort GetCapacitorVoltage = lcdTxCapacitorVoltage;
        /// <summary> (Tx) Alias for lcdTxErrorStatus. </summary>
        public const ushort GetErrorStatus = lcdTxErrorStatus;
        /// <summary> (Rx) Alias for lcdRxReset. </summary>
        public const ushort SystemReset = lcdRxReset;
    }
}