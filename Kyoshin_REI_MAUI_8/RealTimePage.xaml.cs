namespace Kyoshin_REI_MAUI_8;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class RealTimePage : ContentPage
{
    public static double x = 0;
    public static double y = 0;
    public static double z = 0;
    public static double x_ = 0;
    public static double y_ = 0;
    public static double z_ = 0;
    public static double ma_x = 0;
    public static double ma_y = 0;
    public static double ma_z = 0;
    public static double gal;

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

        x_ = x * 0.1 + x_ * 0.9;
        y_ = y * 0.1 + y_ * 0.9;
        z_ = z * 0.1 + z_ * 0.9;
        x = x - x_;
        y = y - y_;
        z = z - z_;

        x_data.Text = x.ToString("0.00000");
        y_data.Text = y.ToString("0.00000");
        z_data.Text = z.ToString("0.00000");

        if (x < 0)
            ma_x = x * -1;
        else
            ma_x = x;
        if (y < 0)
            ma_y = y * -1;
        else
            ma_y = y;
        if (z < 0)
            ma_z = z * -1;
        else
            ma_z = z;

        //100gal = 1m/s

        double max = Max(ma_x, ma_y, ma_z);
        gal = max * 100;

        if (gal < 0.8)
            gal_inten.Text = $"êkìx0 {gal}gal";
        else if(gal < 2.5)
            gal_inten.Text = $"êkìx1 {gal}gal";
        else if(gal < 8)
            gal_inten.Text = $"êkìx2 {gal}gal";
        else if(gal < 25)
            gal_inten.Text = $"êkìx3 {gal}gal";
        else if(gal < 80)
            gal_inten.Text = $"êkìx4 {gal}gal";
        else if(gal < 165)
            gal_inten.Text = $"êkìx5é„ {gal}gal";
        else if(gal < 250)
            gal_inten.Text = $"êkìx5ã≠ {gal}gal";
        else if(gal < 325)
            gal_inten.Text = $"êkìx6é„ {gal}gal";
        else if(gal < 400)
            gal_inten.Text = $"êkìx6ã≠ {gal}gal";
        else if(gal >= 400)
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