using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using LaserControllerApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.Graphics;

namespace LaserControllerApp
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        private AppWindow appWindow;

        public MainWindow()
        {
            this.InitializeComponent();

            // Resolve ViewModel from DI container
            try
            {
                ViewModel = App.Services.GetRequiredService<MainViewModel>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to resolve MainViewModel: {ex.Message}");
                throw;
            }

            // Set the DataContext for Binding on the RootGrid instead of the Window
            if (RootGrid != null)
            {
                RootGrid.DataContext = ViewModel;
            }

            // Subscribe to connection status changes
            ViewModel.ConnectionStatusChanged += OnConnectionStatusChanged;

            // Load LaserControlDashboard directly
            try
            {
                if (DashboardContent != null)
                {
                    DashboardContent.Content = App.Services.GetRequiredService<LaserControlDashboard>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load dashboard: {ex.Message}");
            }

            // Initialize dialog service once layout is ready using the RootGrid
            if (RootGrid != null)
            {
                RootGrid.Loaded += (s, e) => InitializeDialogService();
            }

            // Setup AppWindow sizing & placement
            InitializeAppWindow();
        }

        private void InitializeDialogService()
        {
            // Use RootGrid to get XamlRoot since Window doesn't have it directly
            var rootElement = RootGrid;
            if (rootElement?.XamlRoot != null)
            {
                try
                {
                    var dialogService = App.Services.GetRequiredService<IDialogService>();
                    dialogService.Initialize(rootElement.XamlRoot);
                    Debug.WriteLine("[MainWindow] DialogService initialized.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainWindow] Failed to initialize dialog service: {ex}");
                }
            }
            else
            {
                Debug.WriteLine("[MainWindow] DialogService not initialized (XamlRoot null).");
            }
        }

        private void InitializeAppWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                int defaultWidth = 1200;
                int defaultHeight = 800;
                int minWidth = 1000;
                int minHeight = 700;

                // Resize to default startup size
                appWindow.Resize(new SizeInt32(defaultWidth, defaultHeight));

                // Center window
                CenterWindow(appWindow, windowId, defaultWidth, defaultHeight);

                // Configure presenter
                if (appWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.IsResizable = true;
                    presenter.SetBorderAndTitleBar(true, true);
                }

                // Enforce minimum size
                this.SizeChanged += (s, e) =>
                {
                    if (e.Size.Width < minWidth || e.Size.Height < minHeight)
                    {
                        appWindow.Resize(new SizeInt32(
                            Math.Max(minWidth, (int)e.Size.Width),
                            Math.Max(minHeight, (int)e.Size.Height)
                        ));
                    }
                };

                // Re-center when restored from maximized
                this.Activated += (s, e) =>
                {
                    if (appWindow.Presenter is OverlappedPresenter p &&
                        p.State == OverlappedPresenterState.Restored)
                    {
                        CenterWindow(appWindow, windowId,
                            (int)this.Bounds.Width, (int)this.Bounds.Height);
                    }

                    // Also make sure dialog service is initialized
                    InitializeDialogService();
                };
            }
        }

        private static void CenterWindow(AppWindow appWindow, WindowId windowId, int width, int height)
        {
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            if (displayArea != null)
            {
                int x = (displayArea.WorkArea.Width - width) / 2;
                int y = (displayArea.WorkArea.Height - height) / 2;
                appWindow.MoveAndResize(new RectInt32(x, y, width, height));
            }
        }

        private void OnConnectionStatusChanged(object sender, bool isConnected)
        {
            // Optional: react to connection changes if needed
        }

        private void DismissErrorButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.HasError = false;
            ViewModel.ErrorMessage = string.Empty;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            if (ViewModel != null)
            {
                ViewModel.ConnectionStatusChanged -= OnConnectionStatusChanged;
            }
        }
    }
}