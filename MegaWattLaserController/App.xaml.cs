using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using LaserControllerApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
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

                // Register the dialog service FIRST
                services.AddSingleton<IDialogService, DialogService>();

                // Get the main dispatcher
                var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                if (dispatcherQueue == null)
                {
                    Debug.WriteLine("Failed to get DispatcherQueue for current thread");
                    throw new InvalidOperationException("DispatcherQueue is not available");
                }

                // Register Services
                services.AddSingleton<SerialPortManager>(provider =>
                {
                    var serialPortManager = new SerialPortManager();
                    serialPortManager.Initialize(dispatcherQueue);
                    return serialPortManager;
                });
                services.AddSingleton<StatusService>(provider => new StatusService(dispatcherQueue));
                services.AddSingleton<LoggerService>(provider => new LoggerService(dispatcherQueue));
                services.AddSingleton<LaserSettingsValidator>();

                // Register ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<LaserControlDashboardViewModel>(provider =>
                    new LaserControlDashboardViewModel(
                        provider.GetRequiredService<SerialPortManager>(),
                        provider.GetRequiredService<MainViewModel>(),
                        provider.GetRequiredService<IDialogService>()));
              
                services.AddSingleton<EnergyMonitorViewModel>();              
                services.AddSingleton<ShutterViewModel>();
                services.AddSingleton<CustomCommandsViewModel>();
                services.AddSingleton<InterlockStatusViewModel>();

                // Register MainWindow
                services.AddSingleton<MainWindow>();

                // Register Pages
                services.AddTransient<LaserControlDashboard>();
                services.AddTransient<EnergyMonitorPage>();              
                services.AddTransient<ShutterPage>();             
                services.AddTransient<InterlockStatusPage>();
                services.AddTransient<CustomPage>();
                services.AddTransient<EnergyPage>();
                services.AddTransient<SettingsPage>();

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

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                Debug.WriteLine("=== OnLaunched Starting ===");
                m_window = Services.GetRequiredService<MainWindow>();
                m_window.Activate();

                // ✅ Initialize DialogService with a valid XamlRoot from the window's content
                var dialogService = Services.GetRequiredService<IDialogService>();

                // Wait for the window to be fully loaded before initializing
                m_window.Activated += (sender, e) =>
                {
                    if (m_window.Content is FrameworkElement root && root.XamlRoot != null)
                    {
                        dialogService.Initialize(root.XamlRoot);
                        Debug.WriteLine("DialogService initialized with XamlRoot (OnLaunched).");
                    }
                    else
                    {
                        Debug.WriteLine("DialogService initialization failed: XamlRoot is null.");
                    }
                };

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

            // Optionally log to LoggerService
            var loggerService = Services.GetService<LoggerService>();
            loggerService?.LogError($"Unhandled exception: {e.Exception}");
        }
    }
}
