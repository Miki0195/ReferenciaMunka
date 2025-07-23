using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ELTE.TravelAgency.Shared.Models;

namespace ELTE.TravelAgency.Admin.ViewModel
{
    /// <summary>
    /// Tengerpart típus átalakító típusa.
    /// </summary>
    public class ShoreTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ellenőrizzük az értéket
            if (value == null || !(value is ShoreTypeDto))
                return Binding.DoNothing; // ha nem megfelelő, nem csinálunk semmit

            // ellenőrizzük a paramétert
            if (parameter == null || !(parameter is IEnumerable<string>))
                return Binding.DoNothing;

            List<string> shoreNames = ((IEnumerable<string>)parameter).ToList();
            int index = (int)value;

            if (index < 0 || index >= shoreNames.Count)
                return Binding.DoNothing;

            return shoreNames[index];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ellenőrizzük az értéket
            if (value == null || !(value is string))
                return DependencyProperty.UnsetValue; // ha nem megfelelő, nem csinálunk semmit

            // ellenőrizzük a paramétert
            if (parameter == null || !(parameter is IEnumerable<string>))
                return Binding.DoNothing;

            List<string> shoreNames = ((IEnumerable<string>)parameter).ToList();
            string name = (string)value;

            // megkeressük a nevet
            if (!shoreNames.Contains(name))
                return DependencyProperty.UnsetValue;

            return (ShoreTypeDto)shoreNames.IndexOf(name);
        }
    }
}
