using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace LaserControllerApp.Converters
{
    public class ArmDisarmConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isArmed)
            {
                return isArmed ? "Disarm Laser" : "Arm Laser";
            }
            return "Arm Laser";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ArmDisarmColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isArmed)
            {
                // Return different colors for armed vs disarmed state
                return isArmed ? new SolidColorBrush(Microsoft.UI.Colors.Red) :
                                new SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            return new SolidColorBrush(Microsoft.UI.Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}