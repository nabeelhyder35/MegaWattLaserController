using Microsoft.UI.Xaml.Data;
using System;

namespace LaserControllerApp.Converters
{
    public class BooleanToPauseResumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isRunning)
            {
                return isRunning ? "Pause" : "Resume";
            }
            return "Pause";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}