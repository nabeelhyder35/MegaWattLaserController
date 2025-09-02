using System.Collections.Generic;

namespace LaserControllerApp.Models
{
    public class InterlockStatus
    {
        public List<InterlockItem> Interlocks { get; set; } = new List<InterlockItem>();
        public int StatusMask { get; set; }
        public int FaultMask { get; set; }
        public int EnableMask { get; set; }
    }

    public class InterlockItem
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsFaulted { get; set; }
        public bool HasHistoricalFault { get; set; }
        public string StatusText { get; set; }
    }
}