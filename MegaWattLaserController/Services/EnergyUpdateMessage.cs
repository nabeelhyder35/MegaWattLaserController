using System;

namespace LaserControllerApp.Services
{
    public class EnergyUpdateMessage
    {
        public double Energy { get; }
        public double Time { get; }

        public EnergyUpdateMessage(double time, double energy)
        {
            Time = time;
            Energy = energy;
        }
    }
}
