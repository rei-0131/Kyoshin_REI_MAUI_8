using Android.App;
using AndroidX.Emoji2.Text.FlatBuffer;
using System.Diagnostics;

namespace Kyoshin_REI_MAUI_8;

public partial class IntensityPage : ContentPage
{
	public IntensityPage()
	{
        InitializeComponent();

        intensitylist.RefreshCommand = new Command(() => {
            intensitylist.IsRefreshing = true;
            Intensity_get();
        });

        Intensity_get();
	}

    private async void Intensity_get()
    {
        while (true)
        {
            if(Geoloc.app_window)
            {
                var items = new List<IntenItem>();
                intensitylist.ItemsSource = null;
                var inten_dict = new Dictionary<string, double>();
                var intens = MainPage.result;
                var intens_time = MainPage.targetTime_point;

                if (intens.Data != null)
                {
                    foreach (var point in intens.Data)
                    {
                        try
                        {
                            if (point.GetResultToIntensity() != null)
                                inten_dict.Add($"{point.ObservationPoint.Region} {point.ObservationPoint.Name}", (double)point.GetResultToIntensity());
                        }
                        catch
                        {
                            ;
                        }
                    }
                    var sortedValues = inten_dict.OrderByDescending(x => x.Value);
                    string image_name = null;
                    int n = 0;
                    foreach(KeyValuePair<string, double> kvp in sortedValues)
                    {
                        if(n<=50)
                        {
                            if (kvp.Value != 3)
                            {
                                if (kvp.Value < 0.5)
                                {
                                    image_name = "s_0.png";
                                }
                                else if (kvp.Value >= 0.5 && kvp.Value < 1.5)
                                {
                                    image_name = "s_1.png";
                                }
                                else if (kvp.Value >= 1.5 && kvp.Value < 2.5)
                                {
                                    image_name = "s_2.png";

                                }
                                else if (kvp.Value >= 2.5 && kvp.Value < 3.5)
                                {
                                    image_name = "s_3.png";
                                }
                                else if (kvp.Value >= 3.5 && kvp.Value < 4.5)
                                {
                                    image_name = "s_4.png";
                                }
                                else if (kvp.Value >= 4.5 && kvp.Value < 5.0)
                                {
                                    image_name = "s_5_1.png";
                                }
                                else if (kvp.Value >= 5.0 && kvp.Value < 5.5)
                                {
                                    image_name = "s_5_2.png";
                                }
                                else if (kvp.Value >= 5.5 && kvp.Value < 6.0)
                                {
                                    image_name = "s_6_1.png";
                                }
                                else if (kvp.Value >= 6.0 && kvp.Value < 6.5)
                                {
                                    image_name = "s_6_2.png";
                                }
                                else if (kvp.Value >= 6.5)
                                {
                                    image_name = "s_7.png";
                                }
                                var tmp = new List<string>() { image_name, kvp.Key, kvp.Value.ToString() };
                                items.Add(new IntenItem(tmp));
                            }
                        }
                        else
                            break;
                        n++;
                    }
                    intensitylist.ItemsSource = items;
                    intensity_time.Text = intens_time.ToString("yyyy/MM/dd HH:mm:ss");
                    intensitylist.IsRefreshing = false;
                }
            }
            else
            {
                break;
            }
            await Task.Delay(Geoloc.realtime_in);
        }
    }
}

public class IntenItem
{
    public IntenItem(List<string> list)
    {
        Inten_Image = ImageSource.FromFile(list[0]);
        Loc_Name = list[1];
        Inten_strings = list[2];
    }

    public ImageSource Inten_Image { get; set; }
    public string Loc_Name { get; set; }
    public string Inten_strings { get; set; }
}