using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;

namespace LaserControllerApp.Services
{
    public interface ILoggerService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogException(Exception ex, string context);
        ObservableCollection<string> LogMessages { get; }
    }

    public partial class LoggerService : ObservableObject, ILoggerService
    {
        private readonly DispatcherQueue _dispatcherQueue;

        [ObservableProperty]
        private ObservableCollection<string> _logMessages = new ObservableCollection<string>();

        public LoggerService(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
        }

        public void LogInformation(string message)
        {
            AddLogMessage($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
        }

        public void LogWarning(string message)
        {
            AddLogMessage($"[WARN] {DateTime.Now:HH:mm:ss} - {message}");
        }

        public void LogError(string message)
        {
            AddLogMessage($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}");
        }

        public void LogException(Exception ex, string context)
        {
            AddLogMessage($"[EXCEPTION] {DateTime.Now:HH:mm:ss} - {context}: {ex.Message}");
        }

        private void AddLogMessage(string message)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                LogMessages.Add(message);
                // Keep log size manageable
                if (LogMessages.Count > 1000)
                {
                    LogMessages.RemoveAt(0);
                }
            });
        }
    }
}