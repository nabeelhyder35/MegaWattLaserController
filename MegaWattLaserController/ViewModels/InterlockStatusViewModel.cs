using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace LaserControllerApp.ViewModels
{
    /// <summary>
    /// Represents a single interlock item with name, status, and color.
    /// </summary>
    public partial class InterlockItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string status = "";

        [ObservableProperty]
        private string statusColor = "Gray";
    }

    /// <summary>
    /// ViewModel for managing all interlock items.
    /// </summary>
    public partial class InterlockStatusViewModel : ObservableObject
    {
        public ObservableCollection<InterlockItemViewModel> InterlockItems { get; } = new();

        /// <summary>
        /// Updates the collection of interlock items based on byte data.
        /// 0 = OK, 1 = FAULT.
        /// </summary>
        /// <param name="data">Byte array representing interlock states.</param>
        public void UpdateInterlockStatus(byte[] data)
        {
            InterlockItems.Clear();

            for (int i = 0; i < data.Length; i++)
            {
                InterlockItems.Add(new InterlockItemViewModel
                {
                    Name = $"Interlock {i + 1}",
                    Status = data[i] == 0 ? "OK" : "FAULT",
                    StatusColor = data[i] == 0 ? "Green" : "Red"
                });
            }
        }
    }
}
