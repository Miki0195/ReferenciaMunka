using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ELTE.TravelAgency.Admin.ViewModel
{
    /// <summary>
    /// Épületkép átalakító típusa.
    /// </summary>
    public class BuildingImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is byte[]))
                return Binding.DoNothing;

            try
            {
                using (MemoryStream stream = new MemoryStream((byte[])value)) // a képet a memóriába egy adatfolyamba helyezzük
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // a betöltött tartalom a képbe kerül
                    image.StreamSource = stream; // átalakítjuk bitképpé
                    image.EndInit();
                    return image; // visszaadjuk a létrehozott bitképet
                }
            }
            catch
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
