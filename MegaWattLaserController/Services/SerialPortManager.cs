using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LaserControllerApp.Services
{
    public sealed class SerialPortManager : IDisposable
    {
        #region Singleton Implementation
        private static readonly Lazy<SerialPortManager> _instance =
            new Lazy<SerialPortManager>(() => new SerialPortManager());

        public static SerialPortManager Instance => _instance.Value;
        #endregion

        #region Private Fields
        private readonly SerialPort _serialPort;
        private readonly ObservableCollection<string> _logMessages = new ObservableCollection<string>();
        private bool _isDisposed;
        private bool _isConnected;
        private readonly object _lockObject = new object();
        private CancellationTokenSource _readCancellationTokenSource;
        private Task _readTask;
        #endregion

        #region Events
        public event EventHandler<string> ConnectionStatusChanged;
        public event EventHandler<string> DataReceived;
        public event EventHandler<string> ErrorOccurred;
        #endregion

        #region Properties
        public bool IsConnected
        {
            get
            {
                lock (_lockObject)
                {
                    return _isConnected && _serialPort?.IsOpen == true;
                }
            }
        }

        public string PortName => _serialPort?.PortName ?? "Not connected";
        public int BaudRate => _serialPort?.BaudRate ?? 9600;
        public string ConnectionStatus => IsConnected ? $"Connected to {PortName}" : "Disconnected";

        public ObservableCollection<string> LogMessages => _logMessages;
        #endregion

        #region Constructor
        private SerialPortManager()
        {
            _serialPort = new SerialPort
            {
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                ReadTimeout = 1000,
                WriteTimeout = 1000,
                Handshake = Handshake.None,
                NewLine = "\r\n"
            };

            _readCancellationTokenSource = new CancellationTokenSource();
            AddLogMessage("SerialPortManager initialized");
        }
        #endregion

        #region Public Methods
        public string[] GetAvailablePorts()
        {
            try
            {
                return SerialPort.GetPortNames()
                    .OrderBy(port => port)
                    .ToArray();
            }
            catch (Exception ex)
            {
                AddLogMessage($"Failed to get available ports: {ex.Message}");
                OnErrorOccurred($"Failed to get available ports: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        public async Task<bool> ConnectAsync(string portName, int baudRate = 9600)
        {
            if (IsConnected)
            {
                await DisconnectAsync();
            }

            lock (_lockObject)
            {
                try
                {
                    _serialPort.PortName = portName;
                    _serialPort.BaudRate = baudRate;

                    _serialPort.Open();
                    _isConnected = true;

                    // Start background reading task
                    _readCancellationTokenSource = new CancellationTokenSource();
                    _readTask = Task.Run(() => ReadFromSerialPortAsync(_readCancellationTokenSource.Token));

                    AddLogMessage($"Connected to {portName} at {baudRate} baud");
                    OnConnectionStatusChanged($"Connected to {portName}");

                    return true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    AddLogMessage($"Access denied to serial port: {ex.Message}");
                    OnErrorOccurred($"Access denied to {portName}. Check permissions.");
                }
                catch (ArgumentException ex)
                {
                    AddLogMessage($"Invalid port name: {ex.Message}");
                    OnErrorOccurred($"Invalid port name: {portName}");
                }
                catch (IOException ex)
                {
                    AddLogMessage($"I/O error: {ex.Message}");
                    OnErrorOccurred($"I/O error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    AddLogMessage($"Connection failed: {ex.Message}");
                    OnErrorOccurred($"Connection failed: {ex.Message}");
                }

                _isConnected = false;
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            lock (_lockObject)
            {
                if (!_isConnected) return;

                try
                {
                    // Cancel background reading
                    _readCancellationTokenSource?.Cancel();

                    if (_serialPort?.IsOpen == true)
                    {
                        _serialPort.Close();
                        AddLogMessage("Disconnected from serial port");
                    }
                }
                catch (Exception ex)
                {
                    AddLogMessage($"Error during disconnection: {ex.Message}");
                    OnErrorOccurred($"Disconnection error: {ex.Message}");
                }
                finally
                {
                    _isConnected = false;
                    OnConnectionStatusChanged("Disconnected");
                }
            }

            // Wait for read task to complete
            if (_readTask != null)
            {
                try
                {
                    await _readTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when we cancel the task
                }
                catch (Exception ex)
                {
                    AddLogMessage($"Error in read task during shutdown: {ex.Message}");
                }
            }
        }

        public async Task<bool> SendCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                AddLogMessage("Cannot send empty command");
                OnErrorOccurred("Cannot send empty command");
                return false;
            }

            if (!IsConnected)
            {
                AddLogMessage("Not connected to any port");
                OnErrorOccurred("Not connected to any port");
                return false;
            }

            try
            {
                // Ensure command ends with newline
                var formattedCommand = command.EndsWith("\n") ? command : command + "\n";

                lock (_lockObject)
                {
                    _serialPort.WriteLine(formattedCommand);
                }

                AddLogMessage($"Sent: {formattedCommand.Trim()}");
                return true;
            }
            catch (TimeoutException ex)
            {
                AddLogMessage($"Write timeout: {ex.Message}");
                OnErrorOccurred($"Write timeout: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                AddLogMessage($"Port not open: {ex.Message}");
                OnErrorOccurred("Port is not open");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Failed to send command: {ex.Message}");
                OnErrorOccurred($"Failed to send command: {ex.Message}");
            }

            return false;
        }

        public void UpdateBaudRate(int baudRate)
        {
            lock (_lockObject)
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    AddLogMessage("Cannot change baud rate while port is open");
                    return;
                }

                _serialPort.BaudRate = baudRate;
                AddLogMessage($"Baud rate set to {baudRate}");
            }
        }

        public void ClearLog()
        {
            _logMessages.Clear();
        }
        #endregion

        #region Private Methods
        private async Task ReadFromSerialPortAsync(CancellationToken cancellationToken)
        {
            AddLogMessage("Starting serial port reading task");

            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                try
                {
                    string data;
                    lock (_lockObject)
                    {
                        if (_serialPort?.IsOpen != true || _serialPort.BytesToRead == 0)
                        {
                            continue;
                        }

                        data = _serialPort.ReadLine();
                    }

                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        AddLogMessage($"Received: {data.Trim()}");
                        OnDataReceived(data);
                    }
                }
                catch (TimeoutException)
                {
                    // Normal timeout, continue reading
                    await Task.Delay(100, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Shutdown requested
                    break;
                }
                catch (InvalidOperationException)
                {
                    // Port was closed
                    break;
                }
                catch (Exception ex)
                {
                    AddLogMessage($"Error reading from serial port: {ex.Message}");
                    OnErrorOccurred($"Read error: {ex.Message}");
                    await Task.Delay(1000, cancellationToken); // Wait before retrying
                }
            }

            AddLogMessage("Serial port reading task stopped");
        }

        private void AddLogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[{timestamp}] {message}";

            // For WinUI 3, we need to use DispatcherQueue for UI thread access
            _logMessages.Add(logEntry);

            // Keep log to a reasonable size
            if (_logMessages.Count > 1000)
            {
                _logMessages.RemoveAt(0);
            }
        }

        private void OnDataReceived(string data)
        {
            DataReceived?.Invoke(this, data);
        }

        private void OnConnectionStatusChanged(string status)
        {
            ConnectionStatusChanged?.Invoke(this, status);
        }

        private void OnErrorOccurred(string errorMessage)
        {
            ErrorOccurred?.Invoke(this, errorMessage);
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _readCancellationTokenSource?.Cancel();

            lock (_lockObject)
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                }
                _serialPort?.Dispose();
            }

            _readCancellationTokenSource?.Dispose();

            // Wait for read task to complete if it's still running
            try
            {
                _readTask?.Wait(1000); // Wait up to 1 second
            }
            catch (AggregateException)
            {
                // Task was cancelled, which is expected
            }

            AddLogMessage("SerialPortManager disposed");
        }
        #endregion
    }
}