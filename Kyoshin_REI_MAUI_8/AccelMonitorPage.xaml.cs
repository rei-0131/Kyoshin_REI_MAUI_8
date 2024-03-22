using Kyoshin_REI_MAUI_8.ViewModels;

namespace Kyoshin_REI_MAUI_8;

public partial class AccelMonitorPage : ContentPage
{
    public static bool monitor_type_ = true;

    public AccelMonitorPage()
	{
		InitializeComponent();
	}

    private void monitor_type_Toggled(object sender, ToggledEventArgs e)
    {
        monitor_type_ = monitor_type.IsToggled;
    }
}
