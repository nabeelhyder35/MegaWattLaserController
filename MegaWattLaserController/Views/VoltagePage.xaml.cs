using LaserControllerApp.Models;
using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// Alias to resolve ambiguity
using DispatcherQueueTimer = Microsoft.UI.Dispatching.DispatcherQueueTimer;

namespace LaserControllerApp.Views
{
    public sealed partial class VoltagePage : Page
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly WaveformViewModel _viewModel;
        private readonly DispatcherQueueTimer _updateTimer;
        private double _currentTime = 0;
        private bool _isMonitoring = false;

        public WaveformViewModel ViewModel => _viewModel;

        public VoltagePage()
        {
            this.InitializeComponent();

            // Resolve services from DI container safely
            _serialPortManager = App.Services.GetRequiredService<SerialPortManager>();
            _viewModel = App.Services.GetRequiredService<WaveformViewModel>();

            this.DataContext = _viewModel;

            // Subscribe to data events
            _serialPortManager.DataReceived += SerialPortManager_DataReceived;

            // Initialize UI update timer
            _updateTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(100);
            _updateTimer.Tick += UpdateTimer_Tick;

            this.Loaded += Page_Loaded;
            this.Unloaded += Page_Unloaded;
        }

        private void StartMonitoring_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPortManager.IsConnected)
            {
                StatusText.Text = "Not connected to laser";
                return;
            }

            _isMonitoring = true;
            _currentTime = 0;
            _viewModel.ClearData();
            _updateTimer.Start();

            StatusText.Text = "Monitoring started...";
        }

        private void StopMonitoring_Click(object sender, RoutedEventArgs e)
        {
            _isMonitoring = false;
            _updateTimer.Stop();
            StatusText.Text = "Monitoring stopped";
        }

        private void ClearChart_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearData();
            _currentTime = 0;
            StatusText.Text = "Chart cleared";
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SetTimeRange(10);
            StatusText.Text = "Zoom reset";
        }

        private void TimeScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure ComboBox and ViewModel are ready
            if (TimeScaleComboBox == null || _viewModel == null) return;

            if (TimeScaleComboBox.SelectedItem is ComboBoxItem selectedItem &&
                double.TryParse(selectedItem.Tag?.ToString(), out double seconds))
            {
                _viewModel.SetTimeRange(seconds);
                StatusText.Text = $"Time scale set to {seconds} seconds";
            }
        }

        private void UpdateTimer_Tick(DispatcherQueueTimer sender, object args)
        {
            if (!_isMonitoring || !_serialPortManager.IsConnected)
            {
                _updateTimer.Stop();
                _isMonitoring = false;
                StatusText.Text = "Monitoring stopped (disconnected)";
                return;
            }

            _serialPortManager.RequestEnergyReadingAsync();
        }

        private void SerialPortManager_DataReceived(object? sender, FpgaCommand command)
        {
            if (command.Command == FpgaCommandIds.lcdRxEnergyValue && command.Data.Length >= 4)
            {
                float energy = BitConverter.ToSingle(command.Data, 0);
                _currentTime += 0.1;

                // Update chart safely on UI thread
                DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                {
                    _viewModel.AddDataPoint(_currentTime, energy);
                    StatusText.Text = $"Monitoring... Last energy: {energy:F2} mJ";
                });
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer.Stop();
            _isMonitoring = false;
            _serialPortManager.DataReceived -= SerialPortManager_DataReceived;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Set default selection safely
            if (TimeScaleComboBox?.Items?.Count > 0)
                TimeScaleComboBox.SelectedIndex = Math.Min(2, TimeScaleComboBox.Items.Count - 1);
        }
    }
}
