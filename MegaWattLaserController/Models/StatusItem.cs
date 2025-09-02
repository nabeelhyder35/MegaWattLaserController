using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaserControllerApp.Models
{
    public class StatusItem
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Value { get; set; }
        public bool IsFault { get; set; }
        public bool IsRecovered { get; set; }
        public bool IsDisabled { get; set; }

        public StatusItem(string name, string status = "GOOD", string value = "",
                        bool isFault = false, bool isRecovered = false, bool isDisabled = false)
        {
            Name = name;
            Status = status;
            Value = value;
            IsFault = isFault;
            IsRecovered = isRecovered;
            IsDisabled = isDisabled;
        }
    }
}