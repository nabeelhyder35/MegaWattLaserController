// File: MegaWattLaserController/Services/VoltageUpdateMessage.cs
using System;

namespace LaserControllerApp.Services
{
    public class VoltageUpdateMessage
    {
        public double Time { get; }
        public double Voltage { get; }

        public VoltageUpdateMessage(double time, double voltage)
        {
            Time = time;
            Voltage = voltage;
        }
    }
}