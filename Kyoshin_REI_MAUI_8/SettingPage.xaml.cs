using CommunityToolkit.Maui.Alerts;

namespace Kyoshin_REI_MAUI_8;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{
		InitializeComponent();

		gettime_entry.Text = Geoloc.gettime.ToString();
        gettime_point_entry.Text = Geoloc.gettime_point.ToString();
		eew_interval_entry.Text = Geoloc.eew_in.ToString();
        realtime_interval_entry.Text = Geoloc.realtime_in.ToString();
        back_interval_entry.Text = Geoloc.back_in.ToString();
        kyoshin_interval_entry.Text = Geoloc.kyoshin_in.ToString();
		ps_entry.Text = Geoloc.ps_cal.ToString();
        kyoshin_entry.Text = Geoloc.off_kyoshin.ToString();
		log_entry.Text = Geoloc.get_log.ToString();
        off_ps.IsToggled = Geoloc.off_ps;
		back_op.IsToggled = Geoloc.back_op;
		var hour_traffic = 3600 / (Geoloc.realtime_in / 1000) * 550;
		if (hour_traffic < 1024)
		{
			//B
			traffic_label.Text = hour_traffic.ToString() + "B/h  ";
		}
		else if (hour_traffic >= 1024 && hour_traffic < 1048576)
		{
            //KB
            traffic_label.Text = (hour_traffic / 1024).ToString("0.000") + "KB/h  ";
        }
		else if (hour_traffic >= 1048576 && hour_traffic < 1073741824)
		{
            //MB
            traffic_label.Text = (hour_traffic / 1024 / 1024).ToString("0.000") + "MB/h  ";
        }
		else
		{
            //GB
            traffic_label.Text = (hour_traffic / 1024 / 1024 / 1024).ToString("0.000") + "GB/h  ";
        }
	}

	private async void setting_btn_click(object sender, EventArgs e)
	{
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        try
		{
            Geoloc.gettime = int.Parse(gettime_entry.Text);
            Geoloc.gettime_point = int.Parse(gettime_point_entry.Text);
            Geoloc.eew_in = int.Parse(eew_interval_entry.Text);
            Geoloc.realtime_in = int.Parse(realtime_interval_entry.Text);
#if ADNROID
            if(Geoloc.back_in != int.Parse(back_interval_entry.Text) && back_op.IsToggled)
            {
                var serviceInstance = new BackServices();
                serviceInstance.Stop();
                serviceInstance.Start();
            }
#endif
            Geoloc.back_in = int.Parse(back_interval_entry.Text);
            Geoloc.kyoshin_in = int.Parse(kyoshin_interval_entry.Text);
            Geoloc.ps_cal = int.Parse(ps_entry.Text);
            if (int.Parse(log_entry.Text) > 100)
            {
                Geoloc.get_log = 100;
                string text = "3時間以上前の地震のため再生出来ません";
                var toast = Toast.Make(text);
                await toast.Show(cancellationTokenSource.Token);
            }
			else
                Geoloc.get_log = int.Parse(log_entry.Text);
            Geoloc.off_kyoshin = int.Parse(kyoshin_entry.Text);
            KyoshinPage.add_sc = Geoloc.off_kyoshin;

            Geoloc.off_ps = off_ps.IsToggled;
			Geoloc.back_op = back_op.IsToggled;
			Preferences.Default.Set("gettime", Geoloc.gettime);
            Preferences.Default.Set("gettime_point", Geoloc.gettime_point);
			Preferences.Default.Set("eew_in", Geoloc.eew_in);
            Preferences.Default.Set("realtime_in", Geoloc.realtime_in);
            Preferences.Default.Set("back_in", Geoloc.back_in);
            Preferences.Default.Set("kyoshin_in", Geoloc.kyoshin_in);
			Preferences.Default.Set("ps_cal", Geoloc.ps_cal);
			Preferences.Default.Set("get_log", Geoloc.get_log);
            Preferences.Default.Set("off_kyoshin",Geoloc.off_kyoshin);
            Preferences.Default.Set("off_ps",Geoloc.off_ps);
			Preferences.Default.Set("back_op", Geoloc.back_op);
            var hour_traffic = 3600 / (Geoloc.eew_in / 1000) * 550;
            if (hour_traffic < 1024)
            {
                //B
                traffic_label.Text = hour_traffic.ToString() + "B/h  ";
            }
            else if (hour_traffic >= 1024 && hour_traffic < 1048576)
            {
                //KB
                traffic_label.Text = (hour_traffic / 1024).ToString("0.000") + "KB/h  ";
            }
            else if (hour_traffic >= 1048576 && hour_traffic < 1073741824)
            {
                //MB
                traffic_label.Text = (hour_traffic / 1024 / 1024).ToString("0.000") + "MB/h  ";
            }
            else
            {
                //GB
                traffic_label.Text = (hour_traffic / 1024 / 1024 / 1024).ToString("0.000") + "GB/h  ";
            }
            message_setting.Text = $"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}] 設定変更成功";
#if ANDROID
            if(Geoloc.back_op)
			{
                var serviceInstance = new BackServices();
                serviceInstance.Start();
            }
			else
			{
                var serviceInstance = new BackServices();
                serviceInstance.Stop();
            }
#endif
        }
		catch(Exception ex)
		{
			message_setting.Text = ex.Message;
		}
	}
}