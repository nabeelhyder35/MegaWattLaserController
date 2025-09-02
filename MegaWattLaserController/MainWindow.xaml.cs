using LaserControllerApp.Services;
using LaserControllerApp.ViewModels;
using LaserControllerApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Linq;

namespace LaserControllerApp
{
    public sealed partial class MainWindow : Window
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly MainViewModel _viewModel;
        private Grid _mainGrid;
        private StackPanel _statusPanel;
        private Ellipse _connectionStatusEllipse;
        private TextBlock _statusTextBlock;
        private ComboBox _portComboBox;
        private ComboBox _baudRateComboBox;
        private Button _refreshPortsButton;
        private Button _connectButton;
        private Button _disconnectButton;
        private Frame _contentFrame;
        private Grid _contentGrid;
        private StackPanel _navPanel;

        public MainViewModel ViewModel => _viewModel;

        public MainWindow(SerialPortManager serialPortManager, MainViewModel viewModel)
        {
            try
            {
                Debug.WriteLine("MainWindow constructor started");

                _serialPortManager = serialPortManager ?? throw new ArgumentNullException(nameof(serialPortManager));
                _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
                Debug.WriteLine("Dependencies injected");

                // Create UI elements
                CreateUIElements();
                Debug.WriteLine("UI elements created");

                // Initialize SerialPortManager with dispatcher
                _serialPortManager.Initialize(this.DispatcherQueue);
                Debug.WriteLine("SerialPortManager initialized");

                // Set DataContext for binding
                _mainGrid.DataContext = _viewModel;
                Debug.WriteLine("DataContext set");

                // Initialize UI components
                InitializeUI();
                Debug.WriteLine("UI initialized");

                // Unsubscribe events when window closes
                this.Closed += MainWindow_Closed;
                Debug.WriteLine("Closed event registered");

                // Set window size after window is loaded
                this.Activated += MainWindow_Activated;
                Debug.WriteLine("Activated event registered");

                Debug.WriteLine("MainWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainWindow constructor failed: {ex}");
                throw;
            }
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            try
            {
                Debug.WriteLine("MainWindow activated, setting size");
                this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
                this.Activated -= MainWindow_Activated; // Remove handler after first use
                Debug.WriteLine("Window size set successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting window size: {ex.Message}");
            }
        }

        private void CreateUIElements()
        {
            try
            {
                Debug.WriteLine("Creating UI elements...");

                // Create main grid
                _mainGrid = new Grid();
                _mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                _mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                // Create status panel
                _statusPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Padding = new Thickness(10)
                };

                // Create connection status indicator
                _connectionStatusEllipse = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = new SolidColorBrush(Microsoft.UI.Colors.Red),
                    Margin = new Thickness(0, 0, 5, 0)
                };

                _statusTextBlock = new TextBlock
                {
                    Text = "Disconnected",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                // Create COM port combo box
                _portComboBox = new ComboBox
                {
                    Width = 150,
                    Margin = new Thickness(0, 0, 5, 0),
                    Header = "Port",
                    PlaceholderText = "Select port"
                };

                // Create baud rate combo box
                _baudRateComboBox = new ComboBox
                {
                    Width = 100,
                    Margin = new Thickness(0, 0, 5, 0),
                    Header = "Baud Rate"
                };

                // Add baud rate options
                string[] baudRates = { "9600", "19200", "38400", "57600", "115200" };
                foreach (var rate in baudRates)
                {
                    _baudRateComboBox.Items.Add(new ComboBoxItem { Content = rate });
                }
                _baudRateComboBox.SelectedIndex = 0;

                // Create buttons
                _refreshPortsButton = new Button
                {
                    Content = "Refresh",
                    Margin = new Thickness(0, 0, 5, 0)
                };
                _refreshPortsButton.Click += RefreshPorts_Click;

                _connectButton = new Button
                {
                    Content = "Connect",
                    Margin = new Thickness(0, 0, 5, 0)
                };
                _connectButton.Click += Connect_Click;

                _disconnectButton = new Button
                {
                    Content = "Disconnect",
                    IsEnabled = false
                };
                _disconnectButton.Click += Disconnect_Click;

                // Add elements to status panel
                _statusPanel.Children.Add(_connectionStatusEllipse);
                _statusPanel.Children.Add(_statusTextBlock);
                _statusPanel.Children.Add(_portComboBox);
                _statusPanel.Children.Add(_baudRateComboBox);
                _statusPanel.Children.Add(_refreshPortsButton);
                _statusPanel.Children.Add(_connectButton);
                _statusPanel.Children.Add(_disconnectButton);

                // Create content frame and navigation panel
                _contentFrame = new Frame();
                _contentGrid = new Grid();
                _contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
                _contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                _navPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Padding = new Thickness(10),
                    Spacing = 8
                };

                // Create navigation buttons
                var homeBtn = new Button { Content = "Home", Tag = "HOME", HorizontalAlignment = HorizontalAlignment.Stretch };
                homeBtn.Click += NavButton_Click;
                var systemBtn = new Button { Content = "System Settings", Tag = typeof(Views.SystemSettingsPage), HorizontalAlignment = HorizontalAlignment.Stretch };
                systemBtn.Click += NavButton_Click;
                var energyBtn = new Button { Content = "Energy", Tag = typeof(Views.EnergyPage), HorizontalAlignment = HorizontalAlignment.Stretch };
                energyBtn.Click += NavButton_Click;
                var voltageBtn = new Button { Content = "Voltage", Tag = typeof(Views.VoltagePage), HorizontalAlignment = HorizontalAlignment.Stretch };
                voltageBtn.Click += NavButton_Click;
                var waveformBtn = new Button { Content = "Waveform", Tag = typeof(Views.WaveformPage), HorizontalAlignment = HorizontalAlignment.Stretch };
                waveformBtn.Click += NavButton_Click;
                var pulseBtn = new Button { Content = "Pulse Settings", Tag = typeof(Views.PulseSettingsPage), HorizontalAlignment = HorizontalAlignment.Stretch };
                pulseBtn.Click += NavButton_Click;
                var shutterBtn = new Button { Content = "Shutter", Tag = typeof(Views.ShutterPage), HorizontalAlignment = HorizontalAlignment.Stretch };
                shutterBtn.Click += NavButton_Click;
                var interlockBtn = new Button { Content = "Interlock", Tag = typeof(Views.InterlockStatusPage), HorizontalAlignment = HorizontalAlignment.Stretch };
                interlockBtn.Click += NavButton_Click;
                var energyMonBtn = new Button { Content = "Energy Monitor", Tag = typeof(Views.EnergyMonitorPage), HorizontalAlignment = HorizontalAlignment.Stretch };
                energyMonBtn.Click += NavButton_Click;
                var customBtn = new Button { Content = "Custom", Tag = typeof(Views.CustomPage), HorizontalAlignment = HorizontalAlignment.Stretch };
                customBtn.Click += NavButton_Click;
                var settingsBtn = new Button { Content = "Settings", Tag = typeof(Views.SettingsPage), HorizontalAlignment = HorizontalAlignment.Stretch };
                settingsBtn.Click += NavButton_Click;

                _navPanel.Children.Add(homeBtn);
                _navPanel.Children.Add(systemBtn);
                _navPanel.Children.Add(energyBtn);
                _navPanel.Children.Add(voltageBtn);
                _navPanel.Children.Add(waveformBtn);
                _navPanel.Children.Add(pulseBtn);
                _navPanel.Children.Add(shutterBtn);
                _navPanel.Children.Add(interlockBtn);
                _navPanel.Children.Add(energyMonBtn);
                _navPanel.Children.Add(customBtn);
                _navPanel.Children.Add(settingsBtn);

                Grid.SetColumn(_navPanel, 0);
                Grid.SetColumn(_contentFrame, 1);
                _contentGrid.Children.Add(_navPanel);
                _contentGrid.Children.Add(_contentFrame);

                // Add elements to grid
                Grid.SetRow(_statusPanel, 0);
                Grid.SetRow(_contentGrid, 1);
                _mainGrid.Children.Add(_statusPanel);
                _mainGrid.Children.Add(_contentGrid);

                // Set window content
                this.Content = _mainGrid;
                this.AppWindow.Title = "Laser Controller App";

                Debug.WriteLine("UI elements created successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating UI elements: {ex}");
                throw;
            }
        }

        private void InitializeUI()
        {
            try
            {
                Debug.WriteLine("Initializing UI...");

                // Load available ports
                RefreshPortsList();

                // Subscribe to connection status changes
                _serialPortManager.ConnectionStatusChanged += SerialPortManager_ConnectionStatusChanged;

                // Navigation error handling
                _contentFrame.NavigationFailed += ContentFrame_NavigationFailed;

                // Default navigation
                try
                {
                    _contentFrame.Content = App.Services.GetRequiredService<HomeScreen>();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load HomeScreen: {ex.Message}");
                }

                Debug.WriteLine("UI initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing UI: {ex}");
            }
        }

        private void RefreshPortsList()
        {
            try
            {
                _portComboBox.Items.Clear();
                var ports = _serialPortManager.GetAvailablePorts();
                foreach (var port in ports)
                {
                    _portComboBox.Items.Add(port);
                }

                if (_portComboBox.Items.Count > 0)
                {
                    _portComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing ports: {ex.Message}");
            }
        }

        private void SerialPortManager_ConnectionStatusChanged(object sender, bool isConnected)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    _connectionStatusEllipse.Fill = new SolidColorBrush(
                        isConnected ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
                    _statusTextBlock.Text = isConnected ? "Connected" : "Disconnected";
                    _connectButton.IsEnabled = !isConnected;
                    _disconnectButton.IsEnabled = isConnected;
                    _portComboBox.IsEnabled = !isConnected;
                    _baudRateComboBox.IsEnabled = !isConnected;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating connection status: {ex.Message}");
                }
            });
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn)
                {
                    var tag = btn.Tag;
                    if (tag is Type pageType)
                    {
                        // Resolve the page via DI to support constructor injection
                        var pageObj = App.Services.GetService(pageType) as Page;
                        if (pageObj != null)
                        {
                            _contentFrame.Content = pageObj;
                        }
                        else
                        {
                            // Fallback to type navigation
                            _contentFrame.Navigate(pageType);
                        }
                    }
                    else if (tag is string s && s == "HOME")
                    {
                        _contentFrame.Content = App.Services.GetRequiredService<HomeScreen>();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ShowMessageAsync("Navigation Error", ex.Message);
            }
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            _ = ShowMessageAsync("Navigation Failed", e.Exception?.Message ?? "Unknown error.");
        }

        private void RefreshPorts_Click(object sender, RoutedEventArgs e)
        {
            RefreshPortsList();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (_portComboBox.SelectedItem == null)
            {
                await ShowMessageAsync("Error", "Please select a port.");
                return;
            }

            var portName = _portComboBox.SelectedItem.ToString();
            var baudRate = int.Parse((_baudRateComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "9600");

            try
            {
                await _serialPortManager.ConnectAsync(portName, baudRate);
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Connection Error", ex.Message);
            }
        }

        private async void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _serialPortManager.DisconnectAsync();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Disconnection Error", ex.Message);
            }
        }

        private async System.Threading.Tasks.Task ShowMessageAsync(string title, string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing message: {ex.Message}");
            }
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            try
            {
                _serialPortManager.ConnectionStatusChanged -= SerialPortManager_ConnectionStatusChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cleaning up: {ex.Message}");
            }
        }
    }
}