using CommunityToolkit.Mvvm.ComponentModel;
using LaserControllerApp.Models;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading.Tasks;

namespace LaserControllerApp.Services
{
    public interface ICommandResponseHandler
    {
        event EventHandler<FpgaCommand> CommandResponseReceived;
    }

    public partial class SerialPortManager : ObservableObject, IDisposable, ICommandResponseHandler
    {
        private SerialPort _serialPort;
        private byte[] _receiveBuffer = new byte[4096];
        private int _bufferIndex = 0;
        private DispatcherQueue? _dispatcherQueue;

        public event EventHandler<bool>? ConnectionStatusChanged;
        public event EventHandler<FpgaCommand>? DataReceived;
        public event EventHandler<FpgaCommand>? CommandResponseReceived;
        public event EventHandler<string>? ErrorOccurred;
        public event EventHandler<string>? LogMessageAdded;

        [ObservableProperty] private bool _isConnected;
        [ObservableProperty] private string? _portName;
        [ObservableProperty] private ObservableCollection<string> _logMessages = new();
        [ObservableProperty] private int _bytesReceived;
        [ObservableProperty] private int _bytesSent;
        [ObservableProperty] private int _packetsReceived;
        [ObservableProperty] private int _packetsSent;
        [ObservableProperty] private int _checksumErrors;

        // Protocol constants
        private const byte FPGA_START = 0x2A;
        private const byte FPGA_END = 0x3A;

        public SerialPortManager()
        {
            _serialPort = new SerialPort();
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Initialize(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

        #region Connection Management

        public async Task<bool> ConnectAsync(string portName, int baudRate = 9600)
        {
            if (_serialPort.IsOpen) await DisconnectAsync();

            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            try
            {
                await Task.Run(() => _serialPort.Open());
                IsConnected = true;
                PortName = portName;

                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Connected to {portName} at {baudRate} baud");
                    LogMessageAdded?.Invoke(this, $"Connected to {portName} at {baudRate} baud");
                    ConnectionStatusChanged?.Invoke(this, true);
                });

                return true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Connection error: {ex.Message}");
                    LogMessageAdded?.Invoke(this, $"Connection error: {ex.Message}");
                    ErrorOccurred?.Invoke(this, $"Connection error: {ex.Message}");
                });
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    await Task.Run(() => _serialPort.Close());
                    IsConnected = false;
                    _dispatcherQueue?.TryEnqueue(() =>
                    {
                        LogMessages.Add("Disconnected from serial port");
                        LogMessageAdded?.Invoke(this, "Disconnected from serial port");
                        ConnectionStatusChanged?.Invoke(this, false);
                    });
                }
            }
            catch (Exception ex)
            {
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Disconnection error: {ex.Message}");
                    LogMessageAdded?.Invoke(this, $"Disconnection error: {ex.Message}");
                    ErrorOccurred?.Invoke(this, $"Disconnection error: {ex.Message}");
                });
            }
        }

        #endregion

        #region Command Sending

        public async Task<bool> SendCommandAsync(FpgaCommand command)
        {
            if (!_serialPort.IsOpen)
            {
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add("Cannot send command: Serial port is not open");
                    LogMessageAdded?.Invoke(this, "Cannot send command: Serial port is not open");
                    ErrorOccurred?.Invoke(this, "Serial port is not open");
                });
                return false;
            }

            try
            {
                byte[] packet = BuildCommandPacket(command);
                await Task.Run(() => _serialPort.Write(packet, 0, packet.Length));

                BytesSent += packet.Length;
                PacketsSent++;
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Sent command: {command.Command:X4}, Length: {command.Data.Length}");
                    LogMessageAdded?.Invoke(this, $"Sent command: {command.Command:X4}, Length: {command.Data.Length}");
                });

                return true;
            }
            catch (Exception ex)
            {
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Send error: {ex.Message}");
                    LogMessageAdded?.Invoke(this, $"Send error: {ex.Message}");
                    ErrorOccurred?.Invoke(this, $"Send error: {ex.Message}");
                });
                return false;
            }
        }

        private byte[] BuildCommandPacket(FpgaCommand command)
        {
            ushort dataLength = (ushort)(command.Data?.Length ?? 0);
            int totalLength = 6 + dataLength;
            byte[] packet = new byte[totalLength];
            int index = 0;

            packet[index++] = FPGA_START;
            packet[index++] = (byte)((dataLength >> 8) & 0xFF);
            packet[index++] = (byte)(dataLength & 0xFF);
            packet[index++] = (byte)command.Command;

            if (dataLength > 0) Array.Copy(command.Data, 0, packet, index, dataLength);
            index += dataLength;

            packet[index++] = CalculateChecksum((byte)command.Command, command.Data);
            packet[index] = FPGA_END;

            return packet;
        }

        private byte CalculateChecksum(byte commandId, byte[]? data)
        {
            int sum = commandId;
            if (data != null) foreach (byte b in data) sum += b;
            return (byte)((0 - sum) & 0xFF);
        }

        #endregion

        #region Data Receiving

        private void SerialPort_DataReceived(object? sender, SerialDataReceivedEventArgs e)
        {
            if (!_serialPort.IsOpen) return;

            int bytesToRead = _serialPort.BytesToRead;
            if (bytesToRead <= 0) return;

            if (_bufferIndex + bytesToRead > _receiveBuffer.Length)
                Array.Resize(ref _receiveBuffer, _receiveBuffer.Length * 2);

            int bytesRead = _serialPort.Read(_receiveBuffer, _bufferIndex, bytesToRead);
            _bufferIndex += bytesRead;
            BytesReceived += bytesRead;

            ProcessReceivedData();
        }

        private void ProcessReceivedData()
        {
            int processedIndex = 0;

            while (processedIndex < _bufferIndex)
            {
                if (_receiveBuffer[processedIndex] != FPGA_START)
                {
                    processedIndex++;
                    continue;
                }

                if (processedIndex + 4 > _bufferIndex) break;

                ushort dataLength = (ushort)((_receiveBuffer[processedIndex + 1] << 8) | _receiveBuffer[processedIndex + 2]);
                int totalFrameSize = 6 + dataLength;
                if (processedIndex + totalFrameSize > _bufferIndex) break;

                byte commandId = _receiveBuffer[processedIndex + 3];
                byte[] data = new byte[dataLength];
                if (dataLength > 0)
                    Array.Copy(_receiveBuffer, processedIndex + 4, data, 0, dataLength);

                byte receivedChecksum = _receiveBuffer[processedIndex + 4 + dataLength];
                byte calculatedChecksum = CalculateChecksum(commandId, data);
                byte endByte = _receiveBuffer[processedIndex + 5 + dataLength];

                if (receivedChecksum != calculatedChecksum || endByte != FPGA_END)
                {
                    ChecksumErrors++;
                    _dispatcherQueue?.TryEnqueue(() =>
                    {
                        LogMessages.Add($"Checksum error in received packet");
                        LogMessageAdded?.Invoke(this, $"Checksum error in received packet");
                    });
                    processedIndex++;
                    continue;
                }

                var command = new FpgaCommand((ushort)commandId, data);
                PacketsReceived++;

                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Received command: {command.Command:X4}, Data: {BitConverter.ToString(command.Data)}");
                    LogMessageAdded?.Invoke(this, $"Received command: {command.Command:X4}, Data length: {command.Data.Length}");
                    DataReceived?.Invoke(this, command);
                    CommandResponseReceived?.Invoke(this, command);
                });

                processedIndex += totalFrameSize;
            }

            if (processedIndex > 0)
            {
                int remaining = _bufferIndex - processedIndex;
                if (remaining > 0)
                    Array.Copy(_receiveBuffer, processedIndex, _receiveBuffer, 0, remaining);

                _bufferIndex = remaining;
            }
        }

        #endregion

        #region Common FPGA Commands

        public async Task<bool> SetLaserVoltageAsync(ushort voltage)
        {
            byte[] data = { (byte)(voltage >> 8), (byte)(voltage & 0xFF) };
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add($"Setting laser voltage to: {voltage}V");
                LogMessageAdded?.Invoke(this, $"Setting laser voltage to: {voltage}V");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxLsrVolts, data));
        }

        public async Task<bool> SetLaserStateAsync(bool armed)
        {
            byte state = armed ? (byte)2 : (byte)0; // 2 = Armed, 0 = Idle
            byte[] data = { state };
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add($"Setting laser state to: {(armed ? "ARMED" : "DISARMED")}");
                LogMessageAdded?.Invoke(this, $"Setting laser state to: {(armed ? "ARMED" : "DISARMED")}");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxLsrState, data));
        }

        public async Task<bool> SetLaserRunningStateAsync(bool running)
        {
            byte state = running ? (byte)3 : (byte)4; // 3 = Running, 4 = Paused
            byte[] data = { state };
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add($"Setting laser running state to: {(running ? "RUNNING" : "PAUSED")}");
                LogMessageAdded?.Invoke(this, $"Setting laser running state to: {(running ? "RUNNING" : "PAUSED")}");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxLsrState, data));
        }

        public async Task<bool> SetPulseSettingsAsync(ushort voltage, ushort energy, ushort pulseWidth, uint shots, ushort frequency)
        {
            byte[] data = new byte[16];
            // Voltage (2 bytes)
            data[0] = (byte)(voltage >> 8); data[1] = (byte)(voltage & 0xFF);
            // Energy (2 bytes)
            data[2] = (byte)(energy >> 8); data[3] = (byte)(energy & 0xFF);
            // Pulse Width (2 bytes)
            data[4] = (byte)(pulseWidth >> 8); data[5] = (byte)(pulseWidth & 0xFF);
            // Shots (4 bytes)
            data[6] = (byte)((shots >> 24) & 0xFF);
            data[7] = (byte)((shots >> 16) & 0xFF);
            data[8] = (byte)((shots >> 8) & 0xFF);
            data[9] = (byte)(shots & 0xFF);
            // Frequency (2 bytes)
            data[10] = (byte)(frequency >> 8); data[11] = (byte)(frequency & 0xFF);
            // Delays (4 bytes - set to default)
            data[12] = 0; data[13] = 100; // Delay1 = 100ms default
            data[14] = 0; data[15] = 50;  // Delay2 = 50ms default

            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add($"Setting pulse parameters - V:{voltage}V, E:{energy}J, PW:{pulseWidth}µs, Shots:{shots}, Freq:{frequency}Hz");
                LogMessageAdded?.Invoke(this, $"Setting pulse parameters - V:{voltage}V, E:{energy}J, PW:{pulseWidth}µs, Shots:{shots}, Freq:{frequency}Hz");
            });

            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxLsrPulseConfig, data));
        }

        public async Task<bool> RequestEnergyReadingAsync()
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("Requesting energy reading");
                LogMessageAdded?.Invoke(this, "Requesting energy reading");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxReadEnergy));
        }

        public async Task<bool> RequestVoltageReadingAsync()
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("Requesting voltage reading");
                LogMessageAdded?.Invoke(this, "Requesting voltage reading");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxCapacitorVoltage));
        }

        public async Task<bool> RequestTemperatureReadingAsync()
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("Requesting temperature reading");
                LogMessageAdded?.Invoke(this, "Requesting temperature reading");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxReadTemperature));
        }

        public async Task<bool> RequestInterlockStatusAsync()
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("Requesting interlock status");
                LogMessageAdded?.Invoke(this, "Requesting interlock status");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxInterlockStatus));
        }

        public async Task<bool> RequestSystemStatusAsync()
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("Requesting system status");
                LogMessageAdded?.Invoke(this, "Requesting system status");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxLsrState));
        }

        public async Task<bool> RequestShotCountAsync()
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("Requesting shot count");
                LogMessageAdded?.Invoke(this, "Requesting shot count");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxShotCount));
        }

        public string[] GetAvailablePorts()
        {
            var ports = SerialPort.GetPortNames();
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add($"Found {ports.Length} available serial ports");
                LogMessageAdded?.Invoke(this, $"Found {ports.Length} available serial ports");
            });
            return ports;
        }

        public async Task<bool> SetShutterStateAsync(bool open)
        {
            byte[] data = { 0, (byte)(open ? 1 : 0) }; // Mode 0 = manual, State 1 = open, 0 = closed
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add($"Setting shutter state to: {(open ? "OPEN" : "CLOSED")}");
                LogMessageAdded?.Invoke(this, $"Setting shutter state to: {(open ? "OPEN" : "CLOSED")}");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxShutterConfig, data));
        }

        public async Task<bool> StartChargingAsync(ushort voltageSetpoint)
        {
            // Set the voltage first
            byte[] voltageData = { (byte)(voltageSetpoint >> 8), (byte)(voltageSetpoint & 0xFF) };
            await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxLsrVolts, voltageData));

            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add($"Starting charging process at {voltageSetpoint}V");
                LogMessageAdded?.Invoke(this, $"Starting charging process at {voltageSetpoint}V");
            });

            // For charging, we need to set the laser state to armed first
            return await SetLaserStateAsync(true);
        }

        public async Task<bool> StopChargingAsync()
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("Stopping charging process");
                LogMessageAdded?.Invoke(this, "Stopping charging process");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxLsrChargeCancel, new byte[] { 1 }));
        }

        public async Task<bool> ResetSystemAsync()
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("Resetting system");
                LogMessageAdded?.Invoke(this, "Resetting system");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxReset));
        }

        public async Task<bool> RequestChargeStateAsync()
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("Requesting charge state");
                LogMessageAdded?.Invoke(this, "Requesting charge state");
            });
            return await SendCommandAsync(new FpgaCommand(FpgaCommandIds.lcdRxCapacitorVoltage));
        }

        #endregion

        public void Dispose()
        {
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.DataReceived -= SerialPort_DataReceived;
            _serialPort.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}