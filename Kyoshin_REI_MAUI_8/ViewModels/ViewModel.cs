using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Kyoshin_REI_MAUI_8.ViewModels
{
    public partial class ViewModel : ObservableObject
    {
        private string name = "X";
        private readonly List<DateTimePoint> _values = new();
        private readonly List<DateTimePoint> _values2 = new();
        private readonly List<DateTimePoint> _values3 = new();
        private readonly DateTimeAxis _customAxis;

        public ViewModel()
        {
            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<DateTimePoint>
                {
                    Name = name,
                    Values = _values,
                    Fill = null,
                    GeometryFill = null,
                    Stroke = new SolidColorPaint(SKColors.Red),
                    GeometryStroke = null
                },
                new LineSeries<DateTimePoint>
                {
                    Name = "Y",
                    Values = _values2,
                    Fill = null,
                    GeometryFill = null,
                    Stroke = new SolidColorPaint(SKColors.GreenYellow),
                    GeometryStroke = null
                },
                new LineSeries<DateTimePoint>
                {
                    Name = "Z",
                    Values = _values3,
                    Fill = null,
                    GeometryFill = null,
                    Stroke = new SolidColorPaint(SKColors.Blue),
                    GeometryStroke = null
                }
            };

            _customAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
            {
                CustomSeparators = GetSeparators(),
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                SeparatorsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(100))
            };

            XAxes = new Axis[] { _customAxis };

            _ = ReadData();
        }

        public ObservableCollection<ISeries> Series { get; set; }

        public Axis[] XAxes { get; set; }

        public object Sync { get; } = new object();

        public bool IsReading { get; set; } = true;

        private async Task ReadData()
        {
            while (IsReading)
            {
                if (Geoloc.app_window)
                {
                    lock (Sync)
                    {
                        var itemValue = 0.0;
                        var itemValue2 = 0.0;
                        var itemValue3 = 0.0;
                        if (AccelMonitorPage.monitor_type_)
                        {
                            name = "X";
                            itemValue = RealTimePage.ma_x;
                            itemValue2 = RealTimePage.ma_y;
                            itemValue3 = RealTimePage.ma_z;
                            _values.Add(new DateTimePoint(DateTime.Now, itemValue));
                            _values2.Add(new DateTimePoint(DateTime.Now, itemValue2));
                            _values3.Add(new DateTimePoint(DateTime.Now, itemValue3));
                        }
                        else
                        {
                            name = "震度";
                            if (RealTimePage.gal < 0.6)
                                itemValue = 0;
                            else if (RealTimePage.gal < 1.9)
                                itemValue = 1;
                            else if (RealTimePage.gal < 6)
                                itemValue = 2;
                            else if (RealTimePage.gal < 19)
                                itemValue = 3;
                            else if (RealTimePage.gal < 60)
                                itemValue = 4;
                            else if (RealTimePage.gal < 110)
                                itemValue = 5;
                            else if (RealTimePage.gal < 190)
                                itemValue = 5.5;
                            else if (RealTimePage.gal < 340)
                                itemValue = 6;
                            else if (RealTimePage.gal < 600)
                                itemValue = 6.5;
                            else if (RealTimePage.gal >= 600)
                                itemValue = 7;
                            _values.Add(new DateTimePoint(DateTime.Now, itemValue));
                        }
                        if (_values.Count > 250) _values.RemoveAt(0);

                        _customAxis.CustomSeparators = GetSeparators();
                    }
                    await Task.Delay(100);
                }
                else
                    await Task.Delay(2000);
            }
        }

        private double[] GetSeparators()
        {
            var now = DateTime.Now;

            return new double[]
            {
            now.AddSeconds(-25).Ticks,
            now.AddSeconds(-20).Ticks,
            now.AddSeconds(-15).Ticks,
            now.AddSeconds(-10).Ticks,
            now.AddSeconds(-5).Ticks,
            now.Ticks
            };
        }

        private static string Formatter(DateTime date)
        {
            var secsAgo = (DateTime.Now - date).TotalSeconds;

            return secsAgo < 1
                ? "now"
                : $"{secsAgo:N0}s";
        }
    }
}
