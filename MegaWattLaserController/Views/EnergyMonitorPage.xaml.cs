using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LaserControllerApp.Models;
using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class EnergyMonitorPage : Page
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly MainViewModel _viewModel;
        private DispatcherTimer _updateTimer;

        public EnergyMonitorPage()
        {
            this.InitializeComponent();

            // Get services from DI
            _serialPortManager = App.Services?.GetService<SerialPortManager>()
                               ?? throw new InvalidOperationException("SerialPortManager not available");

            _viewModel = App.Services?.GetService<MainViewModel>()
                       ?? throw new InvalidOperationException("MainViewModel not available");

            InitializeEnergyMonitoring();

            // Subscribe to energy updates
            WeakReferenceMessenger.Default.Register<EnergyUpdateMessage>(this, (r, m) =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    EnergyValueText.Text = m.Value.ToString("F2");
                });
            });

            // Subscribe to page events
            this.Loaded += Page_Loaded;
            this.Unloaded += Page_Unloaded;
        }

        private void InitializeEnergyMonitoring()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2); // Update every 2 seconds
            _updateTimer.Tick += async (s, e) => await UpdateEnergyDisplayAsync();
        }

        private async Task UpdateEnergyDisplayAsync()
        {
            if (!_serialPortManager.IsConnected)
                return;

            try
            {
                EnergyUpdateRing.Visibility = Visibility.Visible;

                // Request actual energy reading from laser
                bool success = await _serialPortManager.RequestEnergyReadingAsync();

                if (!success)
                {
                    EnergyValueText.Text = "Comm Error";
                }
                // The actual value will come via the message system from the response
            }
            catch (Exception)
            {
                EnergyValueText.Text = "Error";
                // Consider logging the exception
            }
            finally
            {
                EnergyUpdateRing.Visibility = Visibility.Collapsed;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_serialPortManager.IsConnected && _updateTimer != null)
            {
                _updateTimer.Start();
            }

            // Display current energy value if available
            EnergyValueText.Text = _viewModel.CurrentEnergy.ToString("F2");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer?.Stop();
            WeakReferenceMessenger.Default.UnregisterAll(this);

            // Unsubscribe from events
            this.Loaded -= Page_Loaded;
            this.Unloaded -= Page_Unloaded;
        }
    }

    // Message class for energy updates
    public class EnergyUpdateMessage : ValueChangedMessage<double>
    {
        public EnergyUpdateMessage(double value) : base(value) { }
    }
}