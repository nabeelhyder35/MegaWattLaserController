using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Linq;

namespace LaserControllerApp
{
    public sealed partial class WaveformPage : Page
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;
        private readonly WaveformViewModel _viewModel;
        private readonly DispatcherQueueTimer _updateTimer;
        private double _currentTime = 0;
        private bool _isMonitoring = false;

        public WaveformViewModel ViewModel => _viewModel;

        public WaveformPage()
        {
            this.InitializeComponent();
            _viewModel = new WaveformViewModel();
            _updateTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(100);
            _updateTimer.Tick += UpdateTimer_Tick;
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
            var xAxis = (Axis)_viewModel.XAxes.First();
            var yAxis = (Axis)_viewModel.YAxes.First();

            if (_viewModel.DataPoints.Count > 0)
            {
                double currentTime = _viewModel.DataPoints[^1].X ?? 0; // Add null coalescing

                xAxis.MinLimit = Math.Max(0, currentTime - 10);
                xAxis.MaxLimit = currentTime;
            }
            else
            {
                xAxis.MinLimit = 0;
                xAxis.MaxLimit = 10;
            }

            yAxis.MinLimit = 0;
            yAxis.MaxLimit = 100;

            _viewModel.OnPropertyChanged(nameof(_viewModel.XAxes));
            _viewModel.OnPropertyChanged(nameof(_viewModel.YAxes));

            StatusText.Text = "Zoom reset";
        }

        private void TimeScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeScaleComboBox.SelectedItem is ComboBoxItem selectedItem &&
                double.TryParse(selectedItem.Tag?.ToString(), out double seconds))
            {
                _viewModel.SetTimeRange(seconds);
                StatusText.Text = $"Time scale set to {seconds} seconds";
            }
        }

        private async void UpdateTimer_Tick(DispatcherQueueTimer sender, object args)
        {
            if (!_isMonitoring || !_serialPortManager.IsConnected)
            {
                _updateTimer.Stop();
                _isMonitoring = false;
                StatusText.Text = "Monitoring stopped (disconnected)";
                return;
            }

            try
            {
                // Simulate data acquisition
                double simulatedValue = await GetSimulatedEnergyReadingAsync();

                _currentTime += 0.1;
                _viewModel.AddDataPoint(_currentTime, simulatedValue);

                StatusText.Text = $"Monitoring... Last value: {simulatedValue:F2} mJ";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
                _updateTimer.Stop();
                _isMonitoring = false;
            }
        }

        private async Task<double> GetSimulatedEnergyReadingAsync()
        {
            await Task.Delay(10);
            var random = new Random();
            double baseValue = 50 + (Math.Sin(_currentTime) * 20);
            double noise = (random.NextDouble() - 0.5) * 5;

            return Math.Max(0, baseValue + noise);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer.Stop();
            _isMonitoring = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TimeScaleComboBox.SelectedIndex = 2;
        }
    }
}