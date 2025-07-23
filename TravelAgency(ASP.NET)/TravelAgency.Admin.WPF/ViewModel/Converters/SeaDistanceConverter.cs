using System;
using System.Windows;
using System.Windows.Data;

namespace ELTE.TravelAgency.Admin.ViewModel
{
    /// <summary>
    /// Tengerpart távolság átalakító.
    /// </summary>
    public class SeaDistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is int)) // ellenőrizzük az értéket
                return Binding.DoNothing; // ha nem megfelelő, nem csinálunk semmit

            int distance = (int)value;
            if (distance > 1000) // megfelelő utótaggal látjuk el
                return (distance / 1000) + " km";
            else
                return distance + " m";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string)) // ellenőrizzük az értéket
                return DependencyProperty.UnsetValue; // ha nem megfelelő, nem tudjuk az értéket beállítani

            try
            {
                string distanceString = (string)value;
                double distance;
                if (distanceString.EndsWith(" km")) // megpróbáljuk konvertálni az értéket
                    distance = double.Parse(distanceString.Substring(0, distanceString.Length - 3)) * 1000; // figyelembe vesszük, hogy törtet is beírhat 
                else
                    distance = double.Parse(distanceString.Substring(0, distanceString.Length - 2));

                if (distance < 0) // negatív sem lehet az érték
                    return DependencyProperty.UnsetValue;

                return (int)distance;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
