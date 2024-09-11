using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombazoSajat.Model
{
    public class Enemy : Charachter
    {
        private static Random random = new Random();
        public bool ToBeDestroyed { get; set; }

        public Enemy((int x, int y) position, BombazoGameModel model, Bitmap bitmap) : base(position, model, bitmap)
        {
            ToBeDestroyed = false;
        }

        public void EnemyMove()
        {
            (int x, int y) prevposition = Position;
            Move();

            if (Position.x < 0 || Position.x >= _gameModel._bombazoMap.Map.Length
                || Position.y < 0 || Position.y >= _gameModel._bombazoMap.Map.Length
                || _gameModel._bombazoMap.Map[Position.y][Position.x].IsRock())
            {
                Position = prevposition;
                RandomDirection();
                Move();
            }
        }

        public void RandomDirection()
        {
            Direction prevDirection = _direction;

            List<Direction> directions = new List<Direction>() { Direction.UP, Direction.DOWN, Direction.RIGHT, Direction.LEFT };
            if (Position.x + 1 >= _gameModel._bombazoMap.Map.Length || _gameModel._bombazoMap.Map[Position.y][Position.x + 1].IsRock())
                directions.Remove(Direction.RIGHT);
            if (Position.x - 1 < 0 || _gameModel._bombazoMap.Map[Position.y][Position.x - 1].IsRock())
                directions.Remove(Direction.LEFT);
            if (Position.y + 1 >= _gameModel._bombazoMap.Map.Length || _gameModel._bombazoMap.Map[Position.y + 1][Position.x].IsRock())
                directions.Remove(Direction.DOWN);
            if (Position.y - 1 < 0 || _gameModel._bombazoMap.Map[Position.y - 1][Position.x].IsRock())
                directions.Remove(Direction.UP);

            _direction = directions[random.Next(0, directions.Count)];

            RotateImage(prevDirection);
        }

        private void RotateImage(Direction prev)
        {
            switch (prev)
            {
                case Direction.UP:
                    switch (_direction)
                    {
                        case Direction.UP:
                            break;
                        case Direction.DOWN:
                            Bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case Direction.RIGHT:
                            Bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        default: //Direction.LEFT
                            Bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                    }
                    break;
                case Direction.DOWN:
                    switch (_direction)
                    {
                        case Direction.UP:
                            Bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case Direction.DOWN:
                            break;
                        case Direction.RIGHT:
                            Bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        default: //Direction.LEFT
                            Bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                    }
                    break;
                case Direction.RIGHT:
                    switch (_direction)
                    {
                        case Direction.UP:
                            Bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        case Direction.DOWN:
                            Bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case Direction.RIGHT:
                            break;
                        default: //Direction.LEFT
                            Bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                    }
                    break;
                default: //Direction.LEFT
                    switch (_direction)
                    {
                        case Direction.UP:
                            Bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case Direction.DOWN:
                            Bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        case Direction.RIGHT:
                            Bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        default: //Direction.LEFT
                            break;
                    }
                    break;
            }
        }
    
    }
}
