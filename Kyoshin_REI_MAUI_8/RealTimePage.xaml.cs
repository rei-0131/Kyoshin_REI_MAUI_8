using Android.Widget;

namespace Kyoshin_REI_MAUI_8;

public partial class RealTimePage : ContentPage
{
    public static double x = 0;
    public static double y = 0;
    public static double z = 0;
    public static double co_x = 0;
    public static double co_y = 0;
    public static double co_z = 0;
    public RealTimePage()
    {
        InitializeComponent();

        Sensor_Data();
    }

    //äJén/èIóπÉ{É^Éìí«â¡

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
        x_data.Text = $"X: {x + co_x}";
        y_data.Text = $"Y: {y + co_y}";
        z_data.Text = $"Z: {z + co_z}";

        //100gal = 1m/s
        double max = Max(x + co_x, y + co_y, z + co_z);
        double gal = max * 100;

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