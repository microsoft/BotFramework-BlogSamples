// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TrivaApp.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string && (parameter as string).Equals("invert", StringComparison.CurrentCultureIgnoreCase))
            {
                if (value is bool)
                {
                    value = !(bool)value;
                }
            }

            if (value is bool && (bool)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
