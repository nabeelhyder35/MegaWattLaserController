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
        // Singleton instance
        private static readonly Lazy<SerialPortManager> _instance = new Lazy<SerialPortManager>(() => new SerialPortManager());
        public static SerialPortManager Instance => _instance.Value;

        private SerialPort _serialPort;
        private byte[] _receiveBuffer = new byte[4096];
        private int _bufferIndex = 0;
        private DispatcherQueue? _dispatcherQueue;

        public event EventHandler<bool>? ConnectionStatusChanged;
        public event EventHandler<FpgaCommand>? DataReceived;
        public event EventHandler<FpgaCommand>? CommandResponseReceived;
        public event EventHandler<string>? ErrorOccurred;

        [ObservableProperty]
        private bool _isConnected;

        [ObservableProperty]
        private string? _portName;

        [ObservableProperty]
        private ObservableCollection<string> _logMessages = new ObservableCollection<string>();

        [ObservableProperty]
        private int _bytesReceived;

        [ObservableProperty]
        private int _bytesSent;

        [ObservableProperty]
        private int _packetsReceived;

        [ObservableProperty]
        private int _packetsSent;

        [ObservableProperty]
        private int _checksumErrors;

        private const byte FPGA_START = 0x2A;
        private const byte FPGA_END = 0x3A;

        // Private constructor for singleton pattern
        private SerialPortManager()
        {
            _serialPort = new SerialPort();
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        // Public parameterless constructor for DI compatibility
        public SerialPortManager(bool forDi = false) : this()
        {
            // This constructor is only for DI - it calls the private constructor
            // The 'forDi' parameter is just to differentiate the signature
        }

        public void Initialize(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public async Task<bool> ConnectAsync(string portName, int baudRate = 9600)
        {
            if (_serialPort.IsOpen)
            {
                await DisconnectAsync();
            }

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
                    ErrorOccurred?.Invoke(this, $"Connection error: {ex.Message}");
                });
                return false;
            }
        }

        public async Task<bool> EnsureConnectionAsync(string portName, int baudRate = 9600, int maxRetries = 3)
        {
            if (IsConnected && PortName == portName) return true;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                if (await ConnectAsync(portName, baudRate))
                {
                    return true;
                }

                if (attempt < maxRetries)
                {
                    await Task.Delay(1000 * attempt);
                }
            }

            return false;
        }

        public async Task DisconnectAsync(bool allowReconnect = false)
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
                        ConnectionStatusChanged?.Invoke(this, false);
                    });

                    if (allowReconnect)
                    {
                        TryAutoReconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Disconnection error: {ex.Message}");
                    ErrorOccurred?.Invoke(this, $"Disconnection error: {ex.Message}");
                });
            }
        }

        private async void TryAutoReconnect()
        {
            if (!IsConnected && !string.IsNullOrEmpty(PortName))
            {
                await Task.Delay(5000);
                await EnsureConnectionAsync(PortName);
            }
        }

        public async Task SendCommandAsync(FpgaCommand command)
        {
            if (!_serialPort.IsOpen)
            {
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add("Cannot send command: Serial port is not open");
                    ErrorOccurred?.Invoke(this, "Serial port is not open");
                });
                return;
            }

            try
            {
                byte[] packet = BuildCommandPacket(command);
                await Task.Run(() => _serialPort.Write(packet, 0, packet.Length));

                BytesSent += packet.Length;
                PacketsSent++;

                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Sent command: {command}");
                });
            }
            catch (Exception ex)
            {
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Send error: {ex.Message}");
                    ErrorOccurred?.Invoke(this, $"Send error: {ex.Message}");
                });
            }
        }

        public async Task UpdateBaudRateAsync(int baudRate)
        {
            if (_serialPort.IsOpen)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        _serialPort.Close();
                        _serialPort.BaudRate = baudRate;
                        _serialPort.Open();
                    });
                    _dispatcherQueue?.TryEnqueue(() =>
                    {
                        LogMessages.Add($"Baud rate updated to {baudRate}");
                    });
                }
                catch (Exception ex)
                {
                    _dispatcherQueue?.TryEnqueue(() =>
                    {
                        LogMessages.Add($"Error updating baud rate: {ex.Message}");
                        ErrorOccurred?.Invoke(this, $"Error updating baud rate: {ex.Message}");
                    });
                }
            }
            else
            {
                _serialPort.BaudRate = baudRate;
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Baud rate set to {baudRate} (port not open)");
                });
            }
        }

        public void ClearLogMessages()
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Clear();
            });
        }

        public void ResetStatistics()
        {
            BytesReceived = 0;
            BytesSent = 0;
            PacketsReceived = 0;
            PacketsSent = 0;
            ChecksumErrors = 0;

            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("Statistics reset");
            });
        }

        private byte[] BuildCommandPacket(FpgaCommand command)
        {
            int totalLength = 7 + command.Data.Length;
            byte[] packet = new byte[totalLength];

            int index = 0;
            packet[index++] = FPGA_START;
            packet[index++] = (byte)((command.Command >> 8) & 0xFF);
            packet[index++] = (byte)(command.Command & 0xFF);
            packet[index++] = (byte)((command.Data.Length >> 8) & 0xFF);
            packet[index++] = (byte)(command.Data.Length & 0xFF);

            Array.Copy(command.Data, 0, packet, index, command.Data.Length);
            index += command.Data.Length;

            byte checksum = 0;
            for (int i = 1; i < index; i++)
            {
                checksum ^= packet[i];
            }
            packet[index++] = checksum;
            packet[index] = FPGA_END;

            return packet;
        }

        public async Task SendLaserStateCommand(LaserState state)
        {
            var command = new FpgaCommand
            {
                Command = FpgaCommandIds.lcdTxLsrState,
                Data = new byte[] { (byte)state }
            };
            await SendCommandAsync(command);
        }

        public async Task SendPulseConfigCommand(int frequency, int pulseWidth, long shotTotal, int delay1, int delay2, FireMode fireMode, TriggerMode triggerMode)
        {
            var data = new byte[14];
            BitConverter.GetBytes((ushort)frequency).CopyTo(data, 0);
            BitConverter.GetBytes((ushort)pulseWidth).CopyTo(data, 2);
            BitConverter.GetBytes((uint)shotTotal).CopyTo(data, 4);
            BitConverter.GetBytes((ushort)delay1).CopyTo(data, 8);
            BitConverter.GetBytes((ushort)delay2).CopyTo(data, 10);
            data[12] = (byte)fireMode;
            data[13] = (byte)triggerMode;

            var command = new FpgaCommand
            {
                Command = FpgaCommandIds.lcdTxLsrPulseConfig,
                Data = data
            };
            await SendCommandAsync(command);
        }

        public async Task SendShutterConfigCommand(ShutterMode mode, ShutterState state)
        {
            var command = new FpgaCommand
            {
                Command = FpgaCommandIds.lcdTxShutterConfig,
                Data = new byte[] { (byte)mode, (byte)state }
            };
            await SendCommandAsync(command);
        }

        public async Task<bool> RequestEnergyReadingAsync()
        {
            try
            {
                var command = new FpgaCommand(FpgaCommandIds.lcdTxReadEnergy);
                await SendCommandAsync(command);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RequestTemperatureReadingAsync()
        {
            try
            {
                var command = new FpgaCommand(FpgaCommandIds.lcdTxReadTemperature);
                await SendCommandAsync(command);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.DataReceived -= SerialPort_DataReceived;
            _serialPort.Dispose();
            _dispatcherQueue?.TryEnqueue(() =>
            {
                LogMessages.Add("SerialPortManager disposed");
            });
        }

        private void SerialPort_DataReceived(object? sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (!_serialPort.IsOpen) return;

                int bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead <= 0) return;

                if (_bufferIndex + bytesToRead > _receiveBuffer.Length)
                {
                    Array.Resize(ref _receiveBuffer, _receiveBuffer.Length * 2);
                }

                int bytesRead = _serialPort.Read(_receiveBuffer, _bufferIndex, bytesToRead);
                _bufferIndex += bytesRead;
                BytesReceived += bytesRead;

                ProcessReceivedData(_receiveBuffer, _bufferIndex);
            }
            catch (Exception ex)
            {
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Data receive error: {ex.Message}");
                    ErrorOccurred?.Invoke(this, $"Data receive error: {ex.Message}");
                });
            }
        }

        private void ProcessReceivedData(byte[] data, int currentLength)
        {
            int processedIndex = 0;
            while (true)
            {
                int startIndex = -1;
                for (int i = processedIndex; i < currentLength; i++)
                {
                    if (data[i] == FPGA_START)
                    {
                        startIndex = i;
                        break;
                    }
                }

                if (startIndex == -1) break;

                int endIndex = -1;
                for (int i = startIndex + 1; i < currentLength; i++)
                {
                    if (data[i] == FPGA_END)
                    {
                        endIndex = i;
                        break;
                    }
                }

                if (endIndex == -1) break;

                int frameLength = endIndex - startIndex + 1;
                if (frameLength >= 7)
                {
                    byte[] frame = new byte[frameLength];
                    Array.Copy(data, startIndex, frame, 0, frameLength);

                    FpgaCommand? command = ParseCommandFrame(frame);
                    if (command != null)
                    {
                        PacketsReceived++;
                        _dispatcherQueue?.TryEnqueue(() =>
                        {
                            LogMessages.Add($"Received command: {command}");
                            DataReceived?.Invoke(this, command);
                            CommandResponseReceived?.Invoke(this, command);
                        });
                    }
                }

                processedIndex = endIndex + 1;
            }

            if (processedIndex > 0)
            {
                Array.Copy(data, processedIndex, data, 0, currentLength - processedIndex);
                _bufferIndex = currentLength - processedIndex;
            }
        }

        private FpgaCommand? ParseCommandFrame(byte[] frame)
        {
            try
            {
                if (frame.Length < 7) return null;

                if (frame[0] != FPGA_START || frame[frame.Length - 1] != FPGA_END)
                    return null;

                ushort command = (ushort)((frame[1] << 8) | frame[2]);
                ushort dataLength = (ushort)((frame[3] << 8) | frame[4]);

                if (frame.Length - 7 != dataLength)
                {
                    _dispatcherQueue?.TryEnqueue(() =>
                    {
                        LogMessages.Add($"Packet length mismatch: Expected {dataLength}, got {frame.Length - 7}");
                    });
                    return null;
                }

                byte calculatedChecksum = 0;
                for (int i = 1; i < 5 + dataLength; i++)
                {
                    calculatedChecksum ^= frame[i];
                }

                byte receivedChecksum = frame[5 + dataLength];

                if (calculatedChecksum != receivedChecksum)
                {
                    ChecksumErrors++;
                    _dispatcherQueue?.TryEnqueue(() =>
                    {
                        LogMessages.Add($"Checksum mismatch: calculated {calculatedChecksum}, received {receivedChecksum}");
                        ErrorOccurred?.Invoke(this, $"Checksum mismatch: calculated {calculatedChecksum}, received {receivedChecksum}");
                    });
                    return null;
                }

                byte[] data = new byte[dataLength];
                Array.Copy(frame, 5, data, 0, dataLength);

                return new FpgaCommand
                {
                    Command = command,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Parse error: {ex.Message}");
                    ErrorOccurred?.Invoke(this, $"Parse error: {ex.Message}");
                });
                return null;
            }
        }
    }
}