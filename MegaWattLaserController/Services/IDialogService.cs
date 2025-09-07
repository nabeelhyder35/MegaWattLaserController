using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace LaserControllerApp.Services
{
    public interface IDialogService
    {
        void Initialize(XamlRoot xamlRoot);
        Task<bool> ShowPulseSettingsDialogAsync(ushort voltage, ushort energy, ushort pulseWidth, uint shots, ushort frequency);
        Task<(ushort Voltage, ushort Energy, ushort PulseWidth, uint Shots, ushort Frequency)?> ShowPulseSettingsDialogWithResultAsync(ushort voltage, ushort energy, ushort pulseWidth, uint shots, ushort frequency);
        Task<bool> ShowConfirmationDialogAsync(string title, string message);
        Task ShowMessageDialogAsync(string title, string message);
    }
}