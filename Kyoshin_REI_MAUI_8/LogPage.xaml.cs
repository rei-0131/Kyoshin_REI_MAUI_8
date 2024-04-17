using CommunityToolkit.Maui.Alerts;
using System.Text.RegularExpressions;
using AngleSharp;
using System.Diagnostics;

namespace Kyoshin_REI_MAUI_8;

public partial class LogPage : ContentPage
{
    public static List<List<string>> quake_list = new List<List<string>>();
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public LogPage()
	{
		InitializeComponent();

        loglist.RefreshCommand = new Command(() => {
            loglist.IsRefreshing = true;
            Log_get();
        });

        Log_get();
    }

    private async void loglist_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var select = e.SelectedItemIndex;
        try
        {
            if (Convert.ToInt64((DateTime.Now - DateTime.ParseExact(quake_list[select][1], "yyyy/MM/dd HH:mm:ss", null)).TotalSeconds) < 86400)
            {
                Geoloc.gettime = Convert.ToInt32((DateTime.Now - DateTime.ParseExact(quake_list[select][1], "yyyy/MM/dd HH:mm:ss", null)).TotalSeconds) * -1;
                string text = "地震を再生しました";
                var toast = Toast.Make(text);
                await toast.Show(cancellationTokenSource.Token);
                //await Shell.Current.GoToAsync("/Home");
            }
            else
            {
                string text = "1日以上前の地震のため再生出来ません";
                var toast = Toast.Make(text);
                await toast.Show(cancellationTokenSource.Token);
            }
        }
        catch(Exception)
        {
            ;
        }
    }

    private async void Log_get()
    {
        quake_list.Clear();
        var items = new List<DatasItem>();
        loglist.ItemsSource = null;

        string strUrl = "https://typhoon.yahoo.co.jp/weather/jp/earthquake/list/";
        var config = Configuration.Default.WithDefaultLoader().WithDefaultCookies();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(strUrl);
        var trNodes = document.QuerySelectorAll("tr");
        for (int i = 0; i < trNodes.Length; i++)
        {
            if (i != 0 && i != trNodes.Length - 1)
            {
                try
                {
                    var tdNodes = trNodes[i].GetElementsByTagName("td");
                    var day_str = tdNodes[0].QuerySelector("a").TextContent;
                    var year = Regex.Match(day_str, "(.*?)年").ToString();
                    var month = Regex.Match(day_str = day_str.Replace(year, ""), "(.*?)月").ToString();
                    var day = Regex.Match(day_str = day_str.Replace(month, ""), "(.*?)日").ToString();
                    var hour = Regex.Match(day_str = day_str.Replace(day + " ", ""), "(.*?)時").ToString();
                    var minute = Regex.Match(day_str.Replace(hour, ""), "(.*?)分").ToString();
                    year = year.Replace("年", "");
                    month = month.Replace("月", "");
                    day = day.Replace("日", "");
                    hour = hour.Replace("時", "");
                    minute = minute.Replace("分", "");
                    var day_id = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), 0).ToString("yyyy/MM/dd HH:mm:ss");
                    var location = tdNodes[1].TextContent;
                    var mag = tdNodes[2].TextContent;
                    var max_intensity = tdNodes[3].TextContent;
                    if (max_intensity == "---")
                    {
                        quake_list.Add(new List<string>() { "s_0.png", day_id, $"{location}   M{mag}" });
                    }
                    else if (max_intensity == "1")
                    {
                        quake_list.Add(new List<string>() { "s_1.png", day_id, $"{location}   M{mag}" });
                    }
                    else if (max_intensity == "2")
                    {
                        quake_list.Add(new List<string>() { "s_2.png", day_id, $"{location}   M{mag}" });
                    }
                    else if (max_intensity == "3")
                    {
                        quake_list.Add(new List<string>() { "s_3.png", day_id, $"{location}   M{mag}" });
                    }
                    else if (max_intensity == "4")
                    {
                        quake_list.Add(new List<string>() { "s_4.png", day_id, $"{location}   M{mag}" });
                    }
                    else if (max_intensity == "5弱")
                    {
                        quake_list.Add(new List<string>() { "s_5_1.png", day_id, $"{location}   M{mag}" });
                    }
                    else if (max_intensity == "5強")
                    {
                        quake_list.Add(new List<string>() { "s_5_2.png", day_id, $"{location}   M{mag}" });
                    }
                    else if (max_intensity == "6弱")
                    {
                        quake_list.Add(new List<string>() { "s_6_1.png", day_id, $"{location}   M{mag}" });
                    }
                    else if (max_intensity == "6強")
                    {
                        quake_list.Add(new List<string>() { "s_6_2.png", day_id, $"{location}   M{mag}" });
                    }
                    else if (max_intensity == "7")
                    {
                        quake_list.Add(new List<string>() { "s_7.png", day_id, $"{location}   M{mag}" });
                    }
                }
                catch
                {
                    ;
                }
            }
        }

        int n = 0;
        foreach (var item in quake_list)
        {
            if (n <= Geoloc.get_log)
            {
                items.Add(new DatasItem(item));
            }
            else
            {
                break;
            }
            n++;
        }
        loglist.ItemsSource = items;
        loglist.IsRefreshing = false;
    }
}

public class DatasItem
{
	public DatasItem(List<string> list)
	{
		Log_Image = ImageSource.FromFile(list[0]);
		ID = list[1];
		Log_strings = list[2];
	}

    public ImageSource Log_Image { get; set; }
	public string ID { get; set; }
	public string Log_strings { get; set; }
}