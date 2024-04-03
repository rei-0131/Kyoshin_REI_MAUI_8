using System.Diagnostics;
using System.Xml;

namespace Kyoshin_REI_MAUI_8;

public partial class TsunamiPage : ContentPage
{
    public static List<List<string>> tsunami_list = new List<List<string>>();

    public TsunamiPage()
	{
		InitializeComponent();

        tsunamilist.RefreshCommand = new Command(() => {
            tsunamilist.IsRefreshing = true;
            Tsunami_get();
        });

        Tsunami_get();
	}

    private async void Tsunami_get()
	{
        tsunami_list.Clear();
        var items = new List<TsunamiItem>();
        tsunamilist.ItemsSource = null;
        string strUrl = "https://www.data.jma.go.jp/developer/xml/feed/eqvol.xml";

        var xmlDoc = new XmlDocument();

        xmlDoc.Load(strUrl);
        XmlNode root = xmlDoc.DocumentElement;

        bool cnt = true;

        foreach (XmlNode childNode in root.ChildNodes)
        {
            if (childNode.Name == "entry" && cnt)
            {
                var url = childNode["id"].InnerText;
                if (url.Contains("VTSE"))
                {
                    xmlDoc.Load(url);
                    root = xmlDoc.DocumentElement;

                    foreach (XmlNode childNode1 in root["Body"]["Tsunami"]["Forecast"].ChildNodes)
                    {
                        if (childNode1.Name == "Item")
                        {
                            var name = childNode1["Area"]["Name"].InnerText;
                            var val = childNode1["Category"]["Kind"]["Name"].InnerText;
                            var val_col = "s_null.png";
                            Debug.WriteLine($"{name} {val}");
                            if(val.Contains("ëÂí√îgåxïÒ"))
                            {
                                val_col = "purple.png";
                            }
                            else if(val.Contains("í√îgåxïÒ"))
                            {
                                val_col = "red.png";
                            }
                            else if(val.Contains("í√îgíçà”ïÒ"))
                            {
                                val_col = "yellow.png";
                            }

                            string now = "";
                            if (childNode1["FirstHeight"] != null)
                            {
                                if (childNode1["FirstHeight"]["Condition"] != null)
                                    now = childNode1["FirstHeight"]["Condition"].InnerText;
                                else
                                {
                                    now = childNode1["FirstHeight"]["ArrivalTime"].InnerText;
                                    now = now.Replace("+09:00", "").Replace("T", " ").Replace("-", "/") + "ç†ìûíB";
                                }
                            }
                            var max_height = "ìûíBíÜ";
                            try
                            {
                                max_height = childNode1["MaxHeight"]["jmx_eb:TsunamiHeight"].Attributes["type"].Value + " " + childNode1["MaxHeight"]["jmx_eb:TsunamiHeight"].Attributes["description"].Value;
                            }
                            catch
                            {
                                ;
                            }
                            tsunami_list.Add(new List<string>() {val_col, $"{name} {val}",$"{now} {max_height}"});
                        }
                    }
                    cnt = false;
                }

            }
        }

        /*xmlDoc.Load("https://www.gpvweather.com/jmaxml-view.php?k=%E6%B4%A5%E6%B3%A2%E8%AD%A6%E5%A0%B1%E3%83%BB%E6%B3%A8%E6%84%8F%E5%A0%B1%E3%83%BB%E4%BA%88%E5%A0%B1a&p=%E6%B0%97%E8%B1%A1%E5%BA%81&ym=2024-01&f=2024-01-01T07%3A22%3A32-20240101072233_0_VTSE41_010000.xml");
        root = xmlDoc.DocumentElement;

        foreach (XmlNode childNode1 in root["Body"]["Tsunami"]["Forecast"].ChildNodes)
        {
            if (childNode1.Name == "Item")
            {
                var name = childNode1["Area"]["Name"].InnerText;
                var val = childNode1["Category"]["Kind"]["Name"].InnerText;
                var val_col = "s_null.png";
                if (val.Contains("ëÂí√îgåxïÒ"))
                {
                    val_col = "purple.png";
                }
                else if (val.Contains("í√îgåxïÒ"))
                {
                    val_col = "red.png";
                }
                else if (val.Contains("í√îgíçà”ïÒ"))
                {
                    val_col = "yellow.png";
                }

                string now = "";
                if (childNode1["FirstHeight"] != null)
                {
                    if (childNode1["FirstHeight"]["Condition"] != null)
                        now = childNode1["FirstHeight"]["Condition"].InnerText;
                    else
                    {
                        now = childNode1["FirstHeight"]["ArrivalTime"].InnerText;
                        now = now.Replace("+09:00", "").Replace("T", " ").Replace("-", "/") + "ç†ìûíB";
                    }
                }

                var max_height = childNode1["MaxHeight"]["jmx_eb:TsunamiHeight"].Attributes["type"].Value + " " + childNode1["MaxHeight"]["jmx_eb:TsunamiHeight"].Attributes["description"].Value;

                tsunami_list.Add(new List<string>() { val_col, $"{name} {val}", $"{now} {max_height}" });
            }
        }
        cnt = false;*/

        if (!cnt)
        {
            foreach(var item in tsunami_list)
            {
                items.Add(new TsunamiItem(item));
            }
        }
        else
        {
            items.Add(new TsunamiItem(new List<string>() { "s_null.png", "åªç›ÅAí√îgèÓïÒÇÕî≠ï\Ç≥ÇÍÇƒÇ¢Ç‹ÇπÇÒ", " " }));
        }
        tsunamilist.ItemsSource = items;
        tsunamilist.IsRefreshing = false;
    }
}

public class TsunamiItem
{
    public TsunamiItem(List<string> list)
    {
        Tsunami_Image = ImageSource.FromFile(list[0]);
        Tsunami_title = list[1];
        Tsunami_strings = list[2];
    }

    public ImageSource Tsunami_Image { get; set; }
    public string Tsunami_title { get; set; }
    public string Tsunami_strings { get; set; }
}