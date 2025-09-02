// File: MegaWattLaserController/Services/LoggerService.cs
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;

namespace LaserControllerApp.Services
{
    public class LoggerService
    {
        private readonly DispatcherQueue _dispatcherQueue;
        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        public LoggerService(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

        public void Log(string message)
        {
            string timestampedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}";
            _dispatcherQueue.TryEnqueue(() => Logs.Add(timestampedMessage));
        }

        public void ClearLogs()
        {
            _dispatcherQueue.TryEnqueue(() => Logs.Clear());
        }
    }
}