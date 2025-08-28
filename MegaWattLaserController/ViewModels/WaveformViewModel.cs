using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Kernel.Sketches;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace LaserControllerApp.ViewModels
{
    public class WaveformViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ObservablePoint> _dataPoints;
        private IEnumerable<ICartesianAxis> _xAxes;
        private IEnumerable<ICartesianAxis> _yAxes;

        public WaveformViewModel()
        {
            InitializeChart();
        }

        public ISeries[] Series { get; private set; }

        public IEnumerable<ICartesianAxis> XAxes
        {
            get => _xAxes;
            set
            {
                _xAxes = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<ICartesianAxis> YAxes
        {
            get => _yAxes;
            set
            {
                _yAxes = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ObservablePoint> DataPoints
        {
            get => _dataPoints;
            set
            {
                _dataPoints = value;
                OnPropertyChanged();
            }
        }

        private void InitializeChart()
        {
            // Initialize data collection
            DataPoints = new ObservableCollection<ObservablePoint>();

            // Configure series
            Series = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = DataPoints,
                    Stroke = new SolidColorPaint(SKColors.Blue, 2f),
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0,
                    Name = "Laser Output"
                }
            };

            // Configure X axis (Time)
            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Time (s)",
                    NameTextSize = 12,
                    TextSize = 10,
                    Labeler = value => value.ToString("F1"),
                    MinLimit = 0,
                    MaxLimit = 10 // Initial 10-second view
                }
            };

            // Configure Y axis (Energy/Voltage)
            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Energy (mJ)",
                    NameTextSize = 12,
                    TextSize = 10,
                    Labeler = value => value.ToString("F1"),
                    MinLimit = 0,
                    MaxLimit = 100 // Adjust based on expected range
                }
            };
        }

        public void AddDataPoint(double time, double value)
        {
            DataPoints.Add(new ObservablePoint(time, value));

            // Auto-scroll: keep last 10 seconds visible
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
                double currentTime = DataPoints[^1].X ?? 0; // Add null coalescing
                ((Axis)XAxes.First()).MinLimit = currentTime - maxSeconds;
                ((Axis)XAxes.First()).MaxLimit = currentTime;
            }
            else
            {
                ((Axis)XAxes.First()).MinLimit = 0;
                ((Axis)XAxes.First()).MaxLimit = maxSeconds;
            }
            OnPropertyChanged(nameof(XAxes));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}