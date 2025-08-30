using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;   // <-- fix

namespace MikoMe.Helpers
{
    public class AlternatingRowConverter : IValueConverter
    {
        private static readonly SolidColorBrush WhiteBrush =
            new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush LightGrayBrush =
            new SolidColorBrush(Color.FromArgb(255, 248, 248, 248));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int index)
            {
                return index % 2 == 0 ? WhiteBrush : LightGrayBrush;
            }
            return WhiteBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
