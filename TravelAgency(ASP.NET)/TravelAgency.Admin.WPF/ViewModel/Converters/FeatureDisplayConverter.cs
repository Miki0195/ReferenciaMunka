using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Linq;
using System.Windows;
using ELTE.TravelAgency.Shared.Models;

namespace ELTE.TravelAgency.Admin.ViewModel
{
    /// <summary>
    /// Jellemző megjelenítés átalakító típusa.
    /// </summary>
    public class FeatureDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // ellenőrizzük az értéket
            if (value == null || !(value is IList<FeatureDto>))
                return Binding.DoNothing; // ha nem megfelelő, nem csinálunk semmit

            // ellenőrizzük a paramétert
            if (parameter == null || !(parameter is IList<string>))
                return Binding.DoNothing;

            IList<string> featureNames = (IList<string>)parameter;
            IList<FeatureDto> features = (IList<FeatureDto>)value;
            string result = string.Empty;

            for (int featureIndex = 0; featureIndex < features.Count && featureIndex < featureNames.Count; featureIndex++)
                if (features[featureIndex].IsAvailable)
                    result += featureNames[features[featureIndex].Id] + ", "; // a jellemzőket összerakjuk

            if (result.Length > 0)
                return result.Substring(0, result.Length - 2); // a megfelelő szöveget visszaadjuk
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // a visszaalakítással nem törődünk
            return DependencyProperty.UnsetValue;
        }
    }
}
