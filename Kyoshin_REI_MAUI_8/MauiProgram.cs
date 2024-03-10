using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using CommunityToolkit.Maui;
using KyoshinMonitorLib;
using KyoshinMonitorLib.ApiResult.WebApi;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.EventArgs;

namespace Kyoshin_REI_MAUI_8
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseLocalNotification(config =>
                {
                    config.AddCategory(new NotificationCategory(NotificationCategoryType.Status)
                    {
                        ActionList = new HashSet<NotificationAction>(new List<NotificationAction>()
                        {
                            new NotificationAction(100)
                            {
                                Title = "バックグラウンド処理を終了",
                                Android =
                                {
                                    LaunchAppWhenTapped = true,
                                    IconName =
                                    {
                                        ResourceName = "i2"
                                    }
                                }
                            }
                        })
                    });
                    config.AddCategory(new NotificationCategory(NotificationCategoryType.Progress)
                    {
                        ActionList = new HashSet<NotificationAction>(new List<NotificationAction>()
                        {
                            new NotificationAction(101)
                            {
                                Title = "バックグラウンド処理を開始",
                                Android =
                                {
                                    LaunchAppWhenTapped = true,
                                    IconName =
                                    {
                                        ResourceName = "i2"
                                    }
                                }
                            }
                        })
                    });
                    config.AddAndroid(android =>
                    {
                        android.AddChannel(new NotificationChannelRequest
                        {
                            Id = "Location_Notice",
                            Name = "Location_Notice",
                            Description = "General"
                        });
                        android.AddChannel(new NotificationChannelRequest
                        {
                            Id = "BackGround_None_Notice",
                            Name = "BackGround_None_Notice",
                            Description = "General"
                        });
                        android.AddChannel(new NotificationChannelRequest
                        {
                            Id = "BackGround_Warning_Notice",
                            Name = "BackGround_Warning_Notice",
                            Description = "Special",
                            Sound = "early_warning"
                        });
                        android.AddChannel(new NotificationChannelRequest
                        {
                            Sound = "early_warning"
                        });
                    });
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            /// Add dependecy injection to main page
            builder.Services.AddSingleton<MainPage>();

#if ANDROID
            builder.Services.AddTransient<IServiceTest, BackServices>();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }

    public interface IServiceTest
    {
        void Start();
        void Stop();
    }

    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeSpecialUse)]
    public class BackServices : Service, IServiceTest
    {
        public ApiResult<Eew> result_eew;
        System.Timers.Timer timer_ = new System.Timers.Timer(Geoloc.back_in);
        Dictionary<string, List<string>> Quake_Dict = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> Quake_tmp = new Dictionary<string, List<string>>();
        bool clear_ac = false;

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            timer_.Elapsed += (sender, e) =>
            {
                Quake_get_timer();
            };

            if (intent.Action == "START_SERVICE")
            {
                NotificationChannel channel = new NotificationChannel("ServiceChannel", "BackService", NotificationImportance.Low);
                NotificationManager manager = (NotificationManager)MainActivity.ActivityCurrent.GetSystemService(Context.NotificationService);
                manager.CreateNotificationChannel(channel);
                Notification notification = new Notification.Builder(this, "ServiceChannel")
                       .SetContentTitle("バックグラウンド処理を開始")
                       .SetSmallIcon(Resource.Drawable.abc_ab_share_pack_mtrl_alpha)
                       .SetOngoing(true)
                       .SetAutoCancel(false)
                       .Build();

                StartForeground(10, notification, Android.Content.PM.ForegroundService.TypeSpecialUse);

                if (!timer_.Enabled)
                    timer_.Start();
            }
            else if (intent.Action == "STOP_SERVICE")
            {
                if(timer_.Enabled)
                    timer_.Stop();
                StopForeground(true);
                StopSelfResult(startId);
            }

            return StartCommandResult.NotSticky;
        }

        private async void NotificationActionTapped(NotificationActionEventArgs e)
        {
            LocalNotificationCenter.Current.NotificationActionTapped += NotificationActionTapped;

            switch (e.ActionId)
            {
                case 100:
                    Stop();

                    var request_ = new NotificationRequest
                    {
                        NotificationId = 102,
                        Title = "バックグラウンド処理を終了しました",
                        Subtitle = $"{DateTime.ParseExact(result_eew.Data.RequestTime, "yyyyMMddHHmmss", null).ToString("yyyy/MM/dd HH:mm:ss")}",
                        Description = "",
                        CategoryType = NotificationCategoryType.Progress,
                        Android =
                        {
                            IconSmallName =
                            {
                                ResourceName = "notify_icon"
                            },
                            ChannelId = "BackGround_None_Notice"
                        }
                    };
                    await LocalNotificationCenter.Current.Show(request_);
                    break;
                case 101:
                    Start();
                    break;
            }
        }

        private async void Quake_get_timer()
        {
            LocalNotificationCenter.Current.NotificationActionTapped += NotificationActionTapped;

            try
            {
                using var webApi = new WebApi();
                result_eew = await webApi.GetEewInfo(DateTime.Now.AddSeconds(Geoloc.gettime));
                webApi.Dispose();

                if(result_eew != null && result_eew.Data.Result.Message != "データがありません")
                {
                    if (clear_ac)
                    {
                        clear_ac = false;
                    }

                    if (Quake_Dict.ContainsKey(result_eew.Data.ReportId.ToString()) == false)
                    {
                        Quake_tmp.Add(result_eew.Data.ReportId.ToString(), new List<string>() { });
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
                            Quake_Dict[result_eew.Data.ReportId.ToString()].Add(result_eew.Data.ReportNum.Value.ToString());
                            int depth_int = result_eew.Data.Depth.Value;
                            var dis = MainPage.CalculateDistance(Geoloc.my_lat, Geoloc.my_lon, result_eew.Data.Location.Latitude, result_eew.Data.Location.Longitude);
                            double intensity_dou = MainPage.CalculateIntensity(depth_int, result_eew.Data.Magunitude.Value, dis);
                            string jma_str = "震度0";
                            if (intensity_dou >= 0.5 && intensity_dou < 1.5)
                                jma_str = "震度1";
                            else if (intensity_dou >= 1.5 && intensity_dou < 2.5)
                                jma_str = "震度2";
                            else if (intensity_dou >= 2.5 && intensity_dou < 3.5)
                                jma_str = "震度3";
                            else if (intensity_dou >= 3.5 && intensity_dou < 4.5)
                                jma_str = "震度4";
                            else if (intensity_dou >= 4.5 && intensity_dou < 5.0)
                                jma_str = "震度5弱";
                            else if (intensity_dou >= 5.0 && intensity_dou < 5.5)
                                jma_str = "震度5強";
                            else if (intensity_dou >= 5.5 && intensity_dou < 6.0)
                                jma_str = "震度6弱";
                            else if (intensity_dou >= 6.0 && intensity_dou < 6.5)
                                jma_str = "震度6強";
                            else if (intensity_dou >= 6.5)
                                jma_str = "震度7";

                            string report_num = "";
                            if (result_eew.Data.IsFinal == true)
                                report_num = "最終報";
                            else
                                report_num = "#" + result_eew.Data.ReportNum.Value.ToString();

                            var request_ = new NotificationRequest
                            {
                                NotificationId = 100,
                                Title = "緊急地震速報が発表されました",
                                Subtitle = $"{DateTime.ParseExact(result_eew.Data.RequestTime, "yyyyMMddHHmmss", null).ToString("yyyy/MM/dd HH:mm:ss")}",
                                Description = $"{result_eew.Data.AlertFlag} 第{result_eew.Data.ReportNum}報 {result_eew.Data.RegionName} {result_eew.Data.MagunitudeString}\n予想震度: {jma_str}",
                                CategoryType = NotificationCategoryType.Status,
                                Android =
                                {
                                    IconSmallName =
                                    {
                                        ResourceName = "notify_icon"
                                    },
                                    ChannelId = "BackGround_Warning_Notice"
                                }
                            };
                            if (result_eew.Data.AlertFlag == "警報")
                                request_.Sound = DeviceInfo.Platform == DevicePlatform.Android ? "early_warning" : "early_warning.mp3";

                            await LocalNotificationCenter.Current.Show(request_);
                        }
                    }
                }
                else
                {
                    var request_ = new NotificationRequest
                    {
                        NotificationId = 101,
                        Title = "緊急地震速報は発表されていません。",
                        Subtitle = $"{DateTime.ParseExact(result_eew.Data.RequestTime, "yyyyMMddHHmmss", null).ToString("yyyy/MM/dd HH:mm:ss")}",
                        Description = "",
                        //Sound = DeviceInfo.Platform == DevicePlatform.Android ? "early_warning" : "early_warning.mp3",
                        CategoryType = NotificationCategoryType.Status,
                        Android =
                        {
                            IconSmallName =
                            {
                                ResourceName = "notify_icon"
                            },
                            ChannelId = "BackGround_None_Notice"
                        }
                    };
                    await LocalNotificationCenter.Current.Show(request_);

                    if(!clear_ac)
                    {
                        Quake_Dict.Clear();
                        Quake_tmp.Clear();
                        clear_ac = true;
                    }
                }
            }
            catch (Exception)
            {
                var request_ = new NotificationRequest
                {
                    NotificationId = 101,
                    Title = "緊急地震速報は発表されていません。",
                    Subtitle = "",
                    Description = "取得失敗",
                    CategoryType = NotificationCategoryType.Status,
                    Android =
                        {
                            IconSmallName =
                            {
                                ResourceName = "notify_icon"
                            },
                            ChannelId = "BackGround_None_Notice"
                        }
                };
                await LocalNotificationCenter.Current.Show(request_);
            }
        }

        public void Start()
        {
            Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(BackServices));
            startService.SetAction("START_SERVICE");
            MainActivity.ActivityCurrent.StartService(startService);
        }

        public void Stop()
        {
            Intent stopIntent = new Intent(MainActivity.ActivityCurrent, this.Class);
            stopIntent.SetAction("STOP_SERVICE");
            MainActivity.ActivityCurrent.StartService(stopIntent);
        }
    }
}
