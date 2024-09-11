using BombazoSajat.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BeadandoSajatWPF.ViewModel
{
    public class ViewModelEnemy
    {
        private Enemy _enemy;
        private Bitmap _bitmap;
        private ImageSource _image;
        private int _size;

        public ViewModelEnemy(Enemy enemy, Bitmap bitmap, ImageSource image, int size)
        {
            _enemy = enemy;
            _bitmap = bitmap;
            _image = image;
            _size = size;
        }
        public void RefreshImage()
        {
            _image = Imaging.CreateBitmapSourceFromHBitmap(_bitmap.GetHbitmap(),
                                 IntPtr.Zero,
                                 Int32Rect.Empty,
                                 BitmapSizeOptions.FromEmptyOptions());
        }
        public Thickness Margin
        {
            get { return new Thickness(_enemy.Position.x * Size, _enemy.Position.y * Size, 0, 0); }
        }
        public Enemy Enemy
        {
            get { return _enemy; }
            set { _enemy = value; }
        }
        public Bitmap Bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; }             
        }
        public ImageSource Image
        {
            get { return _image; }
            set { _image = value; }
        }
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }
    }
}
