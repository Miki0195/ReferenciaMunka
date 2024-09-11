using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombazoSajat.Model
{
    public abstract class Charachter
    {
        public Bitmap Bitmap { get; set; }
        public (int x, int y) Position { get; set; }
        protected Direction _direction;

        protected BombazoGameModel _gameModel;
        public Charachter((int x, int y) position, BombazoGameModel gameModel, Bitmap bitmap)
        {
            this.Bitmap = bitmap;
            this.Position = position;
            this._direction = Direction.UP;
            this._gameModel = gameModel;
        }
        public bool IsBlown(Bomb b)
        {
            return b.Position.x - 1 <= Position.x && Position.x <= b.Position.x + 1
                && b.Position.y - 1 <= Position.y && Position.y <= b.Position.y + 1;
		}

        protected void Move()
        {
            switch (_direction)
            {
                case Direction.UP:
                    Position = (Position.x, Position.y - 1);
                    break;
                case Direction.DOWN:
                    Position = (Position.x, Position.y + 1);
                    break;
                case Direction.RIGHT:
                    Position = (Position.x + 1, Position.y);
                    break;
                default: //Direction.LEFT:
                    Position = (Position.x - 1, Position.y);
                    break;
            }
        }
    }
}
