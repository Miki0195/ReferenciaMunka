namespace Bomberman.Models.Characters
{
    public class Monster : Character
    {
        #region Variables

        private double _randomDirectionChangeTime;
        private double _moveTimer;

        #endregion

        #region Properties

        public double RandomDirectionChangeTime
        {
            get { return _randomDirectionChangeTime; }
            set { _randomDirectionChangeTime = value; }
        }
        public double MoveTimer
        {
            get { return _moveTimer; }
            set { _moveTimer = value; }
        }

        #endregion

        #region Constructors

        public Monster(int randomDirectionChangeTime, (int x, int y) pos, Direction direction) : base(pos, direction)
        {
            _randomDirectionChangeTime = randomDirectionChangeTime;
            _moveTimer = 0;
        }

        #endregion
    }
}
