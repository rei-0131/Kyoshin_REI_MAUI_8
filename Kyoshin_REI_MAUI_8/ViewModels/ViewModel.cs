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
                            itemValue = Math.Round(RealTimePage.sum_x, 3);
                            itemValue2 = Math.Round(RealTimePage.sum_y, 3);
                            itemValue3 = Math.Round(RealTimePage.sum_z, 3);
                            _values.Add(new DateTimePoint(DateTime.Now, itemValue));
                            _values2.Add(new DateTimePoint(DateTime.Now, itemValue2));
                            _values3.Add(new DateTimePoint(DateTime.Now, itemValue3));
                        }
                        else
                        {
                            name = "震度";
                            if(!AccelMonitorPage.intensity_type_)
                            {
                                if (RealTimePage.intensity < 0.5)
                                    itemValue = 0;
                                else if (RealTimePage.intensity < 1.5)
                                    itemValue = 1;
                                else if (RealTimePage.intensity < 2.5)
                                    itemValue = 2;
                                else if (RealTimePage.intensity < 3.5)
                                    itemValue = 3;
                                else if (RealTimePage.intensity < 4.5)
                                    itemValue = 4;
                                else if (RealTimePage.intensity < 5)
                                    itemValue = 5;
                                else if (RealTimePage.intensity < 5.5)
                                    itemValue = 5.5;
                                else if (RealTimePage.intensity < 6)
                                    itemValue = 6;
                                else if (RealTimePage.intensity < 6.5)
                                    itemValue = 6.5;
                                else if (RealTimePage.intensity >= 6.5)
                                    itemValue = 7;
                            }
                            else
                                itemValue = RealTimePage.intensity;
                            _values.Add(new DateTimePoint(DateTime.Now, itemValue));
                            _values2.Add(new DateTimePoint(DateTime.Now, itemValue));
                            _values3.Add(new DateTimePoint(DateTime.Now, itemValue));
                        }
                        if (_values.Count > 250) _values.RemoveAt(0);
                        if (_values2.Count > 250) _values2.RemoveAt(0);
                        if (_values3.Count > 250) _values3.RemoveAt(0);

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
