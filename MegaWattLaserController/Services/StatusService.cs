using Microsoft.UI.Dispatching;
using System;

namespace LaserControllerApp.Services
{
    public interface IStatusService
    {
        void ShowInfo(string message);
        void ShowSuccess(string message);
        void ShowWarning(string message);
        void ShowError(string message);
        void ShowBusy(string message);
        void ClearBusy();

        event EventHandler<string> StatusMessageChanged;
        event EventHandler<bool> BusyStateChanged;
    }

    public class StatusService : IStatusService
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private int _busyCount = 0;

        public event EventHandler<string> StatusMessageChanged;
        public event EventHandler<bool> BusyStateChanged;

        public StatusService(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
        }

        public void ShowInfo(string message)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                StatusMessageChanged?.Invoke(this, $"[INFO] {message}");
            });
        }

        public void ShowSuccess(string message)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                StatusMessageChanged?.Invoke(this, $"[SUCCESS] {message}");
            });
        }

        public void ShowWarning(string message)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                StatusMessageChanged?.Invoke(this, $"[WARNING] {message}");
            });
        }

        public void ShowError(string message)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                StatusMessageChanged?.Invoke(this, $"[ERROR] {message}");
            });
        }

        public void ShowBusy(string message)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _busyCount++;
                StatusMessageChanged?.Invoke(this, $"[BUSY] {message}");
                BusyStateChanged?.Invoke(this, true);
            });
        }

        public void ClearBusy()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _busyCount = Math.Max(0, _busyCount - 1);
                if (_busyCount == 0)
                {
                    BusyStateChanged?.Invoke(this, false);
                }
            });
        }
    }
}