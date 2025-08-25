using Microsoft.UI.Xaml;

namespace LaserControllerApp
{
    public partial class App : Application
    {
        public App()
        {
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}