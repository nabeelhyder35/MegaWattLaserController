using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using LaserControllerApp.Services;

namespace LaserControllerApp.ViewModels
{
    public partial class EnergyMonitorViewModel : ObservableObject
    {
        // Collection of energy data points (time vs energy)
        public ObservableCollection<(double Time, double Energy)> EnergyData { get; }
            = new ObservableCollection<(double Time, double Energy)>();

        [ObservableProperty]
        private double latestEnergy;

        [ObservableProperty]
        private double latestTime;

        public EnergyMonitorViewModel()
        {
            // Subscribe to energy updates
            EnergyUpdateService.Instance.Subscribe(OnEnergyUpdate);
        }

        private void OnEnergyUpdate(EnergyUpdateMessage msg)
        {
            // Add new data point
            EnergyData.Add((msg.Time, msg.Energy));

            // Keep latest values for binding
            LatestTime = msg.Time;
            LatestEnergy = msg.Energy;

            // Optional: limit collection size (for performance in chart)
            if (EnergyData.Count > 500)
            {
                EnergyData.RemoveAt(0);
            }
        }
    }
}
