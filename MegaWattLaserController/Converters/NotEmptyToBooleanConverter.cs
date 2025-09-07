using Microsoft.UI.Xaml.Data;
using System;

namespace LaserControllerApp.Converters
{
    public class NotEmptyToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string str)
                return !string.IsNullOrEmpty(str);

            if (value is System.Collections.ICollection collection)
                return collection.Count > 0;

            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}