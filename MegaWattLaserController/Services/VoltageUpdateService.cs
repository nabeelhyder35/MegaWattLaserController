// File: MegaWattLaserController/Services/VoltageUpdateService.cs
using LaserControllerApp.Services;
using System;
using System.Collections.Generic;

namespace LaserControllerApp.Services
{
    public class VoltageUpdateService
    {
        private static readonly Lazy<VoltageUpdateService> _instance =
            new(() => new VoltageUpdateService());

        public static VoltageUpdateService Instance => _instance.Value;

        private readonly List<Action<VoltageUpdateMessage>> _subscribers = new();

        private VoltageUpdateService() { }

        public void Subscribe(Action<VoltageUpdateMessage> callback)
        {
            if (!_subscribers.Contains(callback))
                _subscribers.Add(callback);
        }

        public void Unsubscribe(Action<VoltageUpdateMessage> callback)
        {
            if (_subscribers.Contains(callback))
                _subscribers.Remove(callback);
        }

        public void Publish(VoltageUpdateMessage message)
        {
            foreach (var sub in _subscribers)
            {
                sub(message);
            }
        }
    }
}