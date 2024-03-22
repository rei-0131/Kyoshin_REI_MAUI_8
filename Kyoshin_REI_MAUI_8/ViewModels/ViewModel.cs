using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
namespace Kyoshin_REI_MAUI_8.ViewModels
{
    public partial class ViewModel : ObservableObject
    {
        private readonly Random _random = new();
        private readonly ObservableCollection<double> _observableValues;

        public ViewModel()
        {
            _observableValues = new ObservableCollection<double>
            {
                99.9,
                99.9,
                99.9,
                99.9,
                99.9,
                99.9,
                99.9,
                99.9,
                99.9,
                99.9,
            };

            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = _observableValues,
                    Fill = null
                }
            };
        }

        public ObservableCollection<ISeries> Series { get; set; }

        [RelayCommand]
        public void AddItem()
        {
            var itemValue = 0.0;
            if (AccelMonitorPage.monitor_type_)
                itemValue = RealTimePage.gal;
            else
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
            _observableValues.Add(itemValue);
        }

        [RelayCommand]
        public void RemoveItem()
        {
            if (_observableValues.Count == 0) return;
            _observableValues.RemoveAt(0);
        }
    }
}
