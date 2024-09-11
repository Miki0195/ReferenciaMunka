using BombazoSajat.Persistence;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombazoSajat.Model
{
    public class Player : Charachter
    {
        public Player((int x, int y) position, BombazoGameModel model, Bitmap bitmap) : base(position, model, bitmap)
        {
            
        }

        private void TryMove((int x, int y) position, Direction direction)
        {
			_direction = direction;

            (int x, int y) nextposition = (Position.x + position.x, Position.y + position.y);

			int n = _gameModel._bombazoMap.Map.Length;

            if (nextposition.x >= 0 && nextposition.x < n
                && nextposition.y >= 0 && nextposition.y < n
                && !_gameModel._bombazoMap.Map[nextposition.y][nextposition.x].IsRock())
			{
				Position = nextposition;
                _gameModel.CheckGameStatus();
            }
        }
		public void Move(char key)
		{
            if (_gameModel.Paused)
				return;

			switch (key)
			{
				
				case 'A':
					this.TryMove((-1, 0), Direction.LEFT);
                    break;
				case 'W':
					this.TryMove((0, -1), Direction.UP);
					break;
				case 'S':
					this.TryMove((0, 1), Direction.DOWN);
					break;
				case 'D':
					this.TryMove((1, 0), Direction.RIGHT);
					break;
				case ' ':
					_gameModel._bombazoMap._bombs.Add(new Bomb(this.Position));
					break;
				default:
					break;
			}
		}
	}
}
