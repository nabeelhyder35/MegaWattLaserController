using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class PulseSettingsDialog : ContentDialog
    {
        // Validation limits
        private const int MaxVoltage = 2000;
        private const int MaxEnergy = 10;
        private const int MinPulseWidth = 1;
        private const int MaxPulseWidth = 1000;
        private const int MinShots = 1;
        private const int MaxShots = 9999;
        private const int MinFrequency = 1;
        private const int MaxFrequency = 100;

        // Bound string props
        public string VoltageText { get; set; }
        public string EnergyText { get; set; }
        public string PulseWidthText { get; set; }
        public string ShotsText { get; set; }
        public string FrequencyText { get; set; }

        // Parsed numeric props
        public ushort Voltage => ushort.TryParse(VoltageText, out var result) ? result : (ushort)0;
        public ushort Energy => ushort.TryParse(EnergyText, out var result) ? result : (ushort)0;
        public ushort PulseWidth => ushort.TryParse(PulseWidthText, out var result) ? result : (ushort)0;
        public uint Shots => uint.TryParse(ShotsText, out var result) ? result : 0;
        public ushort Frequency => ushort.TryParse(FrequencyText, out var result) ? result : (ushort)0;

        public PulseSettingsDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true; // prevent auto-close
            _ = ValidateAndCloseAsync();
        }

        private async Task ValidateAndCloseAsync()
        {
            try
            {
                ResetBordersToDefault();
                bool invalid = false;

                if (!ushort.TryParse(VoltageText, out var voltage) || voltage > MaxVoltage)
                {
                    SetBorderBrushRed(VoltageTextBox);
                    invalid = true;
                }

                if (!ushort.TryParse(EnergyText, out var energy) || energy > MaxEnergy)
                {
                    SetBorderBrushRed(EnergyTextBox);
                    invalid = true;
                }

                if (!ushort.TryParse(PulseWidthText, out var pulseWidth) ||
                    pulseWidth < MinPulseWidth || pulseWidth > MaxPulseWidth)
                {
                    SetBorderBrushRed(PulseWidthTextBox);
                    invalid = true;
                }

                if (!uint.TryParse(ShotsText, out var shots) ||
                    shots < MinShots || shots > MaxShots)
                {
                    SetBorderBrushRed(ShotsTextBox);
                    invalid = true;
                }

                if (!ushort.TryParse(FrequencyText, out var frequency) ||
                    frequency < MinFrequency || frequency > MaxFrequency)
                {
                    SetBorderBrushRed(FrequencyTextBox);
                    invalid = true;
                }

                if (!invalid)
                {
                    this.Hide(); // close dialog
                }
                else
                {
                    await ShowValidationErrorAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Validation error: {ex}");
            }
        }

        private async Task ShowValidationErrorAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "Validation Error",
                Content = "Please check the highlighted fields. They contain invalid values.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void ResetBordersToDefault()
        {
            SolidColorBrush defaultBrush = null;

            try
            {
                defaultBrush = Application.Current.Resources["TextControlBorderBrush"] as SolidColorBrush;
            }
            catch { }

            defaultBrush ??= new SolidColorBrush(Microsoft.UI.Colors.Gray);

            SetBorderBrush(VoltageTextBox, defaultBrush);
            SetBorderBrush(EnergyTextBox, defaultBrush);
            SetBorderBrush(PulseWidthTextBox, defaultBrush);
            SetBorderBrush(ShotsTextBox, defaultBrush);
            SetBorderBrush(FrequencyTextBox, defaultBrush);
        }

        private void SetBorderBrushRed(TextBox textBox) =>
            SetBorderBrush(textBox, new SolidColorBrush(Microsoft.UI.Colors.Red));

        private void SetBorderBrush(TextBox textBox, SolidColorBrush brush)
        {
            if (textBox != null)
            {
                textBox.BorderBrush = brush;
            }
        }

        private void NumericTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            var text = args.NewText ?? string.Empty;

            if (text.Length == 0)
            {
                args.Cancel = false; // allow clearing
                return;
            }

            args.Cancel = text.Any(ch => !char.IsDigit(ch));
        }
    }
}
