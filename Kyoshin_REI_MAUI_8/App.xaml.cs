namespace Kyoshin_REI_MAUI_8
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            Window window = base.CreateWindow(activationState);

            window.Activated += (s, e) =>
            {
                Geoloc.app_window = true;
            };
            
            window.Resumed += (s, e) =>
            {
                Geoloc.app_window = true;
            };

            window.Deactivated += (s, e) =>
            {
                Geoloc.app_window = false;
            };

            window.Stopped += (s, e) =>
            {
                Geoloc.app_window = false;
            };

            window.Backgrounding += (s, e) =>
            {
                Geoloc.app_window = false;
            };
            return window;
        }
    }
}
