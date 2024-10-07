namespace Bomberman.Models.Characters
{
    public abstract class Character
    {
        #region Variables

        protected (int x, int y) _pos;
        protected Direction _direction;

        #endregion

        #region Properties
        public (int x, int y) Pos
        { 
            get => _pos;
            set => _pos = value; 
        }
        public Direction Direction 
        {
            get => _direction;
            set => _direction = value; 
        }

        #endregion

        #region Constructors

        protected Character()
        {
            _pos = (0, 0);
            _direction = Direction.Up;
        }
        protected Character((int x, int y) pos, Direction direction)
        {
            _pos = pos;
            _direction = direction;
        }

        #endregion
    }
}
