using Kyoshin_REI_MAUI_8.ViewModels;

namespace Kyoshin_REI_MAUI_8;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class RealTimePage : ContentPage
{
    public static double x = 0;
    public static double y = 0;
    public static double z = 0;
    public static double co_x = 0;
    public static double co_y = 0;
    public static double co_z = 0;
    public static double ma_x = 0;
    public static double ma_y = 0;
    public static double ma_z = 0;
    public static double gal;

    private bool? isStreaming = false;

    public RealTimePage()
    {
        InitializeComponent();

        Sensor_Data();
    }

    private async void Sensor_Data()
    {
        if (Accelerometer.Default.IsSupported)
        {
            if (!Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
        }
    }

    private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        x = e.Reading.Acceleration.X;
        y = e.Reading.Acceleration.Y;
        z = e.Reading.Acceleration.Z;
        x_data.Text = (x + co_x).ToString("0.00000");
        y_data.Text = (y + co_y).ToString("0.00000");
        z_data.Text = (z + co_z).ToString("0.00000");

        if ((x + co_x) < 0)
            ma_x = (x + co_x) * -1;
        else
            ma_x = (x + co_x);
        if ((y + co_y) < 0)
            ma_y = (y + co_y) * -1;
        else
            ma_y = (y + co_y);
        if ((z + co_z) < 0)
            ma_z = (z + co_z) * -1;
        else
            ma_z = (z + co_z);

        //100gal = 1m/s

        double max = Max(ma_x, ma_y, ma_z);
        gal = max * 100;

        if (gal < 0.6)
            gal_inten.Text = $"êkìx0 {gal}gal";
        else if(gal < 1.9)
            gal_inten.Text = $"êkìx1 {gal}gal";
        else if(gal < 6)
            gal_inten.Text = $"êkìx2 {gal}gal";
        else if(gal < 19)
            gal_inten.Text = $"êkìx3 {gal}gal";
        else if(gal < 60)
            gal_inten.Text = $"êkìx4 {gal}gal";
        else if(gal < 110)
            gal_inten.Text = $"êkìx5é„ {gal}gal";
        else if(gal < 190)
            gal_inten.Text = $"êkìx5ã≠ {gal}gal";
        else if(gal < 340)
            gal_inten.Text = $"êkìx6é„ {gal}gal";
        else if(gal < 600)
            gal_inten.Text = $"êkìx6ã≠ {gal}gal";
        else if(gal >= 600)
            gal_inten.Text = $"êkìx7 {gal}gal";
    }

    public T Max<T>(params T[] nums) where T : IComparable
    {
        if (nums.Length == 0) return default(T);

        T max = nums[0];
        for (int i = 1; i < nums.Length; i++)
        {
            max = max.CompareTo(nums[i]) > 0 ? max : nums[i];

        }
        return max;
    }

    private void co_button_Clicked(object sender, EventArgs e)
    {
        co_x = x * -1;
        co_y = y * -1;
        co_z = z * -1;
    }

    private void ss_button_Clicked(object sender, EventArgs e)
    {
        if (Accelerometer.Default.IsSupported)
        {
            if (!Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
            else
            {
                Accelerometer.Default.Stop();
                Accelerometer.Default.ReadingChanged -= Accelerometer_ReadingChanged;
            }
        }
    }
}