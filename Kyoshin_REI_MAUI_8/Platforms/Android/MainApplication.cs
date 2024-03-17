using Android;
using Android.App;
using Android.Runtime;

[assembly: UsesPermission(Manifest.Permission.AccessCoarseLocation)]
[assembly: UsesPermission(Manifest.Permission.AccessFineLocation)]

[assembly: UsesPermission(Manifest.Permission.AccessNetworkState)]

[assembly: UsesPermission(Manifest.Permission.ForegroundService)]
[assembly: UsesPermission(Manifest.Permission.ForegroundServiceSpecialUse)]

[assembly: UsesPermission(Manifest.Permission.Internet)]

[assembly: UsesPermission(Manifest.Permission.PostNotifications)]

[assembly: UsesPermission(Manifest.Permission.ReadExternalStorage)]

[assembly: UsesPermission(Manifest.Permission.HighSamplingRateSensors)]

namespace Kyoshin_REI_MAUI_8
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
