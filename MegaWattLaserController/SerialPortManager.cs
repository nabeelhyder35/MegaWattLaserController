using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace LaserControllerApp
{
    public class SerialPortManager
    {
        private static readonly SerialPortManager instance = new SerialPortManager();
        public static SerialPortManager Instance => instance;

        private SerialPort serialPort;
        private ObservableCollection<string> logMessages = new ObservableCollection<string>();
        public event EventHandler<string> ConnectionStatusChanged;

        private SerialPortManager()
        {
        }

        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public bool Connect(string portName, int baudRate = 9600)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                    serialPort.Close();

                serialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = baudRate,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                serialPort.Open();
                StartReading();
                ConnectionStatusChanged?.Invoke(this, $"Connected to {portName}");
                return true;
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke(this, $"Connection failed: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                ConnectionStatusChanged?.Invoke(this, "Disconnected");
            }
        }

        private async void StartReading()
        {
            while (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    string data = await Task.Run(() => serialPort.ReadLine());
                    logMessages.Add($"Received: {data}");
                }
                catch (TimeoutException)
                {
                }
                catch (Exception ex)
                {
                    logMessages.Add($"Error: {ex.Message}");
                }
            }
        }

        public void SendCommand(string command)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine(command);
                logMessages.Add($"Sent: {command}");
            }
        }

        public ObservableCollection<string> GetLogMessages()
        {
            return logMessages;
        }
    }
}