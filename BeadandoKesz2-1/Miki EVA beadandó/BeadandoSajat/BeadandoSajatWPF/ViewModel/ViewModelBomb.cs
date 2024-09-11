using BombazoSajat.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BeadandoSajatWPF.ViewModel
{
    public class ViewModelBomb
    {
        private Bomb _bomb;
        private ImageSource _image;
        private int _size;
        public ViewModelBomb(Bomb bomb, ImageSource image, int size)
        {
            _bomb = bomb;
            _image = image;
            _size = size;
        }
        public Thickness Margin
        {
            get { return new Thickness(_bomb.Position.x * Size, _bomb.Position.y * Size, 0, 0); }
        }
        public Bomb Bomb
        {
            get { return _bomb; }
            set { _bomb = value; }
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
