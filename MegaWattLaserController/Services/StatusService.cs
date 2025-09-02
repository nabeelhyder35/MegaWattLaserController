using CommunityToolkit.Mvvm.ComponentModel;
using LaserControllerApp.Models;
using Microsoft.UI.Dispatching;
using System;

namespace LaserControllerApp.Services
{
    public partial class StatusService : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;

        [ObservableProperty]
        private LaserState _currentState = new LaserState();

        [ObservableProperty]
        private string _statusMessage = "System Idle";

        public StatusService(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

        public void UpdateStatus(LaserState state, string message)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                CurrentState = state;
                StatusMessage = message;
            });
        }

        public void UpdateMessage(string message)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                StatusMessage = message;
            });
        }
    }
}
