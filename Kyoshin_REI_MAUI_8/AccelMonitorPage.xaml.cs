using Kyoshin_REI_MAUI_8.ViewModels;

namespace Kyoshin_REI_MAUI_8;

public partial class AccelMonitorPage : ContentPage
{
    public static bool monitor_type_ = true;
    public static bool intensity_type_ = true;

    public AccelMonitorPage()
	{
		InitializeComponent();
        if (!monitor_type.IsToggled)
        {
            intensity_type.IsEnabled = true;
            sw_t.Text = "êkìx";
            sw_f.Text = "åvë™êkìx";
        }
        else
        {
            intensity_type.IsEnabled = false;
            sw_t.Text = "Gal";
            sw_f.Text = "m/s";
        }
    }

    private void monitor_type_Toggled(object sender, ToggledEventArgs e)
    {
        if (!monitor_type.IsToggled)
        {
            intensity_type.IsEnabled = true;
            sw_t.Text = "êkìx";
            sw_f.Text = "åvë™êkìx";
        }
        else
        {
            intensity_type.IsEnabled = false;
            sw_t.Text = "Gal";
            sw_f.Text = "m/s";
        }
        monitor_type_ = monitor_type.IsToggled;
    }

    private void intensity_type_Toggled(object sender, ToggledEventArgs e)
    {
        intensity_type_ = intensity_type.IsToggled;
    }
}
