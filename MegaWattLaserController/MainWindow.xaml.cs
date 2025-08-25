using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace LaserControllerApp
{
    public sealed partial class MainWindow : Window
    {
        // Singleton instance of SerialPortManager for serial communication
        private readonly SerialPortManager serialPortManager = SerialPortManager.Instance;

        public MainWindow()
        {
            this.InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(ShutterPage));
            serialPortManager.ConnectionStatusChanged += SerialPortManager_ConnectionStatusChanged;
            UpdateStatus("Disconnected");
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag.ToString())
                {
                    case "ShutterPage":
                        ContentFrame.Navigate(typeof(ShutterPage));
                        break;
                    case "EnergyPage":
                        ContentFrame.Navigate(typeof(EnergyPage));
                        break;
                    case "CustomPage":
                        ContentFrame.Navigate(typeof(CustomPage));
                        break;
                    case "SettingsPage":
                        ContentFrame.Navigate(typeof(SettingsPage));
                        break;
                }
            }
        }

        private void SerialPortManager_ConnectionStatusChanged(object sender, string status)
        {
            UpdateStatus(status);
        }

        private void UpdateStatus(string status)
        {
            StatusTextBlock.Text = status;
        }
    }
}