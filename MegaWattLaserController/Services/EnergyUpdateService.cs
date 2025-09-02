// File: MegaWattLaserController/Services/EnergyUpdateService.cs
using LaserControllerApp.Services;
using System;
using System.Collections.Generic;

namespace LaserControllerApp.Services
{
    public class EnergyUpdateService
    {
        private static readonly Lazy<EnergyUpdateService> _instance =
            new(() => new EnergyUpdateService());

        public static EnergyUpdateService Instance => _instance.Value;

        private readonly List<Action<EnergyUpdateMessage>> _subscribers = new();

        private EnergyUpdateService() { }

        public void Subscribe(Action<EnergyUpdateMessage> callback)
        {
            if (!_subscribers.Contains(callback))
                _subscribers.Add(callback);
        }

        public void Unsubscribe(Action<EnergyUpdateMessage> callback)
        {
            if (_subscribers.Contains(callback))
                _subscribers.Remove(callback);
        }

        public void Publish(EnergyUpdateMessage message)
        {
            foreach (var sub in _subscribers)
            {
                sub(message);
            }
        }
    }
}