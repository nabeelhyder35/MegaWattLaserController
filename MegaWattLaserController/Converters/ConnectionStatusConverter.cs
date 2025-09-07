using Microsoft.UI.Xaml.Data;
using System;

namespace LaserControllerApp.Converters
{
    public class ConnectionStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isConnected)
            {
                return isConnected ? "Connected to laser system - Ready" : "System ready - Connect to begin";
            }
            return "System status unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}