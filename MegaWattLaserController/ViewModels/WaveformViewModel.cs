using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LaserControllerApp.ViewModels
{
    public class WaveformViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ObservablePoint> _dataPoints = new();
        private IEnumerable<ICartesianAxis> _xAxes = new Axis[0];
        private IEnumerable<ICartesianAxis> _yAxes = new Axis[0];

        public WaveformViewModel()
        {
            InitializeChart();
        }

        public ISeries[] Series { get; private set; } = new ISeries[0];

        public IEnumerable<ICartesianAxis> XAxes
        {
            get => _xAxes;
            set => SetField(ref _xAxes, value);
        }

        public IEnumerable<ICartesianAxis> YAxes
        {
            get => _yAxes;
            set => SetField(ref _yAxes, value);
        }

        public ObservableCollection<ObservablePoint> DataPoints
        {
            get => _dataPoints;
            set => SetField(ref _dataPoints, value);
        }

        private void InitializeChart()
        {
            DataPoints = new ObservableCollection<ObservablePoint>();

            Series = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = DataPoints,
                    Mapping = (point, index) => new Coordinate(point.X ?? 0, point.Y ?? 0), // ✅ use constructor
                    Stroke = new SolidColorPaint(SKColors.Blue, 2f),
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0,
                    Name = "Laser Energy"
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Time (s)",
                    NameTextSize = 12,
                    TextSize = 10,
                    Labeler = value => value.ToString("F1"),
                    MinLimit = 0,
                    MaxLimit = 10
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Energy (mJ)",
                    NameTextSize = 12,
                    TextSize = 10,
                    Labeler = value => value.ToString("F1"),
                    MinLimit = 0,
                    MaxLimit = 100
                }
            };
        }

        public void AddDataPoint(double time, double value)
        {
            DataPoints.Add(new ObservablePoint(time, value));

            // Auto-scroll
            var xAxis = (Axis)XAxes.First();
            if (time > xAxis.MaxLimit)
            {
                xAxis.MinLimit = time - 10;
                xAxis.MaxLimit = time;
                OnPropertyChanged(nameof(XAxes));
            }
        }

        public void ClearData()
        {
            DataPoints.Clear();
            var xAxis = (Axis)XAxes.First();
            xAxis.MinLimit = 0;
            xAxis.MaxLimit = 10;
            OnPropertyChanged(nameof(XAxes));
        }

        public void SetTimeRange(double maxSeconds)
        {
            var xAxis = (Axis)XAxes.First();
            if (DataPoints.Count > 0)
            {
                double currentTime = DataPoints[^1].X ?? 0;
                xAxis.MinLimit = currentTime - maxSeconds;
                xAxis.MaxLimit = currentTime;
            }
            else
            {
                xAxis.MinLimit = 0;
                xAxis.MaxLimit = maxSeconds;
            }
            OnPropertyChanged(nameof(XAxes));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
