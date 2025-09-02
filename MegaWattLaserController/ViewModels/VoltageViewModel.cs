// VoltageMonitorViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using LaserControllerApp.Services;
using Microsoft.UI.Dispatching;

namespace LaserControllerApp.ViewModels
{
    public partial class VoltageMonitorViewModel : ObservableObject
    {
        private readonly VoltageUpdateService _voltageService;
        private readonly DispatcherQueue _dispatcher;

        public ObservableCollection<VoltageDataPoint> VoltageData { get; } = new();

        [ObservableProperty]
        private double latestVoltage;

        [ObservableProperty]
        private double latestTime;

        public VoltageMonitorViewModel(VoltageUpdateService voltageService, DispatcherQueue dispatcher)
        {
            _voltageService = voltageService;
            _dispatcher = dispatcher;

            _voltageService.Subscribe(OnVoltageUpdate);
        }

        private void OnVoltageUpdate(VoltageUpdateMessage msg)
        {
            _dispatcher.TryEnqueue(() =>
            {
                VoltageData.Add(new VoltageDataPoint(msg.Time, msg.Voltage));

                LatestTime = msg.Time;
                LatestVoltage = msg.Voltage;

                if (VoltageData.Count > 500)
                    VoltageData.RemoveAt(0);
            });
        }
    }

    public class VoltageDataPoint
    {
        public double Time { get; set; }
        public double Voltage { get; set; }

        public VoltageDataPoint(double time, double voltage)
        {
            Time = time;
            Voltage = voltage;
        }
    }
}
