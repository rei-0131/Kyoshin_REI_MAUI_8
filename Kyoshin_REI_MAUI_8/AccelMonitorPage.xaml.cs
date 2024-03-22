using Kyoshin_REI_MAUI_8.ViewModels;

namespace Kyoshin_REI_MAUI_8;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AccelMonitorPage : ContentPage
{
    private bool? isStreaming = false;
    public static bool monitor_type_ = true;

    public AccelMonitorPage()
	{
		InitializeComponent();

        Update_xyz();
	}

    private async void Update_xyz()
    {
        var vm = (ViewModel)BindingContext;

        isStreaming = isStreaming is null ? true : !isStreaming;

        while (isStreaming.Value)
        {
            if(Geoloc.app_window)
            {
                vm.RemoveItem();
                vm.AddItem();
                await Task.Delay(250);
            }
            else
                await Task.Delay(1000);
        }
    }

    private void monitor_type_Toggled(object sender, ToggledEventArgs e)
    {
        monitor_type_ = monitor_type.IsToggled;
    }
}
