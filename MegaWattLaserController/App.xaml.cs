using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
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
                Debug.WriteLine("InitializeComponent completed");

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

                // Register SerialPortManager using the public constructor
                services.AddSingleton<SerialPortManager>();

                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();

                // Register ViewModels
                services.AddSingleton<WaveformViewModel>();

                // Register all pages
                services.AddTransient<Views.EnergyMonitorPage>();
                services.AddTransient<Views.HomeScreen>();
                services.AddTransient<Views.InterlockStatusPage>();
                services.AddTransient<Views.SystemSettingsPage>();
                services.AddTransient<Views.EnergyPage>();
                services.AddTransient<Views.ShutterPage>();
                services.AddTransient<Views.PulseSettingsPage>();
                services.AddTransient<Views.WaveformPage>();
                services.AddTransient<Views.CustomPage>();
                services.AddTransient<Views.VoltagePage>();

                var provider = services.BuildServiceProvider();
                Debug.WriteLine("Services configured successfully");
                return provider;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Service configuration failed: {ex}");
                throw;
            }
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                Debug.WriteLine("=== OnLaunched Starting ===");

                if (Application.Current.Resources.MergedDictionaries.Count == 0)
                {
                    Debug.WriteLine("Adding XAML resources...");
                    Application.Current.Resources.MergedDictionaries.Add(new Microsoft.UI.Xaml.Controls.XamlControlsResources());
                    Debug.WriteLine("XAML resources added");
                }

                m_window = Services.GetRequiredService<MainWindow>();
                Debug.WriteLine("MainWindow retrieved from DI");

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