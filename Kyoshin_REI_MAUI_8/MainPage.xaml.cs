using KyoshinMonitorLib;
using KyoshinMonitorLib.ApiResult.WebApi;
using System.Text.RegularExpressions;
using Plugin.LocalNotification;
using KyoshinMonitorLib.SkiaImages;
using System.Diagnostics;

namespace Kyoshin_REI_MAUI_8
{
    static class Geoloc
    {
        public static double my_lat;
        public static double my_lon;
        public static int eew_in;
        public static int back_in;
        public static int kyoshin_in;
        public static int realtime_in;
        public static int gettime;
        public static int gettime_point;
        public static int ps_cal;
        public static int get_log;
        public static bool back_op;
        public static int off_kyoshin;
        public static bool off_ps;
        public static bool app_window = true;
    }

    public partial class MainPage : ContentPage
    {
        Dictionary<string, List<string>> Quake_Dict = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> Quake_tmp = new Dictionary<string, List<string>>();

        public static double my_lat = 35;
        public static double my_lon = 136;
        public static int p_seconds = 999;
        public static int s_seconds = 999;
        public static List<int> nums = new List<int>();
        public static ApiResult<Eew> result_eew;
        public static bool start = false;
        public static DateTime now_time_date;
        public static DateTime targetTime;
        public static DateTime targetTime_point;
        public static string file;
        public static bool clear_ac = false;
        public static bool p_bo = false;
        public static bool loc_auth = false;
        public static PointF nearestCoordinate;
        public static ApiResult<IEnumerable<KyoshinMonitorLib.SkiaImages.ImageAnalysisResult>> result;
        public static string file_path = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "ShindoObsPoints.mpk.lz4");

        internal static readonly string COUNT_KEY = "count";
        public static int count = 0;

        public MainPage()
        {
            InitializeComponent();

            Geoloc.my_lat = my_lat;
            Geoloc.my_lon = my_lon;

            for (int i = 0; i <= 1000; i++)
            {
                nums.Add(i);
            }
            Start();

            if(!Preferences.Default.ContainsKey("eew_in"))
            {
                Preferences.Default.Set("eew_in", 3000);
            }
            Geoloc.eew_in = Preferences.Default.Get("eew_in", 3000);

            if (!Preferences.Default.ContainsKey("realtime_in"))
            {
                Preferences.Default.Set("realtime_in", 3000);
            }
            Geoloc.realtime_in = Preferences.Default.Get("realtime_in", 3000);

            if (!Preferences.Default.ContainsKey("back_in"))
            {
                Preferences.Default.Set("back_in", 10000);
            }
            Geoloc.back_in = Preferences.Default.Get("back_in", 10000);

            if (!Preferences.Default.ContainsKey("kyoshin_in"))
            {
                Preferences.Default.Set("kyoshin_in", 3000);
            }
            Geoloc.kyoshin_in = Preferences.Default.Get("kyoshin_in", 1000);

            if (!Preferences.Default.ContainsKey("gettime"))
            {
                Preferences.Default.Set("gettime", -2);
            }
            Geoloc.gettime = Preferences.Default.Get("gettime", -2);

            if(!Preferences.Default.ContainsKey("gettime_point"))
            {
                Preferences.Default.Set("gettime_point", -2);
            }
            Geoloc.gettime_point = Preferences.Default.Get("gettime_point", -2);

            if (!Preferences.Default.ContainsKey("ps_cal"))
            {
                Preferences.Default.Set("ps_cal", 4);
            }
            Geoloc.ps_cal = Preferences.Default.Get("ps_cal", 2);

            if (!Preferences.Default.ContainsKey("get_log"))
            {
                Preferences.Default.Set("get_log", 50);
            }
            Geoloc.get_log = Preferences.Default.Get("get_log", 50);

            if(!Preferences.Default.ContainsKey("off_kyoshin"))
            {
                Preferences.Default.Set("off_kyoshin", -3);
            }
            Geoloc.off_kyoshin = Preferences.Default.Get("off_kyoshin", 50);

            if (!Preferences.Default.ContainsKey("back_op"))
            {
                Preferences.Default.Set("back_op", false);
            }
            Geoloc.back_op = Preferences.Default.Get("back_op", false);

            if(!Preferences.Default.ContainsKey("off_ps"))
            {
                Preferences.Default.Set("off_ps", true);
            }
            Geoloc.off_ps = Preferences.Default.Get("off_ps", true);
        }

        private async void Start()
        {
            using var reader = await FileSystem.OpenAppPackageFileAsync("tjma2001");
            using (var reader_ = new StreamReader(reader))
            {
                file = reader_.ReadToEnd();
            }
            if (!System.IO.File.Exists(file_path))
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                Debug.WriteLine("ShindoObsPoints.mpk.lz4のダウンロード中");
                wc.DownloadFile("https://github.com/ingen084/KyoshinEewViewerIngen/raw/develop/src/KyoshinEewViewer/Assets/ShindoObsPoints.mpk.lz4", file_path);
                wc.Dispose();
                Debug.WriteLine("ShindoObsPoints.mpk.lz4のダウンロード完了");
            }
            else
            {
                Debug.WriteLine("ShindoObsPoints.mpk.lz4の存在を確認");
            }
            TravelTimeTableConverter.ImportData();

            Per_req();
            Now_loc();

            await Task.Delay(1000);
            Geteew();
            GetPoint();
        }

        private async void Per_req()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            while(true)
            {
                status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status == PermissionStatus.Granted)
                {
                    loc_auth = true;
                    break;
                }
                await Task.Delay(1000);
            }
        }

        private void Now_button_Clicked(object sender, EventArgs e)
        {
            Preferences.Default.Set("gettime", -3);
            Geoloc.gettime = Preferences.Default.Get("gettime", -3);
        }

        private void Loc_button_click(object sender, EventArgs e)
        {
            Now_loc();
        }

        private async void Start_BackService()
        {
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
        }

        private async void Geteew()
        {
            Disp();

            Start_BackService();

            while (true)
            {
                if(Geoloc.app_window)
                {

                    IEnumerable<ConnectionProfile> profiles = Connectivity.Current.ConnectionProfiles;

                    if (profiles.Contains(ConnectionProfile.WiFi))
                    {
                        warning_msg.Text = "Wi-Fiを使用中";
                    }
                    else if (profiles.Contains(ConnectionProfile.Cellular))
                    {
                        warning_msg.Text = "モバイル通信を使用中";
                    }

                    try
                    {
                        using var webApi = new WebApi();
                        result_eew = await webApi.GetEewInfo(DateTime.Now.AddSeconds(Geoloc.gettime));
                        //System.Diagnostics.Debug.WriteLine(result_eew.Data.Result.Message);
                        webApi.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }

                    if (result_eew != null && result_eew.Data.Result.Message != "データがありません")
                    {
                        await Task.Delay(1000);
                    }
                    else
                    {
                        await Task.Delay(Geoloc.eew_in);
                    }
                }
                else
                {
                    await Task.Delay(Geoloc.eew_in);
                }
            }
        }

        private async void Disp()
        {
            ImageSource bitmap_null = ImageSource.FromFile("s_null.png");
            now_time_date = DateTime.Now;
            targetTime = now_time_date.AddSeconds(Geoloc.gettime);
            now_time.Text = now_time_date.ToString("yyyy/MM/dd HH:mm:ss");
            bool ch_red = false;
            while (true)
            {
                if(Geoloc.app_window)
                {
                    now_time_date = DateTime.Now;
                    targetTime = now_time_date.AddSeconds(Geoloc.gettime);
                    now_time.Text = now_time_date.ToString("yyyy/MM/dd HH:mm:ss");
                    //System.Diagnostics.Debug.WriteLine(targetTime.ToString("yyyy/MM/dd HH:mm:ss"));

                    if (result_eew != null)
                    {
                        var req_time = "";
                        try
                        {
                            var tmp_data = DateTime.ParseExact(result_eew.Data.RequestTime, "yyyyMMddHHmmss", null).ToString("yyyy/MM/dd HH:mm:ss");
                            req_time = result_eew.Data.RequestTime;
                        }
                        catch
                        {
                            req_time = targetTime.ToString("yyyyMMddHHmmss");
                        }
                        get_time.Text = DateTime.ParseExact(req_time, "yyyyMMddHHmmss", null).ToString("yyyy/MM/dd HH:mm:ss");
                        var dTime = DateTime.ParseExact(req_time, "yyyyMMddHHmmss", null);
                        if ((targetTime - dTime).TotalSeconds >= 10 && !ch_red)
                        {
                            get_time.TextColor = Microsoft.Maui.Graphics.Color.FromRgb(255, 0, 0);
                            ch_red = true;
                        }
                        else if ((targetTime - dTime).TotalSeconds < 10 && ch_red)
                        {
                            get_time.TextColor = Microsoft.Maui.Graphics.Color.FromRgb(255, 255, 255);
                            ch_red = false;
                        }

                        if (result_eew.Data.Result.Message != "データがありません")
                        {
                            if (clear_ac)
                            {
                                clear_ac = false;
                            }


                            if (Quake_Dict.ContainsKey(result_eew.Data.ReportId.ToString()) == false)
                            {
                                Quake_tmp.Add(result_eew.Data.ReportId.ToString(), new List<string>() { });
                                p_seconds = 999;
                                s_seconds = 999;

                                foreach (var n in Quake_tmp)
                                {
                                    Quake_Dict.Add(n.Key, n.Value);
                                }
                            }

                            if (Quake_Dict.ContainsKey(result_eew.Data.ReportId.ToString()))
                            {
                                bool tmp_Tr = false;
                                foreach (var n in Quake_Dict)
                                {
                                    if (n.Key == result_eew.Data.ReportId.ToString())
                                    {
                                        if (Quake_Dict[n.Key].Count != 0)
                                        {
                                            for (var x = 0; x < Quake_Dict[n.Key].Count; x++)
                                            {
                                                if (Quake_Dict[n.Key][x] == result_eew.Data.ReportNum.Value.ToString())
                                                {
                                                    tmp_Tr = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!tmp_Tr)
                                {
                                    int depth_int = result_eew.Data.Depth.Value;
                                    var dis = CalculateDistance(Geoloc.my_lat, Geoloc.my_lon, result_eew.Data.Location.Latitude, result_eew.Data.Location.Longitude);
                                    if (Quake_Dict[result_eew.Data.ReportId.ToString()].Count == 0)
                                    {
                                        Thread thread0 = new Thread(new ThreadStart(() =>
                                        {
                                            p_bo = false;
                                            (double, double) ps_item;

                                            foreach (int x in nums)
                                            {
                                                if (!p_bo)
                                                {
                                                    ps_item = TravelTimeTableConverter.GetValue(depth_int, x);
                                                    var p = ps_item.Item1;
                                                    if (p >= dis)
                                                    {
                                                        p_seconds = x;
                                                        p_bo = true;
                                                    }
                                                }
                                                else if (p_bo)
                                                {
                                                    ps_item = TravelTimeTableConverter.GetValue(depth_int, x);
                                                    var s = ps_item.Item2;
                                                    if (s >= dis)
                                                    {
                                                        s_seconds = x;
                                                        break;
                                                    }
                                                }
                                            }
                                        }));
                                        thread0.Start();
                                    }
                                    else if (Quake_Dict[result_eew.Data.ReportId.ToString()].Count % Geoloc.ps_cal == 0)
                                    {
                                        Thread thread = new Thread(new ThreadStart(() =>
                                        {
                                            p_bo = false;
                                            (double, double) ps_item;

                                            foreach (int x in nums)
                                            {
                                                if (!p_bo)
                                                {
                                                    ps_item = TravelTimeTableConverter.GetValue(depth_int, x);
                                                    var p = ps_item.Item1;
                                                    if (p >= dis)
                                                    {
                                                        p_seconds = x;
                                                        p_bo = true;
                                                    }
                                                }
                                                else if (p_bo)
                                                {
                                                    ps_item = TravelTimeTableConverter.GetValue(depth_int, x);
                                                    var s = ps_item.Item2;
                                                    if (s >= dis)
                                                    {
                                                        s_seconds = x;
                                                        break;
                                                    }
                                                }
                                            }
                                        }));
                                        thread.Start();
                                    }
                                    Quake_Dict[result_eew.Data.ReportId.ToString()].Add(result_eew.Data.ReportNum.Value.ToString());
                                    EEW_Status.Text = "緊急地震速報が発表されました";
                                    if (result_eew.Data.AlertFlag.ToString() == "予報")
                                    {
                                        EEW_Level.TextColor = Microsoft.Maui.Graphics.Color.FromRgb(255, 255, 0);
                                    }
                                    else if (result_eew.Data.AlertFlag.ToString() == "警報")
                                    {
                                        EEW_Level.TextColor = Microsoft.Maui.Graphics.Color.FromRgb(255, 0, 0);
                                    }
                                    EEW_Level.Text = result_eew.Data.AlertFlag.ToString();
                                    mag.Text = "M" + result_eew.Data.Magunitude.Value.ToString();
                                    region_point.Text = result_eew.Data.RegionName.ToString();

                                    if (result_eew.Data.IsFinal == true)
                                    {
                                        report_num.Text = "最終報";
                                    }
                                    else
                                    {
                                        report_num.Text = "#" + result_eew.Data.ReportNum.Value.ToString();
                                    }
                                    double intensity_dou = CalculateIntensity(depth_int, result_eew.Data.Magunitude.Value, dis);
                                    if (intensity_dou < 0.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_0.png");
                                        max_intensity.Source = bitmap;
                                    }
                                    else if (intensity_dou >= 0.5 && intensity_dou < 1.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_1.png");
                                        max_intensity.Source = bitmap;
                                    }
                                    else if (intensity_dou >= 1.5 && intensity_dou < 2.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_2.png");
                                        max_intensity.Source = bitmap;
                                    }
                                    else if (intensity_dou >= 2.5 && intensity_dou < 3.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_3.png");
                                        max_intensity.Source = bitmap;
                                    }
                                    else if (intensity_dou >= 3.5 && intensity_dou < 4.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_4.png");
                                        max_intensity.Source = bitmap;
                                    }
                                    else if (intensity_dou >= 4.5 && intensity_dou < 5.0)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_5_1.png");
                                        max_intensity.Source = bitmap;
                                    }
                                    else if (intensity_dou >= 5.0 && intensity_dou < 5.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_5_2.png");
                                        max_intensity.Source = bitmap;
                                    }
                                    else if (intensity_dou >= 5.5 && intensity_dou < 6.0)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_6_1.png");
                                        max_intensity.Source = bitmap;
                                    }
                                    else if (intensity_dou >= 6.0 && intensity_dou < 6.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_6_2.png");
                                        max_intensity.Source = bitmap;
                                    }
                                    else if (intensity_dou >= 6.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_7.png");
                                        max_intensity.Source = bitmap;
                                    }
                                }
                                if(Geoloc.off_ps)
                                {
                                    var mai_second_be = (DateTime.Now - DateTime.ParseExact(result_eew.Data.ReportId.ToString(), "yyyyMMddHHmmss", null)).TotalSeconds;
                                    int mai_second = Convert.ToInt32(mai_second_be);
                                    if (p_seconds == 999)
                                    {
                                        p_second.Text = "計算中";
                                    }
                                    else if ((p_seconds - mai_second) < 0)
                                    {
                                        p_second.Text = "0秒";
                                    }
                                    else
                                    {
                                        p_second.Text = (p_seconds - mai_second) + "秒";
                                    }

                                    if (s_seconds == 999)
                                    {
                                        s_second.Text = "計算中";
                                    }
                                    else if ((s_seconds - mai_second) < 0)
                                    {
                                        s_progressbar.Progress = 1;
                                        s_second.Text = "0秒";
                                    }
                                    else
                                    {
                                        s_progressbar.Progress = mai_second / s_seconds;
                                        s_second.Text = (s_seconds - mai_second) + "秒";
                                    }
                                }
                                else
                                {
                                    var mai_second_be = (now_time_date - DateTime.ParseExact(result_eew.Data.ReportId.ToString(), "yyyyMMddHHmmss", null)).TotalSeconds;
                                    int mai_second = Convert.ToInt32(mai_second_be);

                                    if (p_seconds == 999)
                                    {
                                        p_second.Text = "計算中";
                                    }
                                    else if ((p_seconds - mai_second) < 0)
                                    {
                                        p_second.Text = "0秒";
                                    }
                                    else
                                    {
                                        p_second.Text = (p_seconds - mai_second) + "秒";
                                    }

                                    if (s_seconds == 999)
                                    {
                                        s_second.Text = "計算中";
                                    }
                                    else if ((s_seconds - mai_second) < 0)
                                    {
                                        s_progressbar.Progress = 1;
                                        s_second.Text = "0秒";
                                    }
                                    else
                                    {
                                        s_progressbar.Progress = mai_second / s_seconds;
                                        s_second.Text = (s_seconds - mai_second) + "秒";
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!clear_ac)
                            {
                                clear_ac = true;
                                Quake_Dict.Clear();
                                Quake_tmp.Clear();
                                EEW_Status.Text = "緊急地震速報は発表されていません";
                                EEW_Level.Text = "--";
                                report_num.Text = "#--";
                                region_point.Text = "---------------";
                                mag.Text = "M-.-";
                                p_second.Text = "--秒";
                                s_second.Text = "--秒";
                                s_progressbar.Progress = 0;
                                EEW_Level.TextColor = Microsoft.Maui.Graphics.Color.FromRgb(255, 255, 255);
                                max_intensity.Source = bitmap_null;
                                //max_intensity.Source = ImageSource.FromFile("s_7.png");
                            }
                        }
                    }
                    else
                    {
                        get_time.TextColor = Microsoft.Maui.Graphics.Color.FromRgb(255, 0, 0);
                        ch_red = true;
                    }
                }
                await Task.Delay(1000);
            }
        }
        
        private async Task GetPoint()
        {
            ObservationPoint[] points = ObservationPoint.LoadFromMpk(file_path, true);
            Disp_in();
            await Task.Delay(1000);
            while (true)
            {
                if(Geoloc.app_window)
                {
                    try
                    {
                        using var webApi = new WebApi();
                        targetTime_point = DateTime.Now.AddSeconds(Geoloc.gettime_point);
                        result = await webApi.ParseScaleFromParameterAsync(points, targetTime_point);
                        webApi.Dispose();
                    }
                    catch (Exception ex)
                    {
                        /*point_label.Text = ex.ToString();*/
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }
                await Task.Delay(Geoloc.realtime_in);
            }
        }

        private async Task Disp_in()
        {
            flo_time.Text = "Now Loading.";
            await Task.Delay(1000);
            var coordinates = new List<PointF> { };
            flo_time.Text = "Now Loading..";

            try
            {
                bool result_tf = false;
                while(!result_tf)
                {
                    if (result != null)
                    {
                        if (result.Data != null)
                        {
                            foreach (var point in result.Data)
                            {
                                if (point.AnalysisResult == null)
                                {
                                    continue;
                                }

                                coordinates.Add(new PointF(point.ObservationPoint.Location.Latitude, point.ObservationPoint.Location.Longitude));
                            }
                            result_tf = true;
                        }
                        else
                        {
                            await Task.Delay(Geoloc.realtime_in);
                        }
                    }
                    else
                    {
                        await Task.Delay(Geoloc.realtime_in);
                    }
                }
            }
            catch (Exception ex)
            {
                flo_time.Text = ex.ToString();
                System.Diagnostics.Debug.WriteLine(ex);
            }

            flo_time.Text = "Now Loading...";

            var distances = coordinates.Select(coordinate =>
            {
                var diffX = coordinate.X - Geoloc.my_lat;
                var diffY = coordinate.Y - Geoloc.my_lon;

                var distance = Math.Sqrt(diffX * diffX + diffY * diffY);

                return distance;
            }).ToList();

            flo_time.Text = "Now Loading....";

            try
            {
                nearestCoordinate = coordinates.OrderBy(coordinate => distances[coordinates.IndexOf(coordinate)]).First();
            }
            catch (Exception ex)
            {
                flo_time.Text = ex.ToString();
                System.Diagnostics.Debug.WriteLine(ex);
            }

            flo_time.Text = "Now Loading.....";

            while (true)
            {
                if(Geoloc.app_window)
                {
                    try
                    {
                        if (result.Data != null)
                        {
                            flo_time.Text = targetTime_point.ToString("yyyy/MM/dd HH:mm:ss");
                            foreach (var point in result.Data)
                            {
                                if (nearestCoordinate.X == point.ObservationPoint.Location.Latitude && nearestCoordinate.Y == point.ObservationPoint.Location.Longitude)
                                {
                                    open_point.Text = point.ObservationPoint.Region;
                                    point_label.Text = point.ObservationPoint.Name;
                                    double keisoku = Math.Round((double)point.GetResultToIntensity(), 2);
                                    intensity_flo.Text = keisoku.ToString("0.00");
                                    intensity_bar.BackgroundColor = Color.FromRgba(point.Color.Red,point.Color.Green,point.Color.Blue, point.Color.Alpha);

                                    if (keisoku < 0.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_0.png");   
                                        point_sindo.Source = bitmap;
                                    }
                                    else if (keisoku >= 0.5 && keisoku < 1.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_1.png");
                                        point_sindo.Source = bitmap;
                                    }
                                    else if (keisoku >= 1.5 && keisoku < 2.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_2.png");
                                        point_sindo.Source = bitmap;

                                    }
                                    else if (keisoku >= 2.5 && keisoku < 3.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_3.png");
                                        point_sindo.Source = bitmap;
                                    }
                                    else if (keisoku >= 3.5 && keisoku < 4.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_4.png");
                                        point_sindo.Source = bitmap;
                                    }
                                    else if (keisoku >= 4.5 && keisoku < 5.0)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_5_1.png");
                                        point_sindo.Source = bitmap;
                                    }
                                    else if (keisoku >= 5.0 && keisoku < 5.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_5_2.png");
                                        point_sindo.Source = bitmap;
                                    }
                                    else if (keisoku >= 5.5 && keisoku < 6.0)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_6_1.png");
                                        point_sindo.Source = bitmap;
                                    }
                                    else if (keisoku >= 6.0 && keisoku < 6.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_6_2.png");
                                        point_sindo.Source = bitmap;
                                    }
                                    else if (keisoku >= 6.5)
                                    {
                                        ImageSource bitmap = ImageSource.FromFile("s_7.png");
                                        point_sindo.Source = bitmap;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }
                await Task.Delay(Geoloc.realtime_in);
            }
        }

        private async void Now_loc()
        {
            var request_ = new NotificationRequest
            {
                NotificationId = 1,
                Title = "Kyoshin_Rei",
                Subtitle = "情報",
                Description = "位置情報の取得中",
                BadgeNumber = 0,
                Android =
                {
                    ChannelId = "Location_Notice"
                }
            };
            LocalNotificationCenter.Current.Show(request_);
            while(!loc_auth)
            {
                await Task.Delay(1000);
            }
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    loc_Status.Text = $"{location.Latitude} {location.Longitude}";
                    System.Console.WriteLine($"{location.Latitude} {location.Longitude}");
                    Geoloc.my_lat = location.Latitude;
                    Geoloc.my_lon = location.Longitude;

                    request_ = new NotificationRequest
                    {
                        NotificationId = 1,
                        Title = "Kyoshin_Rei",
                        Subtitle = "情報",
                        Description = "位置情報の取得成功",
                        BadgeNumber = 0,
                        Android =
                        {
                            ChannelId = "Location_Notice"
                        }
                    };
                    LocalNotificationCenter.Current.Show(request_);

                    var coordinates = new List<PointF> { };

                    try
                    {
                        bool result_tf = false;
                        while (!result_tf)
                        {
                            if (result != null)
                            {
                                if (result.Data != null)
                                {
                                    foreach (var point in result.Data)
                                    {
                                        if (point.AnalysisResult == null)
                                        {
                                            continue;
                                        }

                                        coordinates.Add(new PointF(point.ObservationPoint.Location.Latitude, point.ObservationPoint.Location.Longitude));
                                    }
                                    result_tf = true;
                                }
                                else
                                {
                                    await Task.Delay(Geoloc.realtime_in);
                                }
                            }
                            else
                            {
                                await Task.Delay(Geoloc.realtime_in);
                            }
                        }
                        var distances = coordinates.Select(coordinate =>
                        {
                            var diffX = coordinate.X - Geoloc.my_lat;
                            var diffY = coordinate.Y - Geoloc.my_lon;

                            var distance = Math.Sqrt(diffX * diffX + diffY * diffY);

                            return distance;
                        }).ToList();

                        nearestCoordinate = coordinates.OrderBy(coordinate => distances[coordinates.IndexOf(coordinate)]).First();
                    }
                    catch (Exception ex)
                    {
                        flo_time.Text = ex.ToString();
                    }
                }
            }
            catch (FeatureNotSupportedException)
            {
                loc_Status.Text = "Not Supported";
                System.Console.WriteLine("Not Supported");
                request_ = new NotificationRequest
                {
                    NotificationId = 1,
                    Title = "Kyoshin_Rei",
                    Subtitle = "情報",
                    Description = "位置情報の取得はサポートしていません",
                    BadgeNumber = 0,
                    Android =
                    {
                        ChannelId = "Location_Notice"
                    }
                };
                LocalNotificationCenter.Current.Show(request_);
            }
            catch (FeatureNotEnabledException)
            {
                loc_Status.Text = "Not Enabled";
                System.Console.WriteLine("Not Enabled");
                request_ = new NotificationRequest
                    {
                        NotificationId = 1,
                        Title = "Kyoshin_Rei",
                        Subtitle = "情報",
                        Description = "位置情報取得の権限がありません",
                        BadgeNumber = 0,
                        Android =
                        {
                            ChannelId = "Location_Notice"
                        }
                };
                    LocalNotificationCenter.Current.Show(request_);
            }
            catch (PermissionException)
            {
                loc_Status.Text = "No Permission";
                System.Console.WriteLine("No Permission");
                request_ = new NotificationRequest
                {
                    NotificationId = 1,
                    Title = "Kyoshin_Rei",
                    Subtitle = "情報",
                    Description = "位置情報の取得失敗",
                    BadgeNumber = 0,
                    Android =
                    {
                        ChannelId = "Location_Notice"
                    }
                };
                LocalNotificationCenter.Current.Show(request_);
            }
            catch (Exception)
            {
                loc_Status.Text = "Grr Error";
                System.Console.WriteLine("Grr Error");
                request_ = new NotificationRequest
                {
                    NotificationId = 1,
                    Title = "Kyoshin_Rei",
                    Subtitle = "情報",
                    Description = "位置情報の取得失敗",
                    BadgeNumber = 0,
                    Android =
                    {
                        ChannelId = "Location_Notice"
                    }
                };
                LocalNotificationCenter.Current.Show(request_);
            }
        }

        public static double CalculateDistance(double lat1, double lon1, float lat2, float lon2)
        {
            double radius = 6371;

            double dLat = (lat2 - lat1) * Math.PI / 180f;

            double dLon = (lon2 - lon1) * Math.PI / 180f;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1 * Math.PI / 180f) * Math.Cos(lat2 * Math.PI / 180f) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return radius * c;
        }

        private class TravelTimeTableConverter
        {
            public static void ImportData()
            {
                _table = Regex.Replace(file, "\x20+", " ")
                  .Trim()
                  .Replace("\r", "")
                  .Split('\n')
                  .Select(x => x.Split(' '))
                  .Select(x =>
                      new TravelTimeTable
                      {
                          P = double.Parse(x[1]),
                          S = double.Parse(x[3]),
                          Depth = int.Parse(x[4]),
                          Distance = int.Parse(x[5])
                      })
                  .ToArray();
            }

            public static (double, double) GetValue(int depth, int time)
            {
                if (depth > 700 || time > 2000) return (double.NaN, double.NaN);

                var values = _table.Where(x => x.Depth == depth).ToArray();
                if (values.Length == 0) return (double.NaN, double.NaN);

                if (!p_bo)
                {
                    var p1 = values.Reverse().FirstOrDefault(x => x.P <= time);
                    var p2 = values.FirstOrDefault(x => x.P >= time);
                    if (p1 == null || p2 == null) return (double.NaN, double.NaN);
                    var p = (time - p1.P) / (p2.P - p1.P) * (p2.Distance - p1.Distance) + p1.Distance;

                    return (p, double.NaN);
                }

                if (p_bo)
                {
                    var s1 = values.Reverse().FirstOrDefault(x => x.S <= time);
                    var s2 = values.FirstOrDefault(x => x.S >= time);
                    if (s1 == null || s2 == null) return (double.NaN, double.NaN);
                    var s = (time - s1.S) / (s2.S - s1.S) * (s2.Distance - s1.Distance) + s1.Distance;

                    return (double.NaN, s);
                }

                return (double.NaN, double.NaN);
            }

            private static TravelTimeTable[] _table;

            private class TravelTimeTable
            {
                public double P { get; set; }
                public double S { get; set; }
                public int Depth { get; set; }
                public int Distance { get; set; }
            }
        }

        public static double CalculateIntensity(int depth, float magJMA, double dis)
        {
            double magW = magJMA - 0.171;

            double hypocenterDistance = Math.Sqrt(depth * depth + dis * dis) - Math.Pow(10, (0.5 * magW - 1.85)) / 2;

            hypocenterDistance = Math.Max(hypocenterDistance, 3);

            double gpv600 = Math.Pow(10, (0.58 * magW + 0.0038 * depth - 1.29 - Math.Log10(hypocenterDistance + 0.0028 * Math.Pow(10, (0.5 * magW))) - 0.002 * hypocenterDistance));

            double pgv400 = gpv600 * 1.31;

            double intensity = 2.68 + 1.72 * Math.Log10(pgv400);

            return intensity;
        }
    }
}
