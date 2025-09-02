using LiveChartsCore;
using LiveChartsCore.Defaults;
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
        private ObservableCollection<ObservablePoint> _dataPoints = new ObservableCollection<ObservablePoint>();
        private IEnumerable<ICartesianAxis> _xAxes = new Axis[0];
        private IEnumerable<ICartesianAxis> _yAxes = new Axis[0];
        private double _currentEnergy;
        private double _currentPower;

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

        public double CurrentEnergy
        {
            get => _currentEnergy;
            set => SetField(ref _currentEnergy, value);
        }

        public double CurrentPower
        {
            get => _currentPower;
            set => SetField(ref _currentPower, value);
        }

        private void InitializeChart()
        {
            DataPoints = new ObservableCollection<ObservablePoint>();

            Series = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = DataPoints,
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

        public void AddDataPoint(double time, double? value)
        {
            if (value.HasValue)
            {
                DataPoints.Add(new ObservablePoint(time, value.Value));
                CurrentEnergy = value.Value;
            }
            else
            {
                DataPoints.Add(new ObservablePoint(time, 0));
                CurrentEnergy = 0;
            }

            // Auto-scroll
            if (time > ((Axis)XAxes.First()).MaxLimit)
            {
                ((Axis)XAxes.First()).MinLimit = time - 10;
                ((Axis)XAxes.First()).MaxLimit = time;
                OnPropertyChanged(nameof(XAxes));
            }
        }

        public void ClearData()
        {
            DataPoints.Clear();
            ((Axis)XAxes.First()).MinLimit = 0;
            ((Axis)XAxes.First()).MaxLimit = 10;
            OnPropertyChanged(nameof(XAxes));
        }

        public void SetTimeRange(double maxSeconds)
        {
            if (DataPoints.Count > 0)
            {
                double? currentTime = DataPoints[^1].X;
                if (currentTime.HasValue)
                {
                    ((Axis)XAxes.First()).MinLimit = currentTime.Value - maxSeconds;
                    ((Axis)XAxes.First()).MaxLimit = currentTime.Value;
                }
                else
                {
                    ((Axis)XAxes.First()).MinLimit = 0;
                    ((Axis)XAxes.First()).MaxLimit = maxSeconds;
                }
            }
            else
            {
                ((Axis)XAxes.First()).MinLimit = 0;
                ((Axis)XAxes.First()).MaxLimit = maxSeconds;
            }
            OnPropertyChanged(nameof(XAxes));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}