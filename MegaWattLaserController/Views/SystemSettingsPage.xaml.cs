using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class SystemSettingsPage : Page
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly MainViewModel _mainViewModel;

        public SystemSettingsPage()
            : this(App.Services.GetRequiredService<SerialPortManager>(),
                   App.Services.GetRequiredService<MainViewModel>())
        {
        }

        public SystemSettingsPage(SerialPortManager serialPortManager, MainViewModel mainViewModel)
        {
            this.InitializeComponent();

            _serialPortManager = serialPortManager ?? throw new ArgumentNullException(nameof(serialPortManager));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateStatusDisplay();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Cleanup if needed
        }

        private void UpdateStatusDisplay()
        {
            ConnectionStatusText.Text = _serialPortManager.IsConnected ?
                "Connected" : "Disconnected";
            ConnectionStatusText.Foreground = _serialPortManager.IsConnected ?
                new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green) :
                new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);

            LaserStateText.Text = _mainViewModel.CurrentState.ToString();
        }

        #region Navigation Button Handlers

        private void PulseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to pulse settings page
            this.Frame?.Navigate(typeof(PulseSettingsPage));
        }

        private void VoltageSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to voltage control page
            this.Frame?.Navigate(typeof(EnergyPage));
        }

        private void DelaySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureNotImplemented("Delay Settings");
        }

        private void InterlockSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to interlock settings
            this.Frame?.Navigate(typeof(InterlockStatusPage));
        }

        private void ShutterSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to shutter control page
            this.Frame?.Navigate(typeof(ShutterPage));
        }

        private void SoftStartSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureNotImplemented("Soft Start Settings");
        }

        private void WaveformSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to waveform page
            this.Frame?.Navigate(typeof(WaveformPage));
        }

        private void SystemInfoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureNotImplemented("System Information");
        }

        private void UserSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureNotImplemented("User Settings");
        }

        private async void FactorySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Prompt for factory password
            var passwordDialog = new ContentDialog
            {
                Title = "Factory Settings",
                Content = "Enter factory password:",
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var passwordBox = new PasswordBox();
            passwordDialog.Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = "Enter factory password:" },
                    passwordBox
                }
            };

            var result = await passwordDialog.ShowAsync();

            if (result == ContentDialogResult.Primary && passwordBox.Password == "29925") // Default factory password
            {
                ShowFeatureNotImplemented("Factory Settings");
            }
            else if (result == ContentDialogResult.Primary)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Invalid factory password",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private void PasswordSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureNotImplemented("Password Settings");
        }

        private void ConfigSaveButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureNotImplemented("Configuration Save/Restore");
        }

        #endregion

        #region Quick Actions Handlers

        private async void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSettingsButton.IsEnabled = false;
                // Implement settings save logic
                await Task.Delay(500); // Simulate save operation

                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Settings saved successfully",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to save settings: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                SaveSettingsButton.IsEnabled = true;
            }
        }

        private async void LoadSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadSettingsButton.IsEnabled = false;
                // Implement settings load logic
                await Task.Delay(500); // Simulate load operation

                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Settings loaded successfully",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();

                UpdateStatusDisplay();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to load settings: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                LoadSettingsButton.IsEnabled = true;
            }
        }

        private async void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Confirm Reset",
                Content = "Are you sure you want to reset all settings to default?",
                PrimaryButtonText = "Reset",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    ResetSettingsButton.IsEnabled = false;
                    // Implement reset logic
                    await Task.Delay(500); // Simulate reset operation

                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Settings reset to default",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();

                    UpdateStatusDisplay();
                }
                catch (Exception ex)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to reset settings: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
                finally
                {
                    ResetSettingsButton.IsEnabled = true;
                }
            }
        }

        #endregion

        private async void ShowFeatureNotImplemented(string featureName)
        {
            var dialog = new ContentDialog
            {
                Title = "Feature Not Implemented",
                Content = $"{featureName} is not yet implemented in this version.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}