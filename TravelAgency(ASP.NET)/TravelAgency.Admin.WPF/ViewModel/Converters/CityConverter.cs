using ELTE.TravelAgency.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ELTE.TravelAgency.Admin.ViewModel
{
    /// <summary>
    /// Város átalakító.
    /// </summary>
    public class CityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // ellenőrizzük az értéket
            if (value == null || !(value is int))
                return Binding.DoNothing; // ha nem megfelelő, nem csinálunk semmit

            int id = (int)value;
            CityDto? city = ((IEnumerable<CityDto>)parameter).FirstOrDefault(c => c.Id == id);

            if (city == null)
                return Binding.DoNothing;

            return city;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // ellenőrizzük az értéket
            if (value == null || !(value is CityDto))
                return DependencyProperty.UnsetValue; // ha nem megfelelő, nem csinálunk semmit

            return ((CityDto)value).Id;
        }
    }
}
