using Microsoft.Maui.Graphics.Platform;
using System.Reflection;

namespace Kyoshin_REI_MAUI_8;

public partial class KyoshinPage : ContentPage
{
	public KyoshinPage()
	{
		InitializeComponent();

        Kyoshin_Get();
	}

    private async void Kyoshin_Get()
    {
        ImageSource image_flo = null;
        ImageSource ps_image_ = null;
        ImageSource fore_image_ = null;

        back_image.Source = ImageSource.FromFile("back_image.png");
        over_image.Source = ImageSource.FromFile("over_image.png");

        while (true)
        {
            if(Geoloc.app_window)
            {
                var nowtime = DateTime.Now.AddSeconds(0);
               
                image_flo = ImageSource.FromUri(new Uri($"https://smi.lmoniexp.bosai.go.jp/data/map_img/RealTimeImg/jma_s/{nowtime.ToString("yyyyMMdd")}/{nowtime.ToString("yyyyMMddHHmmss")}.jma_s.gif"));

                if (MainPage.result_eew != null && MainPage.result_eew.Data.Result.Message != "ÉfÅ[É^Ç™Ç†ÇËÇ‹ÇπÇÒ")
                {
                    ps_image_ = ImageSource.FromUri(new Uri($"https://www.lmoni.bosai.go.jp/monitor/data/data/map_img/PSWaveImg/eew/{nowtime.ToString("yyyyMMdd")}/{nowtime.ToString("yyyyMMddHHmmss")}.eew.gif"));
                    fore_image_ = ImageSource.FromUri(new Uri($"https://smi.lmoniexp.bosai.go.jp/data/map_img/EstShindoImg/eew/{nowtime.ToString("yyyyMMdd")}/{nowtime.ToString("yyyyMMddHHmmss")}.eew.gif"));
                }

                fore_image.Source = fore_image_;
                quake_image.Source = image_flo;
                ps_image.Source = ps_image_;

            }
            await Task.Delay(Geoloc.kyoshin_in);
        }
    }
}