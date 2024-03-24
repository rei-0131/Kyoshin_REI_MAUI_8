using MathNet.Numerics.IntegralTransforms;
using System.Diagnostics;
using System.Numerics;

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
    public static double intensity = 0;
    public static Complex[] x_list = new Complex[0];
    public static Complex[] y_list = new Complex[0];
    public static Complex[] z_list = new Complex[0];

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
        Array.Resize(ref x_list, x_list.Length + 1);
        x_list[x_list.Length - 1] = x * 100;
        Array.Resize(ref y_list, y_list.Length + 1);
        y_list[y_list.Length - 1] = y * 100;
        Array.Resize(ref z_list, z_list.Length + 1);
        z_list[z_list.Length - 1] = z * 100;

        if (x_list.Length == 150)
        {
            Fourier.Forward(x_list);
            Fourier.Forward(y_list);
            Fourier.Forward(z_list);

            var filter = Filter(0.02);

            var filter_x = Ap_Filter(x_list, filter);
            var filter_y = Ap_Filter(y_list, filter);
            var filter_z = Ap_Filter(z_list, filter);
            Fourier.Inverse(filter_x);
            Fourier.Inverse(filter_y);
            Fourier.Inverse(filter_z);
            List<double> comp_xyz = new();
            for (int i = 0; i < 150; i++)
            {
                comp_xyz.Add(Complex.Sqrt(Complex.Pow(filter_x[i], 2) + Complex.Pow(filter_y[i], 2) + Complex.Pow(filter_z[i], 2)).Magnitude);
            }
            double a = Search_Aval(comp_xyz, T);

            intensity = Math.Floor(Math.Round(2 * Math.Log10(a) + 0.94, 4, MidpointRounding.AwayFromZero) * 100) / 100;

            x_list = new Complex[0];
            y_list = new Complex[0];
            z_list = new Complex[0];
        }

        x_ = x * 0.1 + x_ * 0.9;
        y_ = y * 0.1 + y_ * 0.9;
        z_ = z * 0.1 + z_ * 0.9;
        sum_x = x - x_;
        sum_y = y - y_;
        sum_z = z - z_;

        x_data.Text = sum_x.ToString("0.00000");
        y_data.Text = sum_y.ToString("0.00000");
        z_data.Text = sum_z.ToString("0.00000");

        //100gal = 1m/s

        /*double max = Max(ma_x, ma_y, ma_z);
        gal = max * 100;*/

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

    private static Complex[] Ap_Filter(Complex[] input, List<double> filter)
    {
        for(int i = 0; i < input.Length; i++)
        {
            input[i] *= filter[i];
        }

        return input;
    }

    private static double Search_Aval(List<double> a,double Ts)
    {
        double aval = 2000;
        double T_ref = 0.3;
        double epsilon = T_ref * 0.001;

        while(true)
        {
            double count = 0;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] >= aval)
                {
                    count++;
                }
            }     
            double T_above_aval = count * Ts;

            if (T_above_aval < (T_ref - epsilon))
            {
                aval -= aval / 2;
                continue;
            }

            if(T_above_aval > (T_ref + epsilon))
            {
                aval += aval / 2;
                continue;
            }

            break;
        }

        return aval;
    }

    private static List<double> Filter(double Ts)
    {
        var N = x_list.Length;
        var k = new List<int>();
        var f = new List<double>();
        var epsilon = 0.0001;
        var W_pe = new List<double>();
        var x = new List<double>();
        var W_hc = new List<double>();
        var W_lc = new List<double>();
        var filter = new List<double>();
        for (int i = 0; i < N;i++)
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

            filter.Add(W_pe[i] * W_hc[i] * W_lc[i]);
        }

        return filter;
    }
}