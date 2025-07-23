using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ELTE.TravelAgency.Admin.ViewModel
{
    /// <summary>
    /// Jellemező átalakító típusa.
    /// </summary>
    public class FeatureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ellenőrizzük az értéket
            if (value == null || !(value is int))
                return Binding.DoNothing; // ha nem megfelelő, nem csinálunk semmit

            // ellenőrizzük a paramétert
            if (parameter == null || !(parameter is IList<string>))
                return Binding.DoNothing;

            return ((IList<string>)parameter)[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // a visszaalakítással nem törődünk
            return DependencyProperty.UnsetValue;
        }
    }
}
