using MathNet.Numerics.IntegralTransforms;

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
    public static double sum_x = 0;
    public static double sum_y = 0;
    public static double sum_z = 0;
    public static double T = 0.02;
    public static int len = 150;
    public static int length = 0;
    public static double intensity = 0;
    float[] x_list = new float[len];
    float[] y_list = new float[len];
    float[] z_list = new float[len];

    public RealTimePage()
    {
        InitializeComponent();

        Sensor_Data();
    }

    private void Sensor_Data()
    {
        if (Accelerometer.Default.IsSupported)
        {
            if (!Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.Game);

                //Default ÇÕ 200 É~ÉäïbÇÃä‘äuÇégópÇµÇ‹Ç∑ÅB
                //UI ÇÕ 60 É~ÉäïbÇÃä‘äuÇégópÇµÇ‹Ç∑ÅB
                //16.666666666Hz
                //0.09359444774846233
                //Game ÇÕ 20 É~ÉäïbÇÃä‘äuÇégópÇµÇ‹Ç∑ÅB
                //50hz
                //0.0005989956095164629
                //Fastest ÇÕ 5 É~ÉäïbÇÃä‘äuÇégópÇµÇ‹Ç∑ÅB
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
        sum_x = x - x_;
        sum_y = y - y_;
        sum_z = z - z_;

        x_list[length] = Convert.ToSingle(sum_x * 100);
        y_list[length] = Convert.ToSingle(sum_y * 100);
        z_list[length] = Convert.ToSingle(sum_z * 100);

        length++;
        if (length == 150)
        {
            Fourier.ForwardReal(x_list, len - 2);
            Fourier.ForwardReal(y_list, len - 2);
            Fourier.ForwardReal(z_list, len - 2);
            var filter = Filter(len, T);

            var filter_x = Ap_Filter(x_list, filter);
            var filter_y = Ap_Filter(y_list, filter);
            var filter_z = Ap_Filter(z_list, filter);

            Fourier.InverseReal(filter_x, len - 2);
            Fourier.InverseReal(filter_y, len - 2);
            Fourier.InverseReal(filter_z, len - 2);

            List<double> comp_xyz = new();
            for (int i = 0; i < len; i++)
            {
                comp_xyz.Add(Math.Sqrt(Math.Pow(Convert.ToSingle(filter_x[i]), 2) + Math.Pow(Convert.ToSingle(filter_y[i]), 2) + Math.Pow(Convert.ToSingle(filter_z[i]), 2)));
            }
            double a = Search_Aval(comp_xyz, T);

            intensity = Math.Floor(Math.Round(2 * Math.Log10(a) + 0.94, 4, MidpointRounding.AwayFromZero) * 100) / 100;

            x_list = new float[len];
            y_list = new float[len];
            z_list = new float[len];
            length = 0;
        }


        x_data.Text = sum_x.ToString("0.00000");
        y_data.Text = sum_y.ToString("0.00000");
        z_data.Text = sum_z.ToString("0.00000");

        //100gal = 1m/s

        if (intensity < 0.5)
            gal_inten.Text = $"êkìx0 {intensity.ToString("0.00")}gal";
        else if(intensity < 1.5)
            gal_inten.Text = $"êkìx1 {intensity.ToString("0.00")}gal";
        else if(intensity < 2.5)
            gal_inten.Text = $"êkìx2 {intensity.ToString("0.00")}gal";
        else if(intensity < 3.5)
            gal_inten.Text = $"êkìx3 {intensity.ToString("0.00")}gal";
        else if(intensity < 4.5)
            gal_inten.Text = $"êkìx4 {intensity.ToString("0.00")}gal";
        else if(intensity < 5)
            gal_inten.Text = $"êkìx5é„ {intensity.ToString("0.00")}gal";
        else if(intensity < 5.5)
            gal_inten.Text = $"êkìx5ã≠ {intensity.ToString("0.00")}gal";
        else if(intensity < 6)
            gal_inten.Text = $"êkìx6é„ {intensity.ToString("0.00")}gal";
        else if(intensity < 6.5)
            gal_inten.Text = $"êkìx6ã≠ {intensity.ToString("0.00")}gal";
        else if(intensity >= 6.5)
            gal_inten.Text = $"êkìx7 {intensity.ToString("0.00")}gal";
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

    private static float[] Ap_Filter(float[] input, List<float> filter)
    {
        for (int i = 0; i < filter.Count; i++)
        {
            //input[i] = input[i].Magnitude * filter[i];
            input[i] *= filter[i];
        }

        return input;
    }

    private static double Search_Aval(List<double> a, double Ts)
    {
        double aval = 2000;
        double T_ref = 0.3;
        double epsilon = T_ref * 0.001;

        while (true)
        {
            double count = 0;
            for (int i = 0; i < a.Count; i++)
            {
                //Console.WriteLine($"{aval},{a[i]}");
                if (a[i] >= aval)
                {
                    count++;
                }
            }
            double T_above_aval = count * Ts;
            //Console.WriteLine($"{aval},{T_above_aval}");
            if (T_above_aval < (T_ref - epsilon))
            {
                aval -= aval / 2;
                continue;
            }

            if (T_above_aval > (T_ref + epsilon))
            {
                aval += aval / 2;
                continue;
            }

            break;
        }

        return aval;
    }

    private static List<float> Filter(int len, double Ts)
    {
        var N = len;
        var k = new List<int>();
        var f = new List<double>();
        var epsilon = 0.0001;
        var W_pe = new List<double>();
        var x = new List<double>();
        var W_hc = new List<double>();
        var W_lc = new List<double>();
        var filter = new List<float>();
        for (int i = 0; i < N; i++)
        {
            k.Add(i);
            f.Add(k[i] / (N * Ts * 2));
            W_pe.Add(Math.Sqrt(1 / (f[i] + epsilon)));
            x.Add(f[i] / 10);
            W_hc.Add(1 / Math.Sqrt(1 + 0.694 * Math.Pow(x[i], 2) +
                0.241 * Math.Pow(x[i], 4) +
                0.0557 * Math.Pow(x[i], 6) +
                0.009664 * Math.Pow(x[i], 8) +
                0.00134 * Math.Pow(x[i], 10) +
                0.000155 * Math.Pow(x[i], 12)));
            W_lc.Add(Math.Sqrt(1 - Math.Exp(-Math.Pow(f[i] / 0.5, 3))));

            filter.Add(Convert.ToSingle(W_pe[i] * W_hc[i] * W_lc[i]));
        }

        return filter;
    }
}