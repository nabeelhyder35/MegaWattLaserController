using LaserControllerApp.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace LaserControllerApp.Services
{
    public class DialogService : IDialogService
    {
        private XamlRoot? _xamlRoot;

        public void Initialize(XamlRoot xamlRoot)
        {
            _xamlRoot = xamlRoot;
        }

        public async Task<bool> ShowPulseSettingsDialogAsync(
            ushort voltage, ushort energy, ushort pulseWidth, uint shots, ushort frequency)
        {
            if (_xamlRoot == null)
                throw new InvalidOperationException("DialogService is not initialized with a valid XamlRoot.");

            var result = await ShowPulseSettingsDialogWithResultAsync(voltage, energy, pulseWidth, shots, frequency);
            return result.HasValue; // true if user pressed OK
        }

        public async Task<(ushort Voltage, ushort Energy, ushort PulseWidth, uint Shots, ushort Frequency)?>
            ShowPulseSettingsDialogWithResultAsync(
                ushort voltage, ushort energy, ushort pulseWidth, uint shots, ushort frequency)
        {
            if (_xamlRoot == null)
                return null;

            var dialog = new PulseSettingsDialog
            {
                XamlRoot = _xamlRoot,
                VoltageText = voltage.ToString(),
                EnergyText = energy.ToString(),
                PulseWidthText = pulseWidth.ToString(),
                ShotsText = shots.ToString(),
                FrequencyText = frequency.ToString()
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return (dialog.Voltage, dialog.Energy, dialog.PulseWidth, dialog.Shots, dialog.Frequency);
            }

            return null;
        }

        public async Task<bool> ShowConfirmationDialogAsync(string title, string message)
        {
            if (_xamlRoot == null)
                return false;

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                XamlRoot = _xamlRoot
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        public async Task ShowMessageDialogAsync(string title, string message)
        {
            if (_xamlRoot == null)
                return;

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = _xamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}