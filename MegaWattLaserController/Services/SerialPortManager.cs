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

        [ObservableProperty] private bool _isConnected;
        [ObservableProperty] private string? _portName;
        [ObservableProperty] private ObservableCollection<string> _logMessages = new();
        [ObservableProperty] private int _bytesReceived;
        [ObservableProperty] private int _bytesSent;
        [ObservableProperty] private int _packetsReceived;
        [ObservableProperty] private int _packetsSent;
        [ObservableProperty] private int _checksumErrors;

        private const byte FPGA_START = 0x2A;
        private const byte FPGA_END = 0x3A;

        // <-- PUBLIC constructor for DI
        public SerialPortManager()
        {
            _serialPort = new SerialPort();
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Initialize(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

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
                        ConnectionStatusChanged?.Invoke(this, false);
                    });
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

        public async Task<bool> SendCommandAsync(FpgaCommand command)
        {
            if (!_serialPort.IsOpen)
            {
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add("Cannot send command: Serial port is not open");
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
                _dispatcherQueue?.TryEnqueue(() => LogMessages.Add($"Sent command: {command}"));

                return true;
            }
            catch (Exception ex)
            {
                _dispatcherQueue?.TryEnqueue(() =>
                {
                    LogMessages.Add($"Send error: {ex.Message}");
                    ErrorOccurred?.Invoke(this, $"Send error: {ex.Message}");
                });
                return false;
            }
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
            for (int i = 1; i < index; i++) checksum ^= packet[i];
            packet[index++] = checksum;
            packet[index] = FPGA_END;

            return packet;
        }

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

            ProcessReceivedData(_receiveBuffer, _bufferIndex);
        }

        private void ProcessReceivedData(byte[] data, int length)
        {
            int processedIndex = 0;
            while (true)
            {
                int start = Array.IndexOf(data, FPGA_START, processedIndex);
                if (start == -1) break;

                int end = Array.IndexOf(data, FPGA_END, start + 1);
                if (end == -1) break;

                int frameLength = end - start + 1;
                if (frameLength >= 7)
                {
                    byte[] frame = new byte[frameLength];
                    Array.Copy(data, start, frame, 0, frameLength);

                    var command = ParseCommandFrame(frame);
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

                processedIndex = end + 1;
            }

            if (processedIndex > 0)
            {
                Array.Copy(data, processedIndex, data, 0, length - processedIndex);
                _bufferIndex = length - processedIndex;
            }
        }

        private FpgaCommand? ParseCommandFrame(byte[] frame)
        {
            if (frame.Length < 7) return null;
            if (frame[0] != FPGA_START || frame[frame.Length - 1] != FPGA_END) return null;

            ushort cmd = (ushort)((frame[1] << 8) | frame[2]);
            ushort dataLen = (ushort)((frame[3] << 8) | frame[4]);

            if (frame.Length - 7 != dataLen) return null;

            byte checksum = 0;
            for (int i = 1; i < 5 + dataLen; i++) checksum ^= frame[i];
            if (checksum != frame[5 + dataLen])
            {
                ChecksumErrors++;
                return null;
            }

            byte[] data = new byte[dataLen];
            Array.Copy(frame, 5, data, 0, dataLen);

            return new FpgaCommand { Command = cmd, Data = data };
        }

        public async Task<bool> RequestEnergyReadingAsync()
        {
            try
            {
                var command = new FpgaCommand(FpgaCommandIds.lcdTxReadEnergy);
                await SendCommandAsync(command);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> RequestTemperatureReadingAsync()
        {
            try
            {
                var command = new FpgaCommand(FpgaCommandIds.lcdTxReadTemperature);
                await SendCommandAsync(command);
                return true;
            }
            catch { return false; }
        }

        public string[] GetAvailablePorts() => SerialPort.GetPortNames();

        public void Dispose()
        {
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.DataReceived -= SerialPort_DataReceived;
            _serialPort.Dispose();
        }
    }
}
