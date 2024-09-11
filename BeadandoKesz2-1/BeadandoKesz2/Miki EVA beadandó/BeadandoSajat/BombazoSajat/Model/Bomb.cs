using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombazoSajat.Model
{
	public class Bomb
	{
		public (int x, int y) Position { get; set; }
		private int _timer;
		public int Timer
		{
			get => _timer;
			set
			{
				if (value >= 0)
                    _timer = value;
			}
		}

		public Bomb((int x, int y) position)
		{
			Position = position;
            _timer = 3;
        }

    }
}
