using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using LaserControllerApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System.Diagnostics;

namespace LaserControllerApp
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        private Window m_window;

        public App()
        {
            try
            {
                Debug.WriteLine("=== App Constructor Starting ===");
                this.InitializeComponent();

                // Configure DI
                Services = ConfigureServices();
                Debug.WriteLine("DI container configured");

                this.UnhandledException += App_UnhandledException;
                Debug.WriteLine("Exception handler registered");
                Debug.WriteLine("=== App Constructor Completed ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! App constructor FAILED: {ex}");
                throw;
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            try
            {
                Debug.WriteLine("Configuring DI services...");
                var services = new ServiceCollection();

                // Get the main dispatcher
                var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

                // Register Services
                services.AddSingleton<SerialPortManager>(); // SerialPortManager now DI-friendly
                services.AddSingleton<StatusService>(provider => new StatusService(dispatcherQueue));
                services.AddSingleton<LoggerService>(provider => new LoggerService(dispatcherQueue));
                services.AddSingleton<LaserSettingsValidator>();

                // Register ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<WaveformViewModel>();
                services.AddSingleton<EnergyMonitorViewModel>();
                services.AddSingleton<PulseSettingsViewModel>();
                services.AddSingleton<VoltageMonitorViewModel>();
                services.AddSingleton<ShutterViewModel>();
                services.AddSingleton<CustomCommandsViewModel>();
                services.AddSingleton<InterlockStatusViewModel>();


                // Register MainWindow
                services.AddSingleton<MainWindow>();

                // Register Pages
                services.AddTransient<EnergyMonitorPage>();
                services.AddTransient<PulseSettingsPage>();
                services.AddTransient<VoltagePage>();
                services.AddTransient<ShutterPage>();
                services.AddTransient<WaveformPage>();
                services.AddTransient<InterlockStatusPage>();
                services.AddTransient<CustomPage>();
                services.AddTransient<EnergyPage>();
                services.AddTransient<SettingsPage>();

                var provider = services.BuildServiceProvider();

                // Initialize SerialPortManager with dispatcher
                var serialPortManager = provider.GetRequiredService<SerialPortManager>();
                serialPortManager.Initialize(dispatcherQueue);

                Debug.WriteLine("Services configured successfully");
                return provider;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Service configuration failed: {ex}");
                throw;
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                Debug.WriteLine("=== OnLaunched Starting ===");
                m_window = Services.GetRequiredService<MainWindow>();
                m_window.Activate();
                Debug.WriteLine("=== OnLaunched Completed ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! OnLaunched FAILED: {ex}");
                throw;
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"!!! Unhandled exception: {e.Exception}");
            e.Handled = true;
        }
    }
}
