using ELTE.TravelAgency.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ELTE.TravelAgency.Admin.ViewModel
{
    /// <summary>
    /// Város gyűjtemény átalakító.
    /// </summary>
    public class CityCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // itt a városok listájából állítunk elő szöveget
            if (value == null || !(value is IEnumerable<CityDto>)) // ellenőrizzük az értéket
                return Binding.DoNothing; // ha nem megfelelő, nem csinálunk semmit

            return ((IEnumerable<CityDto>)value).Select(city => city.Name);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // a visszalakítással nem törődünk
            return DependencyProperty.UnsetValue;
        }
    }
}
