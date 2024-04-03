using System.Diagnostics;

namespace Kyoshin_REI_MAUI_8;

public partial class KyoshinPage : ContentPage
{
    public static int add_sc = Geoloc.off_kyoshin;

	public KyoshinPage()
	{
		InitializeComponent();
        time_slider.Value = 0;
        time_label.Text = "0ï™";
        Kyoshin_Get();
	}

    private async void Kyoshin_Get()
    {
        ImageSource image_flo = null;
        ImageSource ps_image_ = null;
        ImageSource fore_image_ = null;

        back_image.Source = ImageSource.FromFile("back_image.png");
        over_image.Source = ImageSource.FromFile("over_image_ky.png");

        string now_over = "over_image_ky";

        while (true)
        {
            if(Geoloc.app_window)
            {
                if (now_over != "over_image_ky" && kyoshin_type.IsToggled)
                {
                    over_image.Source = ImageSource.FromFile("over_image_ky.png");
                    now_over = "over_image_ky";
                }
                else if(now_over == "over_image_ky" && !kyoshin_type.IsToggled)
                {
                    over_image.Source = ImageSource.FromFile("over_image_ty.png");
                    now_over = "over_image_ty";
                }

                var targettime = DateTime.Now.AddSeconds(add_sc);
                if (kyoshin_type.IsToggled)
                    image_flo = ImageSource.FromUri(new Uri($"http://www.kmoni.bosai.go.jp/data/map_img/RealTimeImg/jma_s/{targettime.ToString("yyyyMMdd")}/{targettime.ToString("yyyyMMddHHmmss")}.jma_s.gif"));
                else
                    image_flo = ImageSource.FromUri(new Uri($"https://www.lmoni.bosai.go.jp/monitor/data/data/map_img/RealTimeImg/abrspmx_s/{targettime.ToString("yyyyMMdd")}/{targettime.ToString("yyyyMMddHHmmss")}.abrspmx_s.gif"));
                if (MainPage.result_eew != null && MainPage.result_eew.Data.Result.Message != "ÉfÅ[É^Ç™Ç†ÇËÇ‹ÇπÇÒ")
                {
                    ps_image_ = ImageSource.FromUri(new Uri($"http://www.kmoni.bosai.go.jp/data/map_img/PSWaveImg/eew/{targettime.ToString("yyyyMMdd")}/{targettime.ToString("yyyyMMddHHmmss")}.eew.gif"));
                    fore_image_ = ImageSource.FromUri(new Uri($"http://www.kmoni.bosai.go.jp/data/map_img/EstShindoImg/eew/{targettime.ToString("yyyyMMdd")}/{targettime.ToString("yyyyMMddHHmmss")}.eew.gif"));
                }

                fore_image.Source = fore_image_;
                quake_image.Source = image_flo;
                ps_image.Source = ps_image_;

            }
            await Task.Delay(Geoloc.kyoshin_in);
        }
    }

    private void time_slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (Convert.ToInt32(e.NewValue) == 0)
            add_sc = Geoloc.off_kyoshin;
        else
            add_sc = Convert.ToInt32(e.NewValue) * -60;
        time_label.Text = $"{Convert.ToInt32(e.NewValue)}ï™";
    }
}