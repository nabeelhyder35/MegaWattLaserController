using CommunityToolkit.Mvvm.ComponentModel;
using LaserControllerApp.Models;
using LaserControllerApp.Services;
using System.Threading.Tasks;

namespace LaserControllerApp.ViewModels
{
    public partial class PulseSettingsViewModel : ObservableObject
    {
        // UI-facing doubles
        [ObservableProperty] private double _pulseWidth;  // e.g. ms
        [ObservableProperty] private double _frequency;   // e.g. Hz
        [ObservableProperty] private double _voltage;     // e.g. Volts

        private readonly SerialPortManager _serialPortManager;

        // ✅ Inject SerialPortManager via constructor
        public PulseSettingsViewModel(SerialPortManager serialPortManager)
        {
            _serialPortManager = serialPortManager;
        }

        public void RequestPulseSettings()
        {
            _ = _serialPortManager.SendCommandAsync(new FpgaCommand(FpgaCommandIds.PulseConfig));
        }

        public void UpdateFromData(byte[] data)
        {
            if (data.Length >= 6)
            {
                // Convert FPGA 16-bit integers back to doubles
                ushort rawPulseWidth = (ushort)((data[0] << 8) | data[1]);
                ushort rawFrequency = (ushort)((data[2] << 8) | data[3]);
                ushort rawVoltage = (ushort)((data[4] << 8) | data[5]);

                // Assign to doubles for UI
                PulseWidth = rawPulseWidth;
                Frequency = rawFrequency;
                Voltage = rawVoltage;
            }
        }

        public async Task<bool> SendPulseSettingsAsync()
        {
            // Convert doubles safely into 16-bit unsigned integers
            ushort rawPulseWidth = (ushort)PulseWidth;
            ushort rawFrequency = (ushort)Frequency;
            ushort rawVoltage = (ushort)Voltage;

            var data = new byte[6];
            data[0] = (byte)(rawPulseWidth >> 8);
            data[1] = (byte)(rawPulseWidth & 0xFF);
            data[2] = (byte)(rawFrequency >> 8);
            data[3] = (byte)(rawFrequency & 0xFF);
            data[4] = (byte)(rawVoltage >> 8);
            data[5] = (byte)(rawVoltage & 0xFF);

            var command = new FpgaCommand(FpgaCommandIds.PulseConfig, data);
            await _serialPortManager.SendCommandAsync(command);

            return true;
        }
    }
}
